using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class larryBirdScript : enemyBehaviors
{
    public Vector2 homePoint;
    public enum state{otw, stole}
    public GameObject oranges;
    public state myStatus;
    public float orangesCarrying;
    public bool successfulReturn;
    public float dropOrangesChance = 0.75f;
    public override void initStats()
    {
        baseMovementSpeed = thisEnemy.isFootman.speed;
    }

    public override void Start()
    {
        base.Start();
        homePoint = new Vector2(transform.position.x, myTile.transform.position.y);
        myStatus = state.otw;
        tickManager.onTick += larryBirdTick;
    }

    public void larryBirdTick()
    {
        if(!deploymentComplete)
            return;
        
        if(myStatus == state.otw)
            onTheWayToSteal();
        if(myStatus == state.stole)
            onTheWayBack();
    }

    public void onTheWayToSteal()
    {
        if(!reachedTarget(new Vector2(juniorTable.transform.position.x, transform.position.y)))
        {
            walkContinuous();
        }
        else
        {
            transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
            oranges.SetActive(true);
            if(game.oranges > thisEnemy.isSabotage.amountTaking)
            {
                game.takeOrange(thisEnemy.isSabotage.amountTaking);
                orangesCarrying = thisEnemy.isSabotage.amountTaking;
            }      
            else
            {
                orangesCarrying = game.oranges;
                game.takeOrange(game.oranges);  
            }
            audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.snatch);
            myStatus = state.stole;
        }
    }

    public void onTheWayBack()
    {
        if(!reachedTarget(homePoint))
            walkToward(homePoint, factorInModifier(baseMovementSpeed, movementSpeedModifier));
        else
        {
            successfulReturn = true;
            laneMngr.removeEnemyFromLane(thisEnemy, gameObject, currentLaneID);
            Destroy(gameObject);
        }  
    }

    void OnApplicationQuit()
    {
        quitting = true;
    }

    public override void OnDeath()
    {
        if (!successfulReturn)
        {
            float dropChance = Random.Range(0f, 1f);
            if (dropChance <= dropOrangesChance)
            {
                GameObject droppedOrange = Instantiate(orangeDrop, transform.position, Quaternion.identity);
                droppedOrange.GetComponent<collectableOrangeScript>().value = (int)orangesCarrying;
            }
        }
    }


    public override void OnDestroy()
    {
        // Prevent spawning during scene unload or reload
        if (!gameObject.scene.IsValid())
            return;

        base.OnDestroy();
        tickManager.onTick -= larryBirdTick;
    }


}
