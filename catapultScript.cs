using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class catapultScript : troopBehaviors
{
    public enum status { unoperated, idleop, loaded, lobbing }
    public status myState;

    public operatorFinder getOperator;
    public int loadTick;
    public SpriteRenderer projectileShow;
    public GameObject myProjectile;

    // NEW: orientation
    //public bool facingRight = true; // default: shoots right (toward enemies)

    public override void initStats()
    {
        baseRangedForce = thisTroop.isLobber.lobForce;
        baseTicksBeforeShoot = thisTroop.isLobber.tickBeforeNextLob;
        baseLobAngle = thisTroop.isLobber.angle;
        clickLogic = flip;
    }

    public override void Start()
    {
        base.Start();
        tickManager.onTick += catapultTick;
    }

    public void flip()
    {
        if(game.troopRemovalMode)
            return;
        // Flip orientation when clicked
        facingRight = !facingRight;

        // Flip sprite visually
        Vector3 scale = transform.localScale;
        scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public void catapultTick()
    {
        checkOperatorExisting();

        switch (myState)
        {
            case status.unoperated:
                waitForOperator();
                break;

            case status.idleop:
                handleLoading();
                break;

            case status.loaded:
                waitForLaunch();
                break;
        }
    }

    public void checkOperatorExisting()
    {
        if (myState == status.unoperated)
            return;

        if (getOperator.operatorFound == null)
        {
            animator.Play("noOperator");
            myState = status.unoperated;
        }
    }

    public void waitForOperator()
    {
        if (myState != status.unoperated)
            return;

        if (getOperator.operatorFound != null)
        {
            myProjectile = adoptedProjectile();

            if (myProjectile == null)
                return;

            animator.Play("prepare");
            loadTick = tickManager.tickCount + baseTicksBeforeShoot;
            myState = status.idleop;
        }
    }

    public GameObject adoptedProjectile()
    {
        GameObject adoptedProjectile = null;

        troopBehaviors op = getOperator.operatorFound as troopBehaviors;

        // PRIORITY 1 — adopt from TROOPS (not catapults)
        if (op != null && !(op is catapultScript))
        {
            var tType = op.thisTroop.type;

            switch (tType)
            {
                case troopType.shooter:
                    if (op.thisTroop.isShooter.projectile != null)
                        adoptedProjectile = op.thisTroop.isShooter.projectile;
                    if (op.thisTroop.isLobber.projectile != null)
                        adoptedProjectile = op.thisTroop.isLobber.projectile;
                    break;

                case troopType.lobber:
                    if (op.thisTroop.isLobber.projectile != null)
                        adoptedProjectile = op.thisTroop.isLobber.projectile;
                    if (op.thisTroop.isShooter.projectile != null)
                        adoptedProjectile = op.thisTroop.isShooter.projectile;
                    break;

                case troopType.barrage:
                    if (op.thisTroop.isShooter.projectile != null)
                        adoptedProjectile = op.thisTroop.isShooter.projectile;
                    break;

                case troopType.clickshot:
                    if (op.thisTroop.isLobber.projectile != null)
                        adoptedProjectile = op.thisTroop.isLobber.projectile;
                    if (op.thisTroop.isShooter.projectile != null)
                        adoptedProjectile = op.thisTroop.isShooter.projectile;
                    break;

                case troopType.magdumper:
                    if (op.thisTroop.isLobber.projectile != null)
                        adoptedProjectile = op.thisTroop.isLobber.projectile;
                    if (op.thisTroop.isShooter.projectile != null)
                        adoptedProjectile = op.thisTroop.isShooter.projectile;
                    break;
            }

            peelUsage = op.peelUsage;
        }

        // PRIORITY 2 — adopt from OTHER CATAPULTS if troop adoption failed
        if (adoptedProjectile == null && getOperator.operatorFound is catapultScript)
        {
            var catp = getOperator.operatorFound as catapultScript;

            if (catp != null && catp.myProjectile != null)
            {
                adoptedProjectile = catp.myProjectile;
                peelUsage = catp.peelUsage;
            }
        }

        return adoptedProjectile;
    }

    public void handleLoading()
    {
        if (myState == status.loaded)
            return;

        if (game.orangePeels < 1)
            return;

        if (tickManager.tickCount > loadTick)
        {
            audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.catapultLoad);

            projectileShow.gameObject.SetActive(true);
            projectileShow.sprite = myProjectile.GetComponent<SpriteRenderer>().sprite;

            animator.Play("load");
            myState = status.loaded;
        }
    }

    public void waitForLaunch()
    {
        if (!checkForEnemies())
            return;

        // NEW: direction parameter based on orientation
        string direction = facingRight ? "w" : "e";

        if(facingRight)
            lob(myProjectile, peelUsage, direction);
        if(!facingRight)
            lobBackward(myProjectile, peelUsage, direction);

        loadTick = tickManager.tickCount + baseTicksBeforeShoot;
        myState = status.idleop;
    }

    public void OnDestroy()
    {
        tickManager.onTick -= catapultTick;
    }
}
