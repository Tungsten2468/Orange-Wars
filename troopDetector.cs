using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class troopDetector : MonoBehaviour
{
    public GameObject troopDetected;

    public void OnTriggerEnter2D(Collider2D other)
    {
        var healthSc = other.gameObject.GetComponent<entity>();
        var tableHealthSc = other.gameObject.GetComponent<tableHealth>();

        if(healthSc != null && healthSc.thisEntity == entityType.Troop)
        {
            troopDetected = healthSc.gameObject;
        }
        else if(tableHealthSc != null)
        {
            troopDetected = tableHealthSc.gameObject;
        }
    }
    public void OnTriggerStay2D(Collider2D other)
    {
        var healthSc = other.gameObject.GetComponent<entity>();
        var tableHealthSc = other.gameObject.GetComponent<tableHealth>();

        if(healthSc != null && healthSc.thisEntity == entityType.Troop)
        {
            troopDetected = healthSc.gameObject;
        }
        else if(tableHealthSc != null)
        {
            troopDetected = tableHealthSc.gameObject;
        }
    }
    public void OnTriggerExit2D(Collider2D other)
    {
        var healthSc = other.gameObject.GetComponent<healthScript>();
        var tableHealthSc = other.gameObject.GetComponent<tableHealth>();
        if(healthSc != null && healthSc.thisTroop != null)
        {
            troopDetected = null;
        }
        else if(tableHealthSc != null)
        {
            troopDetected = null;
        }
        
    }

    void Update()
    {
        if (troopDetected == null)
            return;

        var health = troopDetected.GetComponent<healthScript>();
        var table = troopDetected.GetComponent<tableHealth>();

        if (health != null && health.currentHealth <= 0)
            troopDetected = null;

        if (table != null && table.currentTableHealth <= 0)
            troopDetected = null;
    }


}
