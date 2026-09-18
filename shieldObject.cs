using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shieldObject : entity
{
    public void Start()
    {
        deathLogic = shieldBreak;
    }

    public void shieldBreak()
    {
        Destroy(gameObject);
    }
}
