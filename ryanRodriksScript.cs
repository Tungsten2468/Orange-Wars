using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ryanRodriksScript : troopBehaviors
{
    public int playTick;
    public List<string> animationStates;
    public override void initStats()
    {
        baseRangedForce = thisTroop.isShooter.shootForce;
        baseTicksBeforeShoot = thisTroop.isShooter.tickBeforeNextShot;
    }

    public override void Start()
    {
        base.Start();
        tickManager.onTick += eGuitarTick;
        playTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
    }

    public void eGuitarTick()
    {
        if(checkForEnemies())
        {
            if(tickManager.tickCount >= playTick)
            {
                animator.Play(Random.Range(0, animationStates.Count));
                multiLaneAttack();
                playTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
            }
        }
    }

    public void multiLaneAttack()
    {
        animator.Play(animationStates[Random.Range(0, animationStates.Count)]);
        // lane he's currently on
        randomDynamicAttack(shootPoint.transform.position);

        // lane above
        if (currentLaneID + 1 < laneMngr.lanes.Count)
        {
            randomDynamicAttack(
                new Vector2(
                    shootPoint.transform.position.x,
                    laneMngr.lanes[currentLaneID + 1].possibleTroopTiles[0].transform.position.y
                )
            );
        }

        // lane below
        if (currentLaneID - 1 >= 0)
        {
            randomDynamicAttack(
                new Vector2(
                    shootPoint.transform.position.x,
                    laneMngr.lanes[currentLaneID - 1].possibleTroopTiles[0].transform.position.y
                )
            );
        }
    }


    public void OnDestroy()
    {
        tickManager.onTick -= eGuitarTick;
    }
}
