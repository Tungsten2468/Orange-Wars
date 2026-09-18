using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class operatorFinder : MonoBehaviour
{
    public entity operatorFound;
    public void OnTriggerEnter2D(Collider2D other)
    {
        var op = other.GetComponent<entity>();
        if(op == null)
            return;
        if(op.thisEntity == entityType.Troop)
        {
            operatorFound = op;
        }
    }
}
