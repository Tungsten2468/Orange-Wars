using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
[System.Serializable]
public enum weatherType 
{
    clear, 
    storm
}
public class weather : MonoBehaviour
{
    public weatherType currentWeather;
    public Light2D globalLight;
    [Header("Weather Settings")]
    public int ticksBetweenThunder;
    public int thunderTicks;
    private List<AudioClip> thunderSounds;
    public void Awake()
    {
        thunderSounds = new List<AudioClip>
        {
            audioManager.instance.thunderClap1,
            audioManager.instance.thunderClap2,
            audioManager.instance.thunderClap3,
            audioManager.instance.thunderClap4
        };
    }

   
    public void Start()
    {
        switch(currentWeather)
        {
            case weatherType.clear:
                globalLight.intensity = 1f;
                break;
            case weatherType.storm:
                globalLight.intensity = 0.1f;
                globalLight.gameObject.GetComponent<Animator>().Play("dark");
                thunderTicks = tickManager.tickCount + ticksBetweenThunder;
                break;
        }

        tickManager.onTick += doWeather;
    }
    public void doWeather()
    {
        switch(currentWeather)
        {
            case weatherType.storm:
                storm();
                break;
        }
    }

    public void storm()
    {
        if(tickManager.tickCount >= thunderTicks)
        {
            globalLight.gameObject.GetComponent<Animator>().Play("thunder");
            audioManager.instance.Play(thunderSounds[Random.Range(0, thunderSounds.Count - 1)]);
            thunderTicks = tickManager.tickCount + ticksBetweenThunder;
        }
    }

    public void OnDisable()
    {
        tickManager.onTick -= doWeather;
    }
}
