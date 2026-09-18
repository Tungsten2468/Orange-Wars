using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mummyScript : enemyBehaviors
{
    public int punchTick;
    public float stepInterval = 0.4f;
    private float stepTimer;
    
    public override void initStats()
    {
        maxHealth = thisEnemy.maxHealth * 1.5f;
        currentHealth = maxHealth;
        baseMeleeDamage = thisEnemy.isFootman.damage * 1.5f;
    }
    public override void Start()
    {
        base.Start();
        tickManager.onTick += mummyLoop;
    }

    public void mummyLoop()
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

    public void OnDestroy()
    {
        tickManager.onTick -= mummyLoop;
    }
}
