using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class sabaClone1 : enemyBehaviors
{
    public GameObject projectile;
    public override void initStats()
    {
        baseTicksBeforeShoot = 8;
        baseMovementSpeed = 0.25f;
        baseRangedForce = 8;
    }
    public int changeLaneFreq; 
    public int shootCooldownTicks;

    private int shootTick;
    private int laneChangeTick;

    private enum BossState { Entrance, Idle, Walking, Shooting }
    private BossState state = BossState.Entrance;
    //public int currentLane;
    public List<int> possibleLaneIDs = new List<int>(){5,7,9};
    public tileScript tileTargetted;
    public entity mainBoss;

    public override void Start()
    {
        base.Start();
        currentHealth = mainBoss.GetComponent<entity>().currentHealth;
        state = BossState.Entrance;
        animator.Play("charge");
        SwitchLanes(7, "l");
        laneChangeTick = tickManager.tickCount + changeLaneFreq;
        tickManager.onTick += onCloneTick;
    }

    public void onCloneTick()
    {
        //tileTargetted = myTile;
        HandleLaneChangeTimer();
        switch (state)
        {
            case BossState.Entrance:
                if (Mathf.Abs(transform.position.y - targetLanePos.y) > 0.05f)
                {
                    walkVertical(targetLanePos.y, 20f);
                }
                else
                {
                    moving = false;
                    animator.Play("idle");
                    state = BossState.Idle;
                }
                break;
            case BossState.Walking:
                HandleWalking();
                break;

            case BossState.Shooting:
                HandleShooting();
                break;
        }
    }

    private void HandleLaneChangeTimer()
    {
        if (tickManager.tickCount >= laneChangeTick)
        {
            int randomLane = possibleLaneIDs[Random.Range(0, possibleLaneIDs.Count)];
            if(currentLaneID != randomLane)
                animator.Play("walk");
            SwitchLanes(randomLane, "l");
            state = BossState.Walking;

            if(randomLane == 7)
                laneChangeTick = tickManager.tickCount + changeLaneFreq/2;
            else
                laneChangeTick = tickManager.tickCount + changeLaneFreq;
        }
    }

    private void HandleWalking()
    {
        Debug.Log("should now walk to next lane");
        if (Mathf.Abs(transform.position.y - targetLanePos.y) > 0.05f)
        {
            walkVertical(targetLanePos.y, factorInModifier(baseMovementSpeed, movementSpeedModifier));
        }
        else
        {
            moving = false;
            animator.Play("idle");
            state = BossState.Shooting;
            shoot(projectile);
            shootTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
        }
    }

    private void HandleShooting()
    {
        if (tickManager.tickCount >= shootTick)
        {
            shoot(projectile);
            shootTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        tickManager.onTick -= onCloneTick;
    }
}
