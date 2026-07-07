using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pistol : Weapon
{
    void Start()
    {
        cooldown = 0;
        auto = false;
        ammoCurrent = 10;
        ammoMax= 10;
        ammoBackPack = 30;
        ammoBackPackMax = 30;
    }

    protected override void OnShoot()
    {
        Vector3 rayStartPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);        
        Ray ray = cam.GetComponent<Camera>().ScreenPointToRay(rayStartPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject gameBullet = Instantiate(particle, hit.point, hit.transform.rotation);
            if(hit.collider.CompareTag("enemy"))
            {
                Enemy enemy = hit.collider.gameObject.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    enemy.GetDamage(10, photonView.Owner.ActorNumber);
                }
            }
            else if (hit.collider.CompareTag("Player"))
            {
                PlayerController playerController = hit.collider.gameObject.GetComponentInParent<PlayerController>();
                if (playerController != null)
                {
                    playerController.GetDamage(10);
                }
            }
            Destroy(gameBullet, 1);
        }
    }
}
