using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class tickManager : MonoBehaviour
{
    public static event Action onTick;
    public static int tickCount = 0;
    public static bool allowTicks = false;


    public float tickRate;
    private float timer;
    private void Awake()
    {
        // Fix alternating retry bug
        onTick = null;
        tickCount = 0;
    }

    void Update()
    {
        if (!allowTicks)
            return;

        timer += Time.deltaTime;
        float interval = 1f / tickRate;

        if (timer >= interval)
        {
            timer -= interval;
            tickCount++;

            if (onTick != null)
            {
                foreach (var d in onTick.GetInvocationList())
                {
                    var target = d.Target as UnityEngine.Object;

                    if (target == null)
                    {
                        onTick -= (Action)d;
                        continue;
                    }

                    d.DynamicInvoke();
                }
            }
        }
    }


    public static void ResetTicks()
    {
        onTick = null;
        tickCount = 0;
    }

    public static void ClearListeners()
    {
        onTick = null;
    }


}
