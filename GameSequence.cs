using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Data;


public class GameSequence : MonoBehaviour
{
    public static int currentLevelID;
    public levelData thisLevel;
    public laneManager laneMngr;
    public saveSystem savingSystem;
    public allTroops everyTroop;
    public List<troopSO> troopsUsed;
    public List<GameObject> troopSlots;
    public userData data;
    public allLevels getLevels;
    public int firstWaveTick = 100;
    public int currentWave;
    public int waveEndTick;
    public bool advanceButton;
    public List<string> lossMessages;
    public List<troopSO> disabledTroops; //for disabling dragging certain units onto the board

    [Header("UI & Visuals")]
    public GameObject spawnGraphicsHere;
    public orangeNapkinVisual orangeVisuals;
    public TMP_Text orangeAmntDisplay;
    public TMP_Text orangePeelAmntDisplay;
    public orangeNapkinVisual orangePeelVisuals;
    public TMP_Text instructionsHere;
    public GameObject advButtonObj;
    public GameObject lossMessage; //KEY: 0 - no more oranges; 1 - table health 0
    public GameObject winScreen;
    public GameObject gameUIS; //the in-game UI such as the table health bar and the troop deployer
    public GameObject troopRemovalInst;
    public Slider waveSlider;
    public Slider timeSlider;
    public GameObject pausedScreen;
    public GameObject countdown;
    public Slider bossHealthSlider;
    public cardDeck cards; //for boss levels and other card belt levels
    public orangeConveyorBelt oConveyor; //orange conveyor belt
    public GameObject troopSelectionList; //for regular levels
    public GameObject table2; //for levels that use two tables
    public GameObject loadingScreen;
    

    [Header("Game/Runtime Variables")]
    public float oranges;
    public float orangePeels;
    public int currentEvent;
    public int tickSinceLastEvent;
    public bool troopRemovalMode;
    public int totalAttackWaves;
    public int currentAttackWave; //SEPARATE from currentevent
    public troopSO currentTroopSelected; //to support tap-to-spawn instead of only drag-to-spawn
    public float timeLeft; //only when the level is timer-based
    public weather weatherManager;
    private void Awake()
    {
        var all = FindObjectsOfType<GameSequence>();
        if (all.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        currentEvent = 0;
        tickSinceLastEvent = 0;
        currentAttackWave = 0;
        totalAttackWaves = 0;
        advanceButton = false;

        if (disabledTroops == null)
            disabledTroops = new List<troopSO>();
        else
            disabledTroops.Clear();
    }


    public void go()
    {
        Debug.Log("GameSequence GO");

        if (thisLevel.hasCountdown)
        {
            gameUIS.SetActive(false);
        }
        else
        {
            gameUIS.SetActive(true);
            Debug.Log("This level doesn't use countdown");
        }

        data = saveSystem.Load();

        weatherManager.currentWeather = thisLevel.levelWeather;

        // Preplaced troops
        if (thisLevel.preplacedTroops.Count > 0)
        {
            foreach (preplacedTroop ppt in thisLevel.preplacedTroops)
            {
                var tile = laneMngr.lanes[ppt.laneID].possibleTroopTiles[ppt.tileID];
                spawnTroop(ppt.troopToPreplace, tile.transform.position, tile);
            }
        }

        // Troop selection override
        if (!thisLevel.allowTroopSelection)
        {
            troopsUsed = new List<troopSO>(thisLevel.predeterminedTroops);
        }

        // Count waves
        foreach (levelEvent lvlEvent in thisLevel.eventsInLevel)
        {
            if (lvlEvent is attackWave)
                totalAttackWaves += 1;
        }
        totalAttackWaves -= 1; // don't count surrender wave

        // UI setup
        if (thisLevel.whatTypeOfLevel == levelType.waves)
        {
            waveSlider.gameObject.SetActive(true);
            waveSlider.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text =
                "Preparing  " + totalAttackWaves + "  wave(s)...";
        }
        else if (thisLevel.whatTypeOfLevel == levelType.time)
        {
            timeLeft = thisLevel.levelTime;
            timeSlider.gameObject.SetActive(true);
            timeSlider.maxValue = timeLeft;
            timeSlider.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text =
                "Time Left In Period: " + timeLeft;
        }
        else if (thisLevel.whatTypeOfLevel == levelType.bossBattle)
        {
            bossHealthSlider.gameObject.SetActive(true);
        }
        if(thisLevel.useSecondTable) //table 2 setting
        {
            table2.SetActive(true);
            table2.GetComponent<Animator>().Play("RollIn");
        }
        else
        {
            table2.SetActive(false);
        }
        orangePeelVisuals.gameObject.transform.position = thisLevel.peelStorageLocation;
        addOrange(thisLevel.startingOranges);
        addPeels(thisLevel.startingPeels);

        // Enemy discovery
        List<int> discoveredEnemies = new List<int>(data.enemiesDiscoveredIDs);
        foreach (enemySO enemyInv in thisLevel.enemiesInvolved)
        {
            if (!data.enemiesDiscoveredIDs.Contains(enemyInv.enemyID))
            {
                data.enemiesDiscoveredIDs.Add(enemyInv.enemyID);
                saveSystem.Save(data);
            }
                
        }
        

        // Cards mode
        if (thisLevel.usesCards)
        {
            cards.gameObject.SetActive(true);
            troopSelectionList.SetActive(false);
            cards.startDeck();
            oConveyor.gameObject.SetActive(true);
            oConveyor.startOrangeConveyor();
        }

        // ⭐ FIXED SUBSCRIPTIONS ⭐
        tickManager.onTick -= HandleTick;
        tickManager.onTick += HandleTick;

        Time.timeScale = 1f;

        tickManager.onTick -= startGame;
        tickManager.onTick += startGame;

        if (!thisLevel.usesCards)
            refreshTroopSetup();

        tickManager.allowTicks = true;
    }


    public void refreshTroopSetup()
    {
        troopSelectionList.SetActive(true);

        // Safety: troopSlots must exist
        if (troopSlots == null)
        {
            Debug.LogError("troopSlots is NULL — cannot refresh troop setup.");
            return;
        }

        // Safety: troopsUsed must exist
        if (troopsUsed == null)
            troopsUsed = new List<troopSO>();

        // ⭐ CRITICAL FIX:
        // If more troops are selected than slots exist, trim manually (no LINQ).
        if (troopsUsed.Count > troopSlots.Count)
        {
            Debug.LogWarning(
                $"refreshTroopSetup: troopsUsed ({troopsUsed.Count}) > troopSlots ({troopSlots.Count}). Trimming."
            );

            List<troopSO> trimmed = new List<troopSO>();
            for (int i = 0; i < troopSlots.Count; i++)
                trimmed.Add(troopsUsed[i]);

            troopsUsed = trimmed;
        }

        // Clear old icons
        for (int i = 0; i < troopSlots.Count; i++)
        {
            var slot = troopSlots[i];

            if (slot == null)
            {
                Debug.LogError($"troopSlots[{i}] is NULL.");
                continue;
            }

            for (int child = slot.transform.childCount - 1; child >= 0; child--)
            {
                Destroy(slot.transform.GetChild(child).gameObject);
            }
        }

        // Rebuild icons safely
        int limit = Mathf.Min(troopsUsed.Count, troopSlots.Count);

        for (int i = 0; i < limit; i++)
        {
            var troop = troopsUsed[i];
            var slot = troopSlots[i];

            if (troop == null)
            {
                Debug.LogError($"troopsUsed[{i}] is NULL.");
                continue;
            }

            if (slot == null)
            {
                Debug.LogError($"troopSlots[{i}] is NULL.");
                continue;
            }

            var icon = Instantiate(troop.troopIcon, slot.transform);

            var drag = icon.GetComponent<draggableItemScript>();
            if (drag != null)
            {
                drag.troopIDToSpawn = troop.troopID;
                drag.troopToSpawnSO = troop;
            }

            RectTransform rt = icon.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(32, 21.5f);
        }
    }

    void startGame()
    {
        if(thisLevel.levelTheme != null)
        {
            musicManager.instance.PlaySongLooped(thisLevel.levelTheme);
        }
        if (tickManager.tickCount >= 1)
        {
            tickManager.onTick -= startGame;
            //tickManager.onTick += checkOranges;
            tickSinceLastEvent = tickManager.tickCount;
            tickManager.onTick += eventRunner;
            /*if(thisLevel.whatTypeOfLevel == levelType.time)
            {
                tickManager.onTick += timePass;
            }*/
        }
    }
    public void eventRunner()
    {
        runEvents(thisLevel, tickSinceLastEvent);
    }

    public void runEvents(levelData level, int initialTick)
    {
        if (currentEvent >= level.eventsInLevel.Count)
            return;

        levelEvent thisEvent = level.eventsInLevel[currentEvent];

        switch (thisEvent.thisEventTrigger)
        {
            case trigger.ticksPassed:
                if (tickManager.tickCount >= initialTick + thisEvent.ticksPassedTrigger.ticksNeeded)
                {
                    tickManager.onTick -= eventRunner;
                    thisEvent.execute(this, laneMngr, instructionsHere.gameObject, initialTick);
                }
                break;

            case trigger.troopPlaced:
                if (laneMngr.lastTroopPlaced == thisEvent.troopPlacedTrigger.triggerTroop)
                {
                    tickManager.onTick -= eventRunner;
                    thisEvent.execute(this, laneMngr, instructionsHere.gameObject, initialTick);
                }
                break;

            case trigger.enemyKilled:
                if (laneMngr.lastEnemyKilled == thisEvent.enemyKilledTrigger.triggerEnemy)
                {
                    tickManager.onTick -= eventRunner;
                    thisEvent.execute(this, laneMngr, instructionsHere.gameObject, initialTick);
                }
                break;

            case trigger.buttonPressed:
                if (advanceButton)
                {
                    advanceButton = false;
                    tickManager.onTick -= eventRunner;
                    thisEvent.execute(this, laneMngr, instructionsHere.gameObject, initialTick);
                }
                break;
            case trigger.valueChanged:
                if (thisEvent.didValueChange(this))
                {
                    tickManager.onTick -= eventRunner;
                    thisEvent.execute(this, laneMngr, instructionsHere.gameObject, initialTick);
                }
                break;
            case trigger.eventOver:
                tickManager.onTick -= eventRunner;
                thisEvent.execute(this, laneMngr, instructionsHere.gameObject, initialTick);
                break;
        }
    }

    /*public void checkOranges()
{
        bool elvisAlive = laneMngr.allTroopsAlive.Contains(everyTroop.troops[0]);
        float elvisCost = everyTroop.troops[0].troopCost;

        if (!elvisAlive && oranges < elvisCost)
        {
            tickManager.onTick -= checkOranges;
            gameLost(lossMessages[0]);
        }
    }*/

    public void progressEvents()
{
    Debug.Log($"PROGRESS: currentEvent={currentEvent}, totalEvents={thisLevel.eventsInLevel.Count}");

    // Apply current event's disabled troops
    disabledTroops.Clear();
    disabledTroops.AddRange(thisLevel.eventsInLevel[currentEvent].dontAllowDeploy);

    // Move to the next event
    currentEvent++;

    // If we reached the end, stop
    if (currentEvent >= thisLevel.eventsInLevel.Count)
    {
        Debug.Log("All events completed");
        tickManager.onTick -= eventRunner;
        return;
    }

    // Show/hide advance button
    advButtonObj.SetActive(
        thisLevel.eventsInLevel[currentEvent].thisEventTrigger == trigger.buttonPressed
    );

    tickSinceLastEvent = tickManager.tickCount;

    // Ensure eventRunner is subscribed ONCE
    tickManager.onTick -= eventRunner;
    tickManager.onTick += eventRunner;

    // Force eventRunner to run on the NEXT tick, not THIS tick
    tickSinceLastEvent = tickManager.tickCount + 1;

}





    public void timePass()
    {
        timeLeft -= 1;
        timeSlider.value = timeLeft;
        timeSlider.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text = "Time Left In Period: "+ timeLeft;
        if(timeLeft <= 0)
        {
            tickManager.onTick -= timePass;
            gameWon(); 
        }
    }

    private void OnDisable()
    {
        tickManager.onTick -= HandleTick;
    }

    private void HandleTick()
    {
        if (currentEvent < 0 || currentEvent >= thisLevel.eventsInLevel.Count)
            return;

        thisLevel.eventsInLevel[currentEvent].tickUpdate(this, laneMngr);
    }


    public void buttonTrigger()
    {
        audioManager.instance.Play(audioManager.instance.click);
        advanceButton = true;
    }

    public void spawnTroop(troopSO troopToSpawn, Vector2 position, GameObject tile)
    {
        var troop = Instantiate(troopToSpawn.troopPrefab, position, Quaternion.identity);
        troop.transform.position = new Vector3(troop.transform.position.x, troop.transform.position.y, 0);
        troop.GetComponentInChildren<SpriteRenderer>(true).sortingOrder = tile.GetComponent<tileScript>().mySortingOrderID;
        laneMngr.addTroopToLane(troopToSpawn, troop, tile.GetComponent<tileScript>().laneID);
        troop.GetComponent<entity>().currentLaneID = tile.GetComponent<tileScript>().laneID;
        troop.GetComponent<entity>().myTile = tile.GetComponent<tileScript>();
        if(troopToSpawn.dontTakeTile == false)
        {
            tile.GetComponent<tileScript>().placeTroop();
            troop.transform.SetParent(tile.transform);
        }
    }

    public void spawnEnemy(enemySO enemyToSpawn, Vector2 position, GameObject tile)
{
    // ============================
    // 1. Check enemyToSpawn
    // ============================
    if (enemyToSpawn == null)
    {
        Debug.LogError("spawnEnemy ERROR: enemyToSpawn is NULL");
        return;
    }

    if (enemyToSpawn.enemyPrefab == null)
    {
        Debug.LogError("spawnEnemy ERROR: enemyToSpawn.enemyPrefab is NULL for enemySO: " + enemyToSpawn.name);
        return;
    }

    // ============================
    // 2. Check tile
    // ============================
    if (tile == null)
    {
        Debug.LogError("spawnEnemy ERROR: tile is NULL");
        return;
    }

    var tileScript = tile.GetComponent<tileScript>();
    if (tileScript == null)
    {
        Debug.LogError("spawnEnemy ERROR: tileScript is MISSING on tile: " + tile.name);
        return;
    }

    // ============================
    // 3. Instantiate enemy
    // ============================
    var en = Instantiate(enemyToSpawn.enemyPrefab, position, Quaternion.identity);
    if (en == null)
    {
        Debug.LogError("spawnEnemy ERROR: Instantiate returned NULL for prefab: " + enemyToSpawn.enemyPrefab.name);
        return;
    }

    en.transform.position = new Vector3(en.transform.position.x, en.transform.position.y, 20);

    // ============================
    // 4. Check entity component
    // ============================
    var ent = en.GetComponent<entity>();
    if (ent == null)
    {
        Debug.LogError("spawnEnemy ERROR: entity component is MISSING on prefab: " + enemyToSpawn.enemyPrefab.name);
        return;
    }

    // ============================
    // 5. Add to lane registry
    // ============================
    if (!enemyToSpawn.doNotCountInEnemyRegistry)
    {
        laneMngr.addEnemyToLane(enemyToSpawn, en, tileScript.laneID);
    }

    // ============================
    // 6. Assign tile + lane
    // ============================
    ent.myTile = tileScript;
    ent.currentLaneID = tileScript.laneID;

    // ============================
    // 7. Parent to tile if needed
    // ============================
    if (!enemyToSpawn.dontTakeTile)
    {
        en.transform.SetParent(tile.transform);
        tileScript.occupied = true;
    }
}

    //overload method for enemies that are spawned by bosses
    public GameObject spawnEnemy(enemySO enemyToSpawn, Vector2 position, GameObject tile, bossManager myBoss)
    {
        var en = Instantiate(enemyToSpawn.enemyPrefab, position, Quaternion.identity);
        en.transform.position = new Vector3(en.transform.position.x, en.transform.position.y, 20);
        if(enemyToSpawn.doNotCountInEnemyRegistry == false)
        {
            laneMngr.addEnemyToLane(enemyToSpawn, en, tile.GetComponent<tileScript>().laneID);
        }
        en.GetComponent<entity>().myTile = tile.GetComponent<tileScript>();
        en.GetComponent<entity>().currentLaneID = tile.GetComponent<tileScript>().laneID;
        en.GetComponent<entity>().myTile = tile.GetComponent<tileScript>();
        if(enemyToSpawn.dontTakeTile == false)
        {
            en.transform.SetParent(tile.transform);
            tile.GetComponent<tileScript>().occupied = true;
        }
        en.GetComponent<entity>().currentLaneID = tile.GetComponent<tileScript>().laneID;
        entity ent = en.GetComponent<entity>();
        enemyBehaviors enemyBehave = ent as enemyBehaviors;
        enemyBehave.bossSource = myBoss;
        //tile.GetComponent<tileScript>().occupied = true;
        return en;
    }
    //method for enemies that are spawned by summoners
    public GameObject summonEnemy(enemySO enemyToSpawn, Vector2 position, GameObject tile)
    {
        var en = Instantiate(enemyToSpawn.enemyPrefab, position, Quaternion.identity);
        en.transform.position = new Vector3(en.transform.position.x, en.transform.position.y, 20);
        if(enemyToSpawn.doNotCountInEnemyRegistry == false)
        {
            laneMngr.addEnemyToLane(enemyToSpawn, en, tile.GetComponent<tileScript>().laneID);
        }
        en.GetComponent<entity>().myTile = tile.GetComponent<tileScript>();
        en.GetComponent<entity>().currentLaneID = tile.GetComponent<tileScript>().laneID;
        en.GetComponent<entity>().myTile = tile.GetComponent<tileScript>();
        if(enemyToSpawn.dontTakeTile == false)
        {
            en.transform.SetParent(tile.transform);
            tile.GetComponent<tileScript>().occupied = true;
        }
        en.GetComponent<entity>().currentLaneID = tile.GetComponent<tileScript>().laneID;
        //tile.GetComponent<tileScript>().occupied = true;
        return en;
    }

    public void takeOrange(float amnt)
    {
        if (oranges >= amnt)
        {
            oranges -= amnt;
            orangeAmntDisplay.text = oranges.ToString();
            orangeVisuals.updateOrangesVisual();
        }
    }

    public void addOrange(float amnt)
    {
        oranges += amnt;
        orangeAmntDisplay.text = oranges.ToString();
        orangeVisuals.updateOrangesVisual();
    }

    public void addPeels(float amnt)
    {
        orangePeels += amnt;
        orangePeelAmntDisplay.text = orangePeels.ToString();
        orangePeelVisuals.updateOrangePeelVisual();
    }

    public void consumePeels(float amnt)
    {
        if(orangePeels <= 0)
            return;
        orangePeels -= amnt;
        orangePeelAmntDisplay.text = orangePeels.ToString();
        orangePeelVisuals.updateOrangePeelVisual();
    }

    public void gameLost(string messageToGive)
    {
        musicManager.instance.musicSource.Stop();
        //graphics
        lossMessage.SetActive(true);
        lossMessage.transform.GetChild(0).GetComponent<Text>().text = messageToGive;
        lossMessage.GetComponent<Animator>().Play("youlost");
        audioManager.instance.Play(audioManager.instance.defeatTheme);
    }

    public void okButton()
    {
        var ls = Instantiate(loadingScreen, gameUIS.transform);
        ls.SetActive(true);
        SceneManager.LoadScene("titleScreen");
    }

    public void gameWon()
    {
        musicManager.instance.musicSource.Stop();
        audioManager.instance.Play(audioManager.instance.victoryFanfare);
        var rewardID = thisLevel.rewardTroopID;
        gameUIS.SetActive(false);
        winScreen.SetActive(true);
        var rewardScreen = winScreen.transform.GetChild(1).gameObject;
        if(rewardID != 0)
        {     
            if(!data.troopsUnlockedIDs.Contains(rewardID)) //unlock troop if not already unlocked
            {
                troopSO rewardTroop = everyTroop.troops[rewardID];
                rewardScreen.SetActive(true);
                rewardScreen.transform.GetChild(1).GetComponent<Image>().sprite = rewardTroop.troopSprite;
                rewardScreen.transform.GetChild(2).GetComponent<TMP_Text>().text = rewardTroop.troopName;
                rewardScreen.transform.GetChild(3).GetComponent<TMP_Text>().text = rewardTroop.functionDesc;
                rewardScreen.transform.GetChild(4).GetComponent<TMP_Text>().text = "Type: " + rewardTroop.type;
                data.troopsUnlockedIDs.Add(rewardID);
            }  
        }
        int nextLevelIndex = thisLevel.levelNumber + 1;
            if (nextLevelIndex < getLevels.everyLevel.Count)
            {
                var nextLevel = getLevels.everyLevel[nextLevelIndex].thisLevel;

                if (!data.levelsUnlockedIDs.Contains(nextLevelIndex))
                    data.levelsUnlockedIDs.Add(nextLevelIndex);
            }
        if(thisLevel.unlockChapterID != 0 && !data.chaptersUnlockedIDs.Contains(thisLevel.unlockChapterID))
        {
            data.chaptersUnlockedIDs.Add(thisLevel.unlockChapterID);
        }

            saveSystem.Save(data);
    }

    public void troopRemovalModeButton()
    {
        audioManager.instance.Play(audioManager.instance.click);
        currentTroopSelected = null;
        if(troopRemovalMode == false)
        {
            troopRemovalMode = true;
            troopRemovalInst.SetActive(true);
        }
        else
        {
            troopRemovalMode = false;
            troopRemovalInst.SetActive(false);
        }
    }

    public void replayLevel()
    {
        tickManager.ClearListeners();
        tickManager.tickCount = 0;

        musicManager.instance.musicSource.Stop();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void titleScreenGo()
    {
        var ls = Instantiate(loadingScreen, gameUIS.transform);
        ls.SetActive(true);
        SceneManager.LoadScene("titleScreen");
    }

    public void pauseUnpauseButton()
    {
        audioManager.instance.Play(audioManager.instance.click);
        if(pausedScreen.activeSelf == false)
        {
            pausedScreen.SetActive(true);
            musicManager.instance.musicSource.Pause();
            Time.timeScale = 0;
        }
        else
        {
            pausedScreen.SetActive(false);
            musicManager.instance.musicSource.UnPause();
            Time.timeScale = 1;
        }
    }
}