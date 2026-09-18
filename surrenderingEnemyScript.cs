using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class surrenderingEnemyScript : enemyBehaviors
{
    public Vector2 targetPos;
    public override void initStats()
    {
        
    }
    
    public override void Start()
    {
        tickManager.onTick += walkOfSurrender;
    }
    public void walkOfSurrender()
    {
        if (Vector2.Distance(rb.position, targetPos) > 0.05f) //walk until reached destination
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, 0.5f); 
            rb.MovePosition(newPos);
        }
        else
        {
            tickManager.onTick -= walkOfSurrender;
        }
    }
}
