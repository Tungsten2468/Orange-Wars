using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class damageArea : MonoBehaviour
{
    public float damage;
    public float radius;
    public List<entity> entitiesInArea;
    public bool targetTroopsOnly;
    public GameObject particles;
    public int ticksLasting = 1;
    public statusEffect effect;
    public void Start()
    {
        entitiesInArea = new List<entity>();
        GetComponent<CircleCollider2D>().radius = radius;
        StartCoroutine(damageAOE());
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        entity takeDmg = other.GetComponent<entity>();
        if(takeDmg == null)
            return;
        if(particles != null)
            Instantiate(particles, transform.position, Quaternion.identity);
        if(!entitiesInArea.Contains(takeDmg))
        {
            if(!targetTroopsOnly)
            {
                if(takeDmg.thisEntity == entityType.Enemy)
                {
                    entitiesInArea.Add(takeDmg);
                } 
            }
            if(targetTroopsOnly)
            {
                if(takeDmg.thisEntity == entityType.Troop)
                {
                    entitiesInArea.Add(takeDmg);
                } 
            }
        }
    }

    public IEnumerator damageAOE()
    {
        Debug.Log("entities in AOE: "+entitiesInArea);
        // allow time for all entities to enter the trigger
        yield return new WaitForSeconds(0.05f);

        foreach(entity victim in entitiesInArea)
        {
            if (victim == null)
                continue;

            victim.takeDamage(damage);

            if (effect != null)
            {
                statusEffectManager inflictStatus = victim.GetComponent<statusEffectManager>();
                if (inflictStatus != null)
                    inflictStatus.beginEffect(effect);
            }
        }

            
        
        yield return new WaitForSeconds(0.05f);
        Debug.Log("All entities in AOE killed now");
        Destroy(gameObject);
    }

}
