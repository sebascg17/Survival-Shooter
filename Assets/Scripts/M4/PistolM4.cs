using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PistolM4 : WeaponM4
{
    void Start()
    {
        cooldown = 0;
        auto = false;
        ammoCurrent = 10;
        ammoMax= 10;
        ammoBackPack = 30;
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
                // Puedes cambiar el número 10 por lo que quieras. Esa es la cantidad de daño que causa una bala.
                hit.collider.gameObject.GetComponent<EnemyM4>().ChangeHealth(10);
            }
            Destroy(gameBullet, 1);
        }
    }
}