using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floatingMessageScript : MonoBehaviour
{
    public int ticksBeforeDisappear = 15;
    private int disappearOnThisTick;
    void Start()
    {
        var rb = gameObject.GetComponent<Rigidbody2D>();
        rb.AddForce(Vector2.up * 2f, ForceMode2D.Impulse);  
        disappearOnThisTick = tickManager.tickCount + ticksBeforeDisappear;
        tickManager.onTick += disappear;
    }
    public void disappear()
    {
        if(tickManager.tickCount >= disappearOnThisTick)
        {
            tickManager.onTick -= disappear;
            Destroy(gameObject);
        }
    }
}
