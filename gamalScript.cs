using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gamalScript : enemyBehaviors
{
    public int punchTick;
    public float stepInterval = 0.4f;
    private float stepTimer;
 
    public override void initStats()
    {
        baseMovementSpeed = thisEnemy.isFootman.speed;
        baseMeleeDamage = thisEnemy.isFootman.damage;
        baseTickBeforeMelee = thisEnemy.isFootman.ticksBetweenAttacks;
    }
    public override void Start()
    {
        base.Start();
        tickManager.onTick += gamalLoop;
    }

    public void gamalLoop()
    {
        if(!deploymentComplete)
            return;
        if (!arrivedAtTroop())
        {
            walkContinuous();
            return;
        }
        moving = false;

        if (!punching)
        {
            punching = true;
            tickManager.onTick += waitForPunch;
        }
    }


    public void waitForPunch()
    {
        enemyInRange = getTroopDetected.troopDetected;
        if (tickManager.tickCount >= punchTick)
        {
            if(enemyInRange == null)
                return;
            punch(enemyInRange.GetComponent<entity>());
            punchTick = tickManager.tickCount + modifiedTick(baseTickBeforeMelee, ticksBeforeMeleeModifier);
        }
        if(!arrivedAtTroop())
        {
            punching = false;
            tickManager.onTick -= waitForPunch;
            animator.Play("walk");
        }
    }

    void Update()
    {
        if (moving)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                audioManager.instance.sfxSource.pitch = Random.Range(0.9f, 1.3f);
                audioManager.instance.Play(thisEnemy.isFootman.walkingSound);
                audioManager.instance.sfxSource.pitch = 1f;
                stepTimer = stepInterval;
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        tickManager.onTick -= gamalLoop;
    }

}
