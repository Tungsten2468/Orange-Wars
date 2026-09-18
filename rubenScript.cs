using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rubenScript : troopBehaviors
{
    public int shootTick;
    public override void initStats()
    {
        baseRangedForce = thisTroop.isBarrage.shootForce;
        baseTicksBeforeShoot = thisTroop.isBarrage.ticksBetweenBarr;
        peelUsage = thisTroop.isBarrage.peelsWaste;
    }
    public override void Start()
    {
        base.Start();
        thisEffects.functionToCallback = resetShootIntervals;
        tickManager.onTick += waitForShoot;
    }

    IEnumerator shootMany(int times, float delay)
    {
        for(int t = 0; t < times; t++)
        {
            if(!checkForEnemies() || !haveEnoughPeels())
            {
                break; // stop barrage early
            }

            shoot(thisTroop.isShooter.projectile, thisTroop.isBarrage.peelsWaste);
            yield return new WaitForSeconds(delay);
        }

        if(!checkForEnemies())
        {
            shootTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
        }
    }

    public void waitForShoot()
    {
        if(!checkForEnemies())
        {
            return;
        }

        if(tickManager.tickCount >= shootTick)
        {
            shootTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
            StartCoroutine(shootMany(thisTroop.isBarrage.ammoInBarrage, thisTroop.isBarrage.secsBetweenAttacks));
        }
    }

    public void resetShootIntervals()
    {
        if(haveEnoughPeels())
            shootTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
        StartCoroutine(shootMany(thisTroop.isBarrage.ammoInBarrage, thisTroop.isBarrage.secsBetweenAttacks));
    }

    public void OnDestroy()
    {
        tickManager.onTick -= waitForShoot;
    }
}
