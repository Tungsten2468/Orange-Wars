using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class jasiereScript : troopBehaviors
{
    public int shootTick;
    public override void initStats()
    {
        baseRangedForce = thisTroop.isLobber.lobForce;
        baseTicksBeforeShoot = thisTroop.isLobber.tickBeforeNextLob;
        baseLobAngle = thisTroop.isLobber.angle;
        peelUsage = thisTroop.isLobber.peelsWaste;
    }
    public override void Start()
    {
        base.Start();
        thisEffects.functionToCallback = resetShootIntervals;

        tickManager.onTick += waitForShoot;
    }

    public void waitForShoot()
    {
        if(!checkForEnemies())
        {
            return;
        }
        if(tickManager.tickCount >= shootTick && haveEnoughPeels())
        {
            lob(thisTroop.isLobber.projectile, peelUsage);
            if(checkForEnemies())
                shootTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
        }
    }

    public void OnDestroy()
    {
        tickManager.onTick -= waitForShoot;
    }
    public void resetShootIntervals()
    {
        if(haveEnoughPeels())
            lob(thisTroop.isLobber.projectile, peelUsage);
        shootTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
    }
}
