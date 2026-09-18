using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class flowerDancerScript : enemyBehaviors
{
    public int hitTick;
    public float stepInterval = 0.4f;
    private float stepTimer;
    public List<Color> colors;
    public SpriteRenderer bottomFlowerHat;
    public SpriteRenderer topFlowerHat;
    public int timeBetweenTwirls;
    public int ticksBeforeTwirl;

    private List<int> fullDancePattern = new List<int> {0,9,4,5};
    private List<int> bottomDancePattern  = new List<int> {4,9,5,4};
    private List<int> topDancePattern = new List<int> {5,0,4,5};
    public List<int> chosenPattern;

    [SerializeField]
    private enum state {walking, switching, idle, attacking}
    private state currentState;

    public override void initStats()
    {
        baseMovementSpeed = thisEnemy.isFootman.speed;
        baseTickBeforeMelee = thisEnemy.isFootman.ticksBetweenAttacks;
        baseMeleeDamage = thisEnemy.isFootman.damage;
    }

    public override void Start()
    {
        base.Start();
        angry = false;

        Color randomColor = colors[Random.Range(0, colors.Count - 1)];
        bottomFlowerHat.color = randomColor;
        topFlowerHat.color = randomColor;

        if(currentLaneID < 4)
            chosenPattern = bottomDancePattern;
        else if(currentLaneID > 5)
            chosenPattern = topDancePattern;
        else
            chosenPattern = fullDancePattern;

        currentState = state.walking;
        ticksBeforeTwirl = tickManager.tickCount + timeBetweenTwirls;

        tickManager.onTick += dancerTick;
    }

    public void moveUpDownLane()
    {
        if (Mathf.Abs(transform.position.y - targetLanePos.y) > 0.05f)
        {
            float newY = Mathf.MoveTowards(transform.position.y, targetLanePos.y, 0.8f);
            rb.MovePosition(new Vector2(targetLanePos.x, newY));
        }
        else
        {
            // Arrived at new lane
            laneMngr.removeEnemyFromLane(thisEnemy, gameObject, currentLaneID);
            laneMngr.addEnemyToLane(thisEnemy, gameObject, nextLane);
            currentLaneID = nextLane;

            ticksBeforeTwirl = tickManager.tickCount + timeBetweenTwirls;

            // If angry, resume angry walking
            if(angry)
                animator.Play("run");
            else
                animator.Play("walk");

            currentState = state.walking;
        }
    }

    public void twirlTick()
    {
        if(angry)
            return;

        if(tickManager.tickCount >= ticksBeforeTwirl)
        {
            currentState = state.switching;
            animator.Play("twirl");

            SwitchLanes(chosenPattern[nextStep]);
            ticksBeforeTwirl = tickManager.tickCount + timeBetweenTwirls;

            if(nextStep == chosenPattern.Count - 1)
                nextStep = 0;
            else
                nextStep += 1;
        }
    }

    public void checkMusic()
    {
        if (angry)
            return;

        if (leader == null)
        {
            BecomeAngry();
            return;
        }

        var audio = leader.GetComponent<AudioSource>();
        if (audio == null || audio.clip == null || !audio.isPlaying)
        {
            BecomeAngry();
            return;
        }
    }

    private void BecomeAngry()
    {
        angry = true;
        audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.angryFemale);

        baseMovementSpeed *= 2;

        // IMPORTANT FIX:
        // If she is mid-switch, DO NOT interrupt it.
        // Let moveUpDownLane finish the transition.
        if (currentState == state.switching)
        {
            animator.Play("twirl"); // keep animation consistent
            return;
        }

        // Otherwise she resumes angry walking normally
        animator.Play("run");
        currentState = state.walking;
    }

    public void dancerTick()
    {
        if(!deploymentComplete)
            return;

        if (this == null || gameObject == null)
            return;

        twirlTick();
        checkMusic();

        switch(currentState)
        {
            case state.walking:
                walkContinuous();
                if(arrivedAtTroop())
                {
                    hitTick = tickManager.tickCount + modifiedTick(baseTickBeforeMelee, ticksBeforeMeleeModifier);
                    currentState = state.attacking;
                }
                break;

            case state.switching:
                moveUpDownLane();
                break;

            case state.attacking:
                waitForAttack();
                break;

            case state.idle:
                break;
        }
    }

    public void waitForAttack()
    {
        if (tickManager.tickCount >= hitTick)
        {
            if (!arrivedAtTroop())
            {
                animator.Play("run");
                currentState = state.walking;
                return;
            }

            hitTick = tickManager.tickCount + modifiedTick(baseTickBeforeMelee, ticksBeforeMeleeModifier);
            punch(getTroopDetected.troopDetected.GetComponent<entity>());
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        tickManager.onTick -= dancerTick;
    }

    void Update()
    {
        if (currentState == state.walking)
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
}
