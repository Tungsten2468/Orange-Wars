using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class sunscreenScript : enemyBehaviors
{
    public int sunscreenTick;
    public collectMyEnemyFriends getEnemiesAround;
    public statusEffect myEffect;

    public override void initStats()
    {
        
    }

    public override void Start()
    {
        base.Start();
        sunscreenTick = tickManager.tickCount + 20;
        tickManager.onTick += sunscrnTick;
    }

    public void sunscrnTick()
    {
        if(tickManager.tickCount >= sunscreenTick)
        {
            giveSunscreen();
            sunscreenTick = tickManager.tickCount + thisEnemy.isChemical.chemicalTicksLasting;
        }
    }

    public void giveSunscreen()
    {
        animator.Play("squirt");
        foreach(enemyBehaviors enemy in getEnemiesAround.enemiesInArea)
        {
            if(enemy.GetComponent<statusEffectManager>() == null)
                continue;
            enemy.GetComponent<statusEffectManager>().beginEffect(myEffect);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        tickManager.onTick -= sunscrnTick;
    }
}
