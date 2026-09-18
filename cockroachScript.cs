using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cockroachScript : enemyBehaviors
{
    public int maxOffspring = 20;
    public int offspringProduced;
    public bool amOffspring;
    public float reproductionChance = 0.02f; // 2%
    public bool offspringDeployed;

    public override void initStats()
    {
        baseMovementSpeed = thisEnemy.isFootman.speed;
    }

    public override void Start()
    {
        base.Start();
        tickManager.onTick += roachTick;
    }

    void roachTick()
    {
        if (!deploymentComplete)
            return;

        // Offspring first visually "deploy" to their lane's tile Y
        if (amOffspring && !offspringDeployed)
        {
            offspringDeploy();
            return; // don't move toward table until deployed
        }

        randomReproduce();
        onTheWayToSteal();
    }

    void offspringDeploy()
    {
        if (myTile == null)
        {
            // safety: if no tile, just mark as deployed
            offspringDeployed = true;
            return;
        }

        float targetY = myTile.transform.position.y;

        if (Mathf.Abs(transform.position.y - targetY) > 0.01f)
        {
            float newY = Mathf.MoveTowards(transform.position.y, targetY, 0.8f);
            rb.MovePosition(new Vector2(transform.position.x, newY));
        }
        else
        {
            offspringDeployed = true;
        }
    }

    void randomReproduce()
    {
        if (amOffspring)
            return;
        if (offspringProduced >= maxOffspring)
            return;

        float repChance = Random.Range(0f, 1f);
        if (repChance < reproductionChance)
        {
            // pick a lane + tile for the offspring's lane/tile assignment
            var randomLane = laneMngr.lanes[Random.Range(0, laneMngr.lanes.Count)];
            var laneTiles = laneMngr.lanes[randomLane.laneID].availableEnemyPositions;
            var randomTile = laneTiles[Random.Range(0, laneTiles.Count)];

            // spawn visually at the parent position
            GameObject offspring = game.summonEnemy(
                thisEnemy,
                transform.position,
                randomTile.gameObject
            );

            var roach = offspring.GetComponent<cockroachScript>();
            roach.amOffspring = true;
            roach.offspringDeployed = false;

            // start at parent position, but assign correct lane + tile
            offspring.transform.position = transform.position;
            roach.currentLaneID = randomLane.laneID;
            roach.myTile = randomTile;

            offspringProduced += 1;
        }
    }

    void onTheWayToSteal()
    {
        // walk continuously toward the junior table's X
        Vector2 targetPos = new Vector2(juniorTable.transform.position.x, transform.position.y);

        if (!reachedTarget(targetPos))
        {
            walkContinuous();
        }
        else
        {
            // at the table: either take peels or deal 0.5 damage
            if (game.orangePeels > 0)
            {
                // take some peels; here we take 1 peel per cockroach
                game.consumePeels(1);
            }
            else
            {
                juniorTable.GetComponent<tableHealth>().takeDamage(0.5f);
            }

            audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.snatch);

            // remove from lane list and destroy
            laneMngr.removeEnemyFromLane(thisEnemy, gameObject, currentLaneID);
            Destroy(gameObject);
        }

        if(transform.position.x < -15)
        {
            // remove from lane list and destroy anyway
            laneMngr.removeEnemyFromLane(thisEnemy, gameObject, currentLaneID);
            Destroy(gameObject);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        tickManager.onTick -= roachTick;
    }
}
