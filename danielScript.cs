using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class danielScript : troopBehaviors
{
    public int nextEatTick;
    public int inspectionTicks;
    public GameObject orangePeelPileObj;
    public float eatSoundInterval = 0.4f;
    private float eatSoundTimer;
    private enum state {playing, inspect, eating}
    private state currentState;
    public int inspectLasting;
    public override void initStats()
    {
        baseTicksBetweenProduction = thisTroop.isProducer.productionTicks;
        baseProductionAmount = thisTroop.isProducer.productionAmount;
        baseAutoProduceTicks = thisTroop.isProducer.autoTicks;
    }   
    public override void Start()
    {
        base.Start();
        thisEffects.functionToCallback = resetEatIntervals;
        orangePeelPileObj = GameObject.Find("peelNapkin");
        nextEatTick = tickManager.tickCount + modifiedTick(baseAutoProduceTicks, autoProduceTicksModifier);
        currentState = state.playing;
        tickManager.onTick += passiveTick;
    }

    public void passiveTick()
    {
        switch(currentState)
        {
            case state.playing:
                waitToEat();
                break;
            case state.inspect:
                inspectOrange();
                break;
            case state.eating:
                finishEating();
                nextEatTick = tickManager.tickCount + modifiedTick(baseAutoProduceTicks, autoProduceTicksModifier);
                currentState = state.playing;
                break;
        }
    }
    public void waitToEat()
    {
        if(tickManager.tickCount >= nextEatTick)
        {
            if(game.oranges < 1)
            {
                nextEatTick = tickManager.tickCount + modifiedTick(baseAutoProduceTicks, autoProduceTicksModifier);
                return;
            }
            animator.Play("inspect");
            inspectionTicks = tickManager.tickCount + inspectLasting;
            game.takeOrange(1);
            audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.lickLips);
            currentState = state.inspect;
        }
    }
    public void inspectOrange()
    {
        if(tickManager.tickCount >= inspectionTicks)
        {
            currentState = state.eating;
            eatOrange(); 
        }
    }

    void Update()
    {
        if (eating)
        {
            eatSoundTimer -= Time.deltaTime;

            if (eatSoundTimer <= 0f)
            {
                myAudio.pitch = Random.Range(0.5f, 1f);
                myAudio.PlayOneShot(audioManager.instance.jaykoEat);
                myAudio.pitch = 1f;
                eatSoundTimer = eatSoundInterval;
            }
        }
    }

    public void resetEatIntervals()
    {
        eating = false;
        animator.SetBool("eating", false);

        eatOrange();
    }


    public void OnDestroy()
    {
        tickManager.onTick -= passiveTick;
    }
}
