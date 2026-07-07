using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControllerM4 : MonoBehaviour
{
    public enum Weapons
    {
        None,
        Pistol,
        Rifle,
        MiniGun
    }
    Weapons weapons = Weapons.None;
    
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] GameObject pistol, rifle, miniGun;

    [SerializeField] Image pistolUI, rifleUI, miniGunUI, cusror;

    bool isPistol, isRifle, isMiniGun;
    float currentSpeed;

    [SerializeField] Rigidbody rb;
    Vector3 direction;

    [SerializeField] float shiftSpeed = 10f;
    [SerializeField] float jumpForce = 7f;

    int health;

    [SerializeField] ThirdPersonCamera cameraScript;
    float stamina = 5f;

    bool isGrounded = true;

    [SerializeField] Animator anim; 
    // Hacemos referencia al AudioSource
    [SerializeField] AudioSource characterSounds;
    // Hacemos referencia al clip de sonido del salto
    [SerializeField] AudioClip jump;

    void Start()
    {        
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        currentSpeed = movementSpeed;
        health = 100;
        // ChangeHealth(-100);
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        direction = new Vector3(moveHorizontal, 0.0f, moveVertical);
        direction = transform.TransformDirection(direction);

        //Logica para el movimiento del personaje
        if(direction.x != 0 || direction.z != 0)
        {
            anim.SetBool("Run", true);
            // Si el AudioSource no está reproduciendo ningún sonido y estamos en el suelo, entonces...
            if(!characterSounds.isPlaying && isGrounded)
            {
                // Reproduciendo el sonido 
                characterSounds.Play();
            }
        }
        // Si el personaje no se está moviendo, entonces...
        if(direction.x == 0 && direction.z == 0)
        {
            anim.SetBool("Run", false);
            // Desactivando el sonido si el personaje se detiene
            characterSounds.Stop();
        }

        //logica de salto
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            isGrounded = false;
            anim.SetBool("Jump", true);
            // Desactivando el sonido de correr
            characterSounds.Stop();
            // Creando un AudioSource temporal para el salto
            AudioSource.PlayClipAtPoint(jump, transform.position);
        }
        // Logica para correr
        if(Input.GetKey(KeyCode.LeftShift))
        {
            if(stamina > 0)
            {
                stamina -= Time.deltaTime;
                currentSpeed = shiftSpeed;
            }
            else
            {
                currentSpeed = movementSpeed;
            }
        }
        else if (!Input.GetKey(KeyCode.LeftShift))
        {            
            stamina += Time.deltaTime;                      
            currentSpeed = movementSpeed;
        }
        if(stamina > 5f)
        {
            stamina = 5f;
        }
        else if (stamina < 0)
        {
            stamina = 0;
        }
        if(Input.GetKeyDown(KeyCode.Alpha1) && isPistol)
        {
            ChooseWeapon(Weapons.Pistol);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && isRifle)
        {
            ChooseWeapon(Weapons.Rifle);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && isMiniGun)
        {
            ChooseWeapon(Weapons.MiniGun);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ChooseWeapon(Weapons.None);
        }

    }
    public void ChangeHealth(int count)
    {
        // restando salud
        health -= count;
        // Si la salud llega a cero o menos, entonces...
        if (health <= 0)
        {
            //Activando la animación de muerte
            anim.SetBool("Die", true);
            //Quitar el arma
            ChooseWeapon(Weapons.None);
            //Deshabilitar el script PlayerController hace que el reproductor no pueda moverse
            this.enabled = false;
        }
    }
    
    public void ChooseWeapon(Weapons weapons)
    {
        anim.SetBool("Pistol", weapons == Weapons.Pistol);
        anim.SetBool("Assault", weapons == Weapons.Rifle);
        anim.SetBool("MiniGun", weapons == Weapons.MiniGun);
        anim.SetBool("NoWeapon", weapons == Weapons.None);
        pistol.SetActive(weapons == Weapons.Pistol);
        rifle.SetActive(weapons == Weapons.Rifle);
        miniGun.SetActive(weapons == Weapons.MiniGun);
        
        if(weapons != Weapons.None)
        {
            cusror.enabled = true;
        }
        else
        {
            cusror.enabled = false;
        }
    }
    
    void FixedUpdate()
    {
        rb.MovePosition(transform.position + direction * currentSpeed * Time.deltaTime);
    }
    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
        anim.SetBool("Jump", false);
    }
    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "pistol":
                if (!isPistol)
                {
                    isPistol = true;
                    pistolUI.color = Color.white;
                    ChooseWeapon(Weapons.Pistol);
                }
                break;

            case "rifle":
                if (!isRifle)
                {
                    isRifle = true;
                    rifleUI.color = Color.white;
                    ChooseWeapon(Weapons.Rifle);
                }
                break;

            case "minigun":
                if (!isMiniGun)
                {
                    isMiniGun = true;
                    miniGunUI.color = Color.white;
                    ChooseWeapon(Weapons.MiniGun);
                }
                break;
            default:
                break;
        }
        Destroy(other.gameObject);
    }    
}