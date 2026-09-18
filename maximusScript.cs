using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class maximusScript : troopBehaviors
{
    private enum state { idle, spraying, rummaging }
    private state currentState;

    public int sprayTick;
    public int stopRummageTick;
    public float rummageSoundInterval;
    private float rummageSoundTimer;

    public override void initStats()
    {
        baseTicksBeforeShoot = thisTroop.isMagdumper.reloadTicks;
        baseRangedForce = thisTroop.isMagdumper.shootForce;
        baseLobAngle = thisTroop.isLobber.angle;
    }

    public override void Start()
    {
        base.Start();
        shooting = false;
        ammoLeft = thisTroop.isMagdumper.magCapacity;
        thisEffects.functionToCallback = resetShootIntervals;
        tickManager.onTick += maxTick;
        currentState = state.idle;
    }

    public void maxTick()
    {
        switch (currentState)
        {
            case state.idle:
                waitForEnemy();
                break;
            case state.spraying:
                spraying();
                break;
            case state.rummaging:
                rummage();
                break;
        }
    }

    public void waitForEnemy()
    {
        if (ammoLeft == 0)
        {
            stopRummageTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
            currentState = state.rummaging;
            animator.Play("rummage");
            return;
        }

        if (checkForEnemies())
        {
            animator.Play("spray");
            currentState = state.spraying;
        }
    }

    public void spraying()
    {
        if (!shooting)
        {
            StartCoroutine(spray(ammoLeft, thisTroop.isMagdumper.secsBetweenShots));
            shooting = true;
        }
    }

    public void rummage()
    {
        if (tickManager.tickCount >= stopRummageTick)
        {
            ammoLeft = thisTroop.isMagdumper.magCapacity;
            currentState = state.idle;
            animator.Play("idle");
        }
    }

    IEnumerator spray(int times, float delay)
    {
        Debug.Log("spraying started");

        // SAFETY: no enemies → stop
        if (!checkForEnemies())
        {
            shooting = false;
            animator.Play("idle");
            yield break;
        }

        // Find farthest enemy
        float farthestX = float.MinValue;
        Transform farthestEnemy = null;

        foreach (var e in laneMngr.lanes[currentLaneID].enemyObjects)
        {
            var hs = e.GetComponent<entity>();
            if (hs == null || hs.myTile == null)
                continue;

            float ex = hs.myTile.transform.position.x;
            if (ex > farthestX)
            {
                farthestX = ex;
                farthestEnemy = e.transform;
            }
        }

        // SAFETY: no valid target
        if (farthestEnemy == null)
        {
            shooting = false;
            animator.Play("idle");
            yield break;
        }

        currentTargetX = farthestEnemy.transform.position.x;

        // SPRAY LOOP
        for (int t = 0; t < times; t++)
        {
            if (!checkForEnemies())
            {
                shooting = false;
                animator.Play("idle");
                yield break;
            }

            if (ammoLeft < thisTroop.isMagdumper.magCapacity * 0.5f)
                currentTargetX -= 0.5f;

            modifiedLob(thisTroop.isMagdumper.projectile);
            ammoLeft -= 1;

            yield return new WaitForSeconds(delay);
        }

        // WAIT BEFORE THROWING BOTTLE
        yield return new WaitForSeconds(1f);

        animator.Play("lob");

        // SAFETY: ensure an enemy still exists
        var enemyList = laneMngr.lanes[currentLaneID].enemyObjects;
        if (enemyList.Count == 0)
        {
            shooting = false;
            animator.Play("idle");
            yield break;
        }

        // THROW THE BOTTLE
        currentTargetX = enemyList[0].transform.position.x;
        modifiedLob(thisTroop.isLobber.projectile);

        yield return new WaitForSeconds(0.5f);

        // FINAL RESET — ONLY shooting, NOT state
        shooting = false;

        animator.Play("rummaging");
        stopRummageTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
        currentState = state.rummaging;
    }


    public void resetShootIntervals()
    {
        ammoLeft = thisTroop.isMagdumper.magCapacity;
        currentState = state.idle;
    }

    void Update()
    {
        if (currentState == state.rummaging)
        {
            rummageSoundTimer -= Time.deltaTime;

            if (rummageSoundTimer <= 0f)
            {
                myAudio.pitch = Random.Range(0.5f, 1f);
                myAudio.PlayOneShot(thisTroop.isMagdumper.reloadSound);
                myAudio.pitch = 1f;
                rummageSoundTimer = rummageSoundInterval;
            }
        }
    }

    public void OnDestroy()
    {
        tickManager.onTick -= maxTick;
    }
}
