using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleLasting : MonoBehaviour
{
    public int lastingTicks = 5;
    private int disappearTick;
    public ParticleSystem particles;
    public void Start()
    {
        particles.Play();
        disappearTick = tickManager.tickCount + lastingTicks;
        tickManager.onTick += disappear;
    }
    public void disappear()
    {
        if(tickManager.tickCount >= disappearTick)
        {
            tickManager.onTick -= disappear;
            Destroy(gameObject);
        }
    }
}
