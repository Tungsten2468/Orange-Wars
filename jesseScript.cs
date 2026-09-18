using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jesseScript : troopBehaviors
{
    public GameObject troopToBoost;
    public int tickSinceLastBuff;
    public float typeTimer;
    public float typeInterval;
    public override void initStats()
    {
        
    }
    public override void Start()
    {
        base.Start();
        laneMngr = GameObject.Find("gameManager").GetComponent<laneManager>();
        tickManager.onTick += waitForTroop;
    }
    public void waitForTroop()
    {
        //don't do jack if placed on the front tile(no more troops in front of him)
        if(myTile.tileID == laneMngr.lanes[currentLaneID].possibleTroopTiles.Count - 1)
        {
            return;
        }

        var nextTile = laneMngr.lanes[myTile.laneID].possibleTroopTiles[myTile.tileID + 1].GetComponent<tileScript>();
        if(nextTile.occupied)
        {
            if(nextTile.transform.childCount <= 0)
            {
                Debug.Log("This tile has no child!");
                return;
            }
            troopToBoost = nextTile.transform.GetChild(0).gameObject;
            tickManager.onTick -= waitForTroop;
            tickSinceLastBuff = tickManager.tickCount;
            buffTroop();
        }
    }

    public void waitForBuff()
    {
        if(troopToBoost == null)
        {
            tickManager.onTick += waitForTroop;
            return;
        }
        if(tickManager.tickCount >= tickSinceLastBuff + thisTroop.isIntelligence.ticksBetweenBuffs)
        {
            buffTroop();
        }
    }
    public void buffTroop()
    {
        if(troopToBoost == null)
        {
            tickManager.onTick += waitForTroop;
            return;
        }
        statusEffectManager troopStatus = troopToBoost.GetComponent<statusEffectManager>();
        if(troopStatus != null)
        {
            troopStatus.beginEffect(thisTroop.isIntelligence.myBuff);
        } 
        tickSinceLastBuff = tickManager.tickCount;
        tickManager.onTick += waitForBuff;
    }

    void Update()
    {
        typeTimer -= Time.deltaTime;

        if (typeTimer <= 0f)
        {
            audioManager.instance.sfxSource.pitch = Random.Range(0.9f, 1.3f);
            audioManager.instance.Play(audioManager.instance.footstep);
            audioManager.instance.sfxSource.pitch = 1f;
            typeTimer = typeInterval;
        }
    }
    
}
