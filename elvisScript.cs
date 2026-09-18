using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class elvisScript : troopBehaviors
{
    public Vector2 point1;
    public Vector2 mySeat;
    public Vector2 lunchLady;
    public int endOrangeTicks;
    public float stepInterval = 0.4f;
    private float stepTimer;
    public Vector2 currentDirection;
    //public healthScript myHealth;
    public override void initStats()
    {
        baseMovementSpeed = 0.55f;
        baseLogisticsTicks = thisTroop.isLogistics.retrieveTicks;
        clickLogic = elvisRetrieve;
    }
    public override void Start()
    {
        base.Start();
        thisEffects.functionToCallback = beginMoving;
        mySeat = transform.position;
    }
    /*void OnMouseDown()
    {
        if(stunned)
            return;
        if (moving)
            return;

        beginMoving();
    }*/

    public void elvisRetrieve()
    {
        if(stunned)
            return;
        if (moving)
            return;

        beginMoving();
    }

    public void beginMoving()
    {
        animator.SetInteger("dir", 1);
        point1 = new Vector2(-7, transform.position.y); //makes it look like he is walking away from the table first
        currentDirection = point1;
        tickManager.onTick += walk;
    }
    public void walk()
    {
        if (Vector2.Distance(rb.position, currentDirection) > 0.05f) //walk until reached destination
        {
            moving = true;

            step(currentDirection);
        }
        else
        {
            moving = false;
            if (currentDirection == mySeat) //stop moving if back at seat
            {
                tickManager.onTick -= walk;
                //moving = false;
                animator.SetInteger("dir", 0);
                game.addOrange(thisTroop.isLogistics.retrieveAmount); //oranges will be added once Elvis has returned
                return;
            }
            if (courseCharter() == false) //chart next course if possible
            {
                tickManager.onTick -= walk;
                endOrangeTicks = tickManager.tickCount + modifiedTick(baseLogisticsTicks, logisticsTicksModifier);
                tickManager.onTick += getOranges;
            }
        }
    }
    public bool courseCharter()
    {
        if(currentDirection == null)
        {
            currentDirection = point1;
            return true;
        }
        else if(currentDirection == point1)
        {
            currentDirection = lunchLady;
            return true;
        }
        else if(currentDirection == lunchLady)
        {
            animator.SetInteger("dir", 2);
            currentDirection = mySeat;
            return false;
        }
        return false;
    }
    public void getOranges()
    {
        if(tickManager.tickCount >= endOrangeTicks)
        {
            tickManager.onTick -= getOranges;
            currentDirection = mySeat;
            tickManager.onTick += walk;
        }
    }

    void Update()
    {
        if (moving)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                myAudio.pitch = Random.Range(0.9f, 1.3f);
                myAudio.PlayOneShot(audioManager.instance.footstep);
                myAudio.pitch = 1f;
                stepTimer = stepInterval;
            }
        }
    }
}
