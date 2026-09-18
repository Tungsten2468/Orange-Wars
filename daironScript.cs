using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class daironScript : troopBehaviors
{
    public SpriteRenderer graphic;
    public int nextHealTick;
    public int leaveTick;
    public float healInterval = 0.4f;
    private float healTimer;
    public Canvas myUI;
    public override void initStats()
    {
        
    }
    public override void Start()
    {
        base.Start();
        healing = false;
        game = GameObject.Find("gameManager").GetComponent<GameSequence>();
        juniorTable = GameObject.Find("juniorTable").GetComponent<tableHealth>();
        leaveTick = tickManager.tickCount + thisTroop.isHealer.ticksLasting;
        animator.Play("walk");
        tickManager.onTick += walkToBackTable;
    }

    public void walkToBackTable()
    {
        if(rb.position.x > juniorTable.transform.position.x + 2.2f)
        {
            rb.MovePosition(rb.position + Vector2.left * thisTroop.isHealer.speed);
        }
        else
        {
            moving = false;
            healing = true;
            graphic.sortingOrder = 10;
            graphic.sortingLayerName = "Table";
            animator.Play("heal");
            nextHealTick = tickManager.tickCount + thisTroop.isHealer.ticksPerHeal;
            leaveTick = tickManager.tickCount + thisTroop.isHealer.ticksLasting;
            tickManager.onTick -= walkToBackTable;
            tickManager.onTick += cleanTable;
            tickManager.onTick += leaveTimer;
        }
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("destroyProj"))
        {
            tickManager.onTick -= walkAway;
            Destroy(gameObject);
        }
    }

    public void walkAway()
    {
        rb.MovePosition(rb.position + Vector2.left * thisTroop.isHealer.speed);
    }
    public void leaveTimer()
    {
        if(tickManager.tickCount >= leaveTick || juniorTable.currentTableHealth >= juniorTable.maxTableHealth)
        {
            tickManager.onTick -= cleanTable;
            graphic.sortingLayerName = "Troops";
            game.laneMngr.removeTroopFromLane(thisTroop, gameObject, currentLaneID);
            tickManager.onTick -= leaveTimer;
            healing = false;
            animator.Play("walk");
            tickManager.onTick += walkAway;
        }
    }

    public void cleanTable()
    {
        if(tickManager.tickCount >= nextHealTick)
        {
            juniorTable.healTable(thisTroop.isHealer.healAmount);
            var hText = Instantiate(healText, myUI.transform);
            hText.GetComponent<hurtTextScript>().damageTaken = thisTroop.isHealer.healAmount;
            nextHealTick = tickManager.tickCount + thisTroop.isHealer.ticksPerHeal;
        }
    }
    void Update()
    {
        if (healing)
        {
            healTimer -= Time.deltaTime;

            if (healTimer <= 0f)
            {
                myAudio.pitch = Random.Range(0.1f, 1.3f);
                myAudio.PlayOneShot(audioManager.instance.cleaning);
                myAudio.pitch = 1f;
                healTimer = healInterval;
            }
        }
        if (!healing)
        {
            healTimer -= Time.deltaTime;

            if (healTimer <= 0f)
            {
                myAudio.pitch = Random.Range(0.9f, 1.3f);
                myAudio.PlayOneShot(audioManager.instance.footstep);
                myAudio.pitch = 1f;
                healTimer = healInterval;
            }
        }
    }
}
