using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sophomoreLobberScript : enemyBehaviors
{
    public int shootTick;
    public override void initStats()
    {
        baseRangedForce = thisEnemy.isLobber.lobForce;
        baseTicksBeforeShoot = thisEnemy.isLobber.tickBeforeNextLob;
        baseLobAngle = thisEnemy.isLobber.angle;
        mainProjectile = thisEnemy.isLobber.projectile;
    }

    public override void Start()
    {
        base.Start();
        shootTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
        tickManager.onTick += waitForShoot;
    }

    public void waitForShoot()
    {
        if(tickManager.tickCount >= shootTick)
        {
            lob();
            shootTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        tickManager.onTick -= waitForShoot;
    }
}
