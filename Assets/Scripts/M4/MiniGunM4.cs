using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGunM4 : WeaponM4
{
    void Start()
    {
        cooldown = 0.1f;
        auto = true;
        ammoCurrent = 100;
        ammoMax = 100;
        ammoBackPack = 200;
    }

    protected override void OnShoot()
    {
        Vector3 rayStartPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        Vector3 drift = new Vector3(Random.Range(-15, 15), Random.Range(-15, 15), Random.Range(-15, 15));
        
        Ray ray = cam.GetComponent<Camera>().ScreenPointToRay(rayStartPosition + drift);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject gameBullet = Instantiate(particle, hit.point, hit.transform.rotation);
            if(hit.collider.CompareTag("enemy"))
            {
                // Puedes cambiar el número 10 por lo que quieras. Esa es la cantidad de daño que causa una bala.
                hit.collider.gameObject.GetComponent<EnemyM4>().ChangeHealth(10);
            }
            Destroy(gameBullet, 1);
        }
    }
}