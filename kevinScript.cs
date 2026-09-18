using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kevinScript : troopBehaviors
{
    public int shootTick;
    public override void initStats()
    {
        baseRangedForce = thisTroop.isShooter.shootForce;
        baseTicksBeforeShoot = thisTroop.isShooter.tickBeforeNextShot;
        peelUsage = thisTroop.isShooter.peelsWaste;
    }
    public override void Start()
    {
        base.Start();
        tickManager.onTick -= waitForShoot; // remove ghost listeners
        thisEffects.functionToCallback = resetShootIntervals;
        laneMngr = GameObject.Find("gameManager").GetComponent<laneManager>();
        game = GameObject.Find("gameManager").GetComponent<GameSequence>();
        tickManager.onTick += waitForShoot;
    }

    public void waitForShoot()
    {
        if (!checkForEnemies())
            return;

        if (tickManager.tickCount >= shootTick && haveEnoughPeels())
        {
            shoot(thisTroop.isShooter.projectile, thisTroop.isShooter.peelsWaste);
            if(checkForEnemies())
                shootTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
        }
    }

    public void resetShootIntervals()
    {
        if(haveEnoughPeels())
            shoot(thisTroop.isShooter.projectile, thisTroop.isShooter.peelsWaste);
        shootTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
    }
}