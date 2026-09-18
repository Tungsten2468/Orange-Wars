using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class juanEspScript : troopBehaviors
{
    public override void initStats()
    {
        baseRangedForce = thisTroop.isShooter.shootForce;
        peelUsage = thisTroop.isShooter.peelsWaste;
        clickLogic = clickShoot;
    }

    public void clickShoot()
    {
        if(game.orangePeels < 1)
            return;
        shoot(thisTroop.isShooter.projectile, peelUsage, "y");
    }
}
