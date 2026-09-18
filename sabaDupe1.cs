using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sabaDupe1 : MonoBehaviour, laneComponent
{
    public sabaPhase3 mainBoss;
    public GameObject poof;
    public int currentLaneID;
    [HideInInspector]
    public int interfaceLaneID
    {
        get => currentLaneID;
        set => currentLaneID = value;
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        var hitMe = other.GetComponent<projectileScript>();

        if(hitMe == null)
            return;

        if(hitMe.myLane != interfaceLaneID)
            return;

        if(!hitMe.enemyProjectile)
        {
            mainBoss.StartCoroutine(mainBoss.dupeChosen());
            Instantiate(poof, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    public IEnumerator destroyMyself()
    {
        yield return new WaitForSeconds(0.001f);
        Destroy(gameObject);
    }
}
