using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shootingSophomoreScript : enemyBehaviors
{
    public int shootTick;
    public override void initStats()
    {
        baseRangedForce = thisEnemy.isShooter.shootForce;
        baseTicksBeforeShoot = thisEnemy.isShooter.tickBeforeNextShot;
        mainProjectile = thisEnemy.isShooter.projectile;
    }
    public override void Start()
    {
        base.Start();
        laneMngr = GameObject.Find("gameManager").GetComponent<laneManager>();
        shootTick = tickManager.tickCount + baseTicksBeforeShoot;
        tickManager.onTick += waitForShoot;
    }

    public void waitForShoot()
    {
        /*if(checkForTroops() == false)
        {
            return;
        }*/
        if(tickManager.tickCount >= shootTick)
        {
            shoot(mainProjectile);
            shootTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        tickManager.onTick -= waitForShoot;
    }
}
