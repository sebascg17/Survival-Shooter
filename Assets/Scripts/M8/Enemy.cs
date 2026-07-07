using Newtonsoft.Json.Linq;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviourPunCallbacks
{
    [SerializeField] protected int health;
    [SerializeField] protected float attackDistance;    
    [SerializeField] protected int damage;
    [SerializeField] protected float cooldown;
    protected GameObject player;

    protected GameObject[] players;

    protected Animator anim;
    protected Rigidbody rb;
    protected float distance;
    protected float timer;  
    public bool dead = false;
    [SerializeField] Image healthBar;

    public virtual void Move() 
    {
    }
    public virtual void Attack() 
    {
    }
    public void GetDamage(int count, int killerActorNumber) 
    {
        photonView.RPC("ChangeHealth", RpcTarget.All, count, killerActorNumber);
    } 
    public void ChangeHealth(int count)
    {
        ChangeHealth(count, -1);
    }
    [PunRPC]
    public void ChangeHealth(int count, int killerActorNumber)
    {
        if (dead)
        {
            return;
        }

        health -= count;
        float fillPercent = health / 100f;
        healthBar.fillAmount = fillPercent;
        if (health <= 0)
        {
            dead = true;
            GetComponent<Collider>().enabled = false;
            anim.enabled = true;
            anim.SetBool("Die", true);
            if (PhotonNetwork.IsMasterClient)
            {
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null && killerActorNumber > 0)
                {
                    gameManager.RegisterEnemyDefeat(killerActorNumber);
                    gameManager.ChangeEnemiesList();
                }
            }
            Destroy(GetDeathTarget(), 3f);
        }
    }
    GameObject GetDeathTarget()
    {
        return transform.root.gameObject;
    }
    void Start()
    {
        
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        CheckPlayers();
    }
    void CheckPlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        Invoke("CheckPlayers", 3f);
    }
    private void Update() 
    {
        float closestDistance = Mathf.Infinity;
        foreach (GameObject closestPlayer in players)
        {
            if (closestPlayer == null) { return; }
            float checkDistance = Vector3.Distance(closestPlayer.transform.position, transform.position);
            if (checkDistance < closestDistance)
            {
                if(closestPlayer.GetComponent<PlayerController>().dead == false) 
                {
                    player = closestPlayer;
                    closestDistance = checkDistance;
                }
                
            }
        }
        if (player != null)
        {            
            distance = Vector3.Distance(transform.position, player.transform.position);
            if (!dead)
            {
                Attack();
            }
        }            
    }
    private void FixedUpdate()
    {
        if (!dead && player != null)
        {
            Move();
        }
    }
}
