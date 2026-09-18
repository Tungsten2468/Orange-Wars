using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jaykoScript : troopBehaviors
{
    public GameObject orangePeelPileObj;
    public float eatSoundInterval = 0.4f;
    private float eatSoundTimer;
    public override void initStats()
    {
        baseTicksBetweenProduction = thisTroop.isProducer.productionTicks;
        baseProductionAmount = thisTroop.isProducer.productionAmount;
    }
    public override void Start()
    {
        base.Start();
        thisEffects.functionToCallback = resetEatIntervals;
        orangePeelPileObj = GameObject.Find("peelNapkin");
    }

    void Update()
    {
        if (eating)
        {
            eatSoundTimer -= Time.deltaTime;

            if (eatSoundTimer <= 0f)
            {
                myAudio.pitch = UnityEngine.Random.Range(0.5f, 1f);
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

        tickManager.onTick -= finishEating;

        getPeels();

        eatOrange();
    }


    /*public IEnumerator flyPeel(Rigidbody2D peelRb, Vector2 peelTarget)
    {
        while (Vector2.Distance(peelRb.position, peelTarget) > 0.05f)
        {
            peelRb.MovePosition(Vector2.MoveTowards(
                peelRb.position,
                peelTarget,
                10f * Time.deltaTime
            ));

            yield return null; // THIS prevents freezing
        }

        Destroy(peelRb.gameObject);
        game.addPeels(1);
    }*/

}
