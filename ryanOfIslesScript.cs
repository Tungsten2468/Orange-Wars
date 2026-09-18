using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ryanOfIslesScript : enemyBehaviors
{
    public int hitTick;
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
        StartCoroutine(summonMinions(thisEnemy.isSummoner.summonAmount, thisEnemy.isSummoner.secsBetweenSummon));
        tickManager.onTick += ryanLoop;
    }

    public void ryanLoop()
    {
        if(!deploymentComplete)
            return;
        if (!arrivedAtTroop())
        {
            walkContinuous();
            PlaySong(audioManager.instance.spring);
            moving = true;
            return;
        }
        moving = false;

        if (!punching)
        {
            StopSong();
            punching = true;
            tickManager.onTick += waitForAttack;
        }
    }
    public void waitForAttack()
    {
        enemyInRange = getTroopDetected.troopDetected;
        if (tickManager.tickCount >= hitTick)
        {
            if(enemyInRange == null)
            return;
            punch(enemyInRange.GetComponent<entity>());
            hitTick = tickManager.tickCount + modifiedTick(baseTickBeforeMelee, ticksBeforeMeleeModifier);
        }
        if(!arrivedAtTroop())
        {
            punching = false;
            tickManager.onTick -= waitForAttack;
            animator.Play("walk");
        }
    }

    public void PlaySong(AudioClip track)
    {
        if(myAudio.clip != null)
            return;
        myAudio.clip = track;
        myAudio.loop = true;
        myAudio.Play();
    }

    public void StopSong()
    {
        if (myAudio.clip != null)
        {
            myAudio.Stop();
            myAudio.clip = null;
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
        tickManager.onTick -= ryanLoop;
    }
}
