using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;

[CreateAssetMenu(menuName = "Orange Wars/Level Events/Randomized Attack Wave")]
public class randomizedAttackWave : attackWave
{
    public int maxPayloadAmount = 3;
    public Stack<payload> payloadPool;
    public Stack<payLoadEntry> enemyEntryPool;
    public float initialDifficulty;
    public float waveDifficulty;
    public float maxDifficulty;
    public List<int> allowedLanes;
    public float timeElapsed;
    public override void execute(GameSequence game, laneManager laneMngr, GameObject instructionsObj, int initialTick)
    {
        tickManager.onTick += game.timePass;
        if (payloadPool == null)
            payloadPool = new Stack<payload>();
        if (enemyEntryPool == null)
            enemyEntryPool = new Stack<payLoadEntry>();

        payloads.Clear(); //reset payloads
        game.disabledTroops = new List<troopSO>(dontAllowDeploy);

        clearUI(game, instructionsObj, game.spawnGraphicsHere);

        if(message != "")
        {
            instructionsObj.transform.parent.gameObject.SetActive(true);
            instructionsObj.GetComponent<TMP_Text>().text = message;
        }
        isRunning = true;
        generateRandomPayloads(game);

        currentPayload = 0;
        firstTick = tickManager.tickCount;

        SpawnPayload(game, payloads[currentPayload]);
    }
    public override void tickUpdate(GameSequence game, laneManager laneMngr)
    {
        if (!isRunning)
            return;
        timeElapsed += 1;

        int ticksPassed = tickManager.tickCount - firstTick;

        if (currentPayload < payloads.Count - 1)
        {
            if (ticksPassed >= payloads[currentPayload].ticksTillNextPayload)
            {
                currentPayload++;
                SpawnPayload(game, payloads[currentPayload]);
                firstTick = tickManager.tickCount;
            }

            return;
        }
        if (laneMngr.allEnemiesAlive.Count <= 0)
        {
            isRunning = false;
            foreach(payload pload in payloads)
            {
                foreach(payLoadEntry pEntry in pload.enemies)
                {
                    enemyEntryPool.Push(pEntry);
                }
                pload.enemies.Clear();
                payloadPool.Push(pload);

                float diffOscillation = 
                initialDifficulty + (maxDifficulty - initialDifficulty) * 
                (math.sin(math.PI * timeElapsed/game.thisLevel.levelTime)+1)/2;

                waveDifficulty = Mathf.Lerp(initialDifficulty, maxDifficulty, diffOscillation);
                //float progress = ticksPassed * 2 / GameSequence.thisLevel.levelTime;
                //waveDifficulty = Mathf.Lerp(initialDifficulty, maxDifficulty, progress);

            }
            payloads.Clear();

            if(game.timeLeft > 0)
            {
                generateRandomPayloads(game);
                isRunning = true;
            }
        }
    }
    public void generateRandomPayloads(GameSequence game)
    {
        int randomPayloadAmount = UnityEngine.Random.Range(1, maxPayloadAmount + 1);
        for (int p = 0; p < randomPayloadAmount; p++)
        {
            payload thePayload;
            if (payloadPool.Count > 0)
                thePayload = payloadPool.Pop();
            else
                thePayload = new payload();
            
            if (thePayload.enemies == null)
                thePayload.enemies = new List<payLoadEntry>();
            else
                thePayload.enemies.Clear();

            int enemyCount = UnityEngine.Random.Range((int)waveDifficulty + 1, (int)waveDifficulty + 3);

            var eList = appropriateEnemies(game);
            if (eList.Count == 0)
            {
                Debug.LogWarning("No enemies available for difficulty " + waveDifficulty);
                continue;
            }


            int randomLane = allowedLanes[UnityEngine.Random.Range(0, allowedLanes.Count)];

            for (int i = 0; i < enemyCount; i++)
            {
                var template = eList[UnityEngine.Random.Range(0, eList.Count)];

                payLoadEntry newPayloadEntry;
                if(enemyEntryPool.Count > 0)
                {
                    newPayloadEntry = enemyEntryPool.Pop();
                }
                else
                {
                    newPayloadEntry = new payLoadEntry();
                }

                newPayloadEntry.enemy = template;
                newPayloadEntry.laneToSpawn = randomLane;

                thePayload.enemies.Add(newPayloadEntry);
            }

            // spacing between payloads
            thePayload.ticksTillNextPayload = UnityEngine.Random.Range(20, 60);
            payloads.Add(thePayload);
        }

        currentPayload = 0;
        firstTick = tickManager.tickCount;
    }


    public List<enemySO> appropriateEnemies(GameSequence game)
    {
        List<enemySO> appropriateEnemyList = new List<enemySO>();
        foreach(enemySO enemy in game.thisLevel.enemiesInvolved)
        {
            if(enemy.difficultyTier <= waveDifficulty)
            {
                appropriateEnemyList.Add(enemy);
            }
        }
        return appropriateEnemyList;
    }
}
