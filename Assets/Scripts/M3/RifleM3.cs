using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RifleM3 : PistolM3
{    
    void Start()
    {
        cooldown = 0.2f;
        auto = true;
    }
}