using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RifleM4 : PistolM4
{    
    void Start()
    {
        cooldown = 0.2f;
        auto = true;
        ammoCurrent = 30;
        ammoMax = 30;
        ammoBackPack = 60;
    }
}