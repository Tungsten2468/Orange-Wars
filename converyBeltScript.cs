using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class converyBeltScript : MonoBehaviour
{
    public GameSequence game;
    public Transform spawnIconsHere;
    public int spawnInterval;
    public float defaultMaxYLevel = -30f;
    public float currentmaxYLevel;
    public int maxAmntIcons;
    public int nextSpawnTick;
    public List<GameObject> iconsOnConveyor;
    public void startConveyor()
    {
        nextSpawnTick = tickManager.tickCount + spawnInterval;
        tickManager.onTick += spawnIconsLoop;
    }

    public void spawnIconsLoop()
    {
        if(tickManager.tickCount >= nextSpawnTick)
        {
            tickManager.onTick -= spawnIconsLoop;
            int randomIcon = Random.Range(0, game.thisLevel.predeterminedTroops.Count -1);
            var icon = Instantiate(game.thisLevel.predeterminedTroops[randomIcon].troopIcon, spawnIconsHere);
            var citem = icon.AddComponent<conveyorItem>();
            citem.conveyorBelt = this;
            currentmaxYLevel = defaultMaxYLevel;
            for(int i = 0; i < iconsOnConveyor.Count; i++)
            {
                currentmaxYLevel -= 30f;
            }
            nextSpawnTick = tickManager.tickCount + spawnInterval;
            tickManager.onTick += spawnIconsLoop;
        }
    }

    /*void Update()
    {
        // Move upward
        scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime;

        // Stop when reaching the top
        if (stopAtTop && scrollRect.verticalNormalizedPosition <= 0f)
        {
            scrollRect.verticalNormalizedPosition = 0f;
            enabled = false; // disables this script so it stops scrolling
        }
    }*/
}
