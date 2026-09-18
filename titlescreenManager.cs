using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class titlescreenManager : MonoBehaviour
{
    public userData getData;
    public allLevels getLevels;
    public allTroops everyTroop;
    public allEnemies everyEnemy;
    public GameObject selectPanel;
    public GameObject lvlSelect;
    public GameObject spawnLevelCardsHere;
    public GameObject levelCardPrefab;
    public List<string> splashTexts;
    public TMP_Text splashTextDisplay;
    public GameObject settingsPanel;
    public GameObject yearbookPanel;
    public GameObject profilePanel;
    public GameObject enemyYBIcon;
    public List<string> tips;
    public float tipFrequency;
    public TMP_Text tipText;
    public GameObject chapterSelect;
    public chapterList chapters;
    public GameObject spawnChaptersHere;
    public GameObject chapterCard;
    public List<AudioClip> titleMusic;
    private bool waitingForNextSong;
    public GameObject loadingScreen;
    public Transform UItransform;
    public GameObject creditsPanel;
    public Animator creditsAnim;
    public GameObject mainPanel;

    public static titlescreenManager instance;

    private void Awake()
    {
        instance = this;
    }


    public void Start()
    {
        loadingScreen.SetActive(false);
        Time.timeScale = 1f;
        var splash = splashTexts[Random.Range(0, splashTexts.Count)];
        splashTextDisplay.text = splash;
        getData = saveSystem.Load();
        //AudioClip randomMusic = titleMusic[Random.Range(0, titleMusic.Count)];
        //musicManager.instance.PlaySongLooped(randomMusic);

        if (getData.troopsUnlockedIDs == null)
        {
            getData.troopsUnlockedIDs = new List<int>();
            saveSystem.Save(getData);
        }
            

        if (getData.levelsUnlockedIDs == null)
        {
            getData.levelsUnlockedIDs = new List<int>();
            saveSystem.Save(getData);
        }
            

        if (getData.troopsUnlockedIDs.Count == 0)
        {
            getData.troopsUnlockedIDs.Add(everyTroop.troops[0].troopID);
            getData.troopsUnlockedIDs.Add(everyTroop.troops[1].troopID);
            getData.troopsUnlockedIDs.Add(everyTroop.troops[2].troopID);
            saveSystem.Save(getData);
        }

        if(getData.chaptersUnlockedIDs.Count == 0)
        {
            getData.chaptersUnlockedIDs.Add(chapters.allChapters[0].thisChapter.chapterID);
            saveSystem.Save(getData);
        }

        if (getData.levelsUnlockedIDs.Count == 0)
        {
            getData.levelsUnlockedIDs.Add(getLevels.everyLevel[0].thisLevel.levelNumber);
            saveSystem.Save(getData);
        }
    }

    //mainly just used for music right now
    void Update()
    {
        var src = musicManager.instance.musicSource;

        if (!src.isPlaying && !waitingForNextSong)
        {
            waitingForNextSong = true;
            musicManager.instance.playRandom(titleMusic);
        }

        if (src.isPlaying && waitingForNextSong)
        {
            waitingForNextSong = false;
        }
    }

    public void callReset()
    {
        audioManager.instance.Play(audioManager.instance.click);
        saveSystem.DeleteAll();
        if (getData.troopsUnlockedIDs == null)
            getData.troopsUnlockedIDs = new List<int>();
        else
            getData.troopsUnlockedIDs.Clear();
        if (getData.enemiesDiscoveredIDs == null)
            getData.enemiesDiscoveredIDs = new List<int>();
        else
            getData.enemiesDiscoveredIDs.Clear();

        if (getData.levelsUnlockedIDs == null)
            getData.levelsUnlockedIDs = new List<int>();
        else
            getData.levelsUnlockedIDs.Clear();

        getData.troopsUnlockedIDs.Add(everyTroop.troops[0].troopID);
        getData.troopsUnlockedIDs.Add(everyTroop.troops[1].troopID);
        getData.troopsUnlockedIDs.Add(everyTroop.troops[2].troopID);

        getData.enemiesDiscoveredIDs.Add(everyEnemy.enemies[0].enemyID);
        getData.enemiesDiscoveredIDs.Add(everyEnemy.enemies[1].enemyID);

        getData.levelsUnlockedIDs.Add(getLevels.everyLevel[0].thisLevel.levelNumber);
        saveSystem.Save(getData);
        //saveSystem.Load();
    }
    public void showJson()
    {
        Debug.Log(JsonUtility.ToJson(getData, true));
    }

    

    public void openSelectionMenu()
    {
        audioManager.instance.Play(audioManager.instance.click);
        //display tips
        var randomTip = Random.Range(0, tips.Count);
        tipText.text = "TIP: "+tips[randomTip];
        StartCoroutine(tipsCoroutine());
        if(!selectPanel.activeSelf)
            selectPanel.SetActive(true);
        chapterSelect.SetActive(true);
        lvlSelect.SetActive(false);
        displayAvailableChapters();
    }
    public void openLevelSelect(chapter chosenChapter)
    {
        audioManager.instance.Play(audioManager.instance.click);
        //display tips
        var randomTip = Random.Range(0, tips.Count);
        tipText.text = "TIP: "+tips[randomTip];
        StartCoroutine(tipsCoroutine());

        if(!lvlSelect.activeSelf)
        {
            lvlSelect.SetActive(true);
            displayAvailableLevels(chosenChapter);
        }
        else
        {
            lvlSelect.SetActive(false);
        }
    }
    public void displayAvailableChapters()
    {
        if(spawnChaptersHere.transform.childCount > 0)
        {
            for(int child = 0; child < spawnChaptersHere.transform.childCount; child++)
            {
                Destroy(spawnChaptersHere.transform.GetChild(child).gameObject);
            }
        }
        foreach(chapterEntry chp in chapters.allChapters)
            {
                GameObject chCard = Instantiate(chapterCard, spawnChaptersHere.transform);
                chCard.GetComponent<chapterCardScript>().thisChapter = chp.thisChapter;
                chCard.GetComponent<chapterCardScript>().tlt = this;
                chCard.GetComponent<chapterCardScript>().thisChapter = chp.thisChapter;
                chCard.transform.GetChild(0).GetComponent<TMP_Text>().text = chp.thisChapter.chapterName;
                chCard.transform.GetChild(1).GetComponent<Image>().sprite = chp.thisChapter.chapterSprite; //Sprite
                chCard.transform.GetChild(2).GetComponent<TMP_Text>().text = "Location: " + chp.thisChapter.location; //"The Grand Level"
                chCard.transform.GetChild(3).GetComponent<TMP_Text>().text = "Difficulty: " + chp.thisChapter.difficulty;
                chCard.transform.GetChild(4).GetComponent<TMP_Text>().text = "Levels: " + chp.thisChapter.levelsInChapter.Count;

                if(!getData.chaptersUnlockedIDs.Contains(chp.thisChapter.chapterID))
                {
                    chCard.GetComponent<chapterCardScript>().buttonTxt.transform.parent.GetComponent<Button>().interactable = false;
                    chCard.GetComponent<chapterCardScript>().buttonTxt.text = "Locked";
                }
            }
    }
    public void displayAvailableLevels(chapter whichChapter)
    {
        audioManager.instance.Play(audioManager.instance.click);
        chapterSelect.SetActive(false);
        lvlSelect.SetActive(true);
        if(spawnLevelCardsHere.transform.childCount > 0)
        {
            for(int child = 0; child < spawnLevelCardsHere.transform.childCount; child++)
            {
                Destroy(spawnLevelCardsHere.transform.GetChild(child).gameObject);
            }
        }
        foreach(levelEntry lvl in whichChapter.levelsInChapter)
        {
            GameObject levelCard = Instantiate(levelCardPrefab, spawnLevelCardsHere.transform);
            levelCard.GetComponent<levelCardScript>().thisLevelData = lvl.thisLevel;
            levelCard.GetComponent<levelCardScript>().tlt = this;
            levelCard.GetComponent<levelCardScript>().thisLevelData = lvl.thisLevel;
            levelCard.transform.GetChild(0).GetComponent<TMP_Text>().text = "Day "+ lvl.thisLevel.levelNumber; //Day X
            levelCard.transform.GetChild(1).GetComponent<Image>().sprite = lvl.thisLevel.levelImage; //Sprite
            levelCard.transform.GetChild(3).GetComponent<TMP_Text>().text = "''" + lvl.thisLevel.levelName + "''"; //"The Grand Level"

            if(!getData.levelsUnlockedIDs.Contains(lvl.thisLevel.levelNumber))
            {
                levelCard.GetComponent<levelCardScript>().buttonTxt.transform.parent.GetComponent<Button>().interactable = false;
                levelCard.GetComponent<levelCardScript>().buttonTxt.text = "Locked";
            }
        }
    }
    public void levelSelect()
    {
        
    }
    public void resetJSON()
    {
        audioManager.instance.Play(audioManager.instance.click);
        saveSystem.DeleteAll();
        Debug.Log("JSON file deleted!");
    }

    public void settingPanel()
    {
        audioManager.instance.Play(audioManager.instance.click);
        if(settingsPanel.activeSelf == false)
        {
            settingsPanel.SetActive(true);
        }
        else
        {
            settingsPanel.SetActive(false);
        }
    }

    public void loadLevel(levelData levelToLoad, levelCardScript card)
    {
        musicManager.instance.musicSource.Stop();
        musicManager.instance.playTitleMusic = false;
        var ls = Instantiate(loadingScreen, UItransform);
        ls.SetActive(true);
        //loadingScreen.SetActive(true);
        audioManager.instance.Play(audioManager.instance.click);
        card.buttonTxt.text = "loading...";
        GameSequence.currentLevelID = levelToLoad.levelNumber;

        StartCoroutine(loadInAFewFrames(true)); // true = apply delay
    }


    public void resetSoundSettings()
    {
        audioManager.instance.Play(audioManager.instance.click);
        saveSystem.resetSoundSettings(getData);
    }

    public void openCloseYearbook()
    {
        if(yearbookPanel.activeSelf == false)
        {
            audioManager.instance.Play(audioManager.instance.pageTurn);
            yearbookPanel.SetActive(true);
        }
        else
        {
            audioManager.instance.Play(audioManager.instance.closeYearbook);
            yearbookPanel.SetActive(false);
        }
    }

    public void nextYBPage(string whatPage)
    {
        audioManager.instance.Play(audioManager.instance.pageTurn);
        if(whatPage == "j")
        {
            yearbookPanel.transform.GetChild(0).gameObject.SetActive(false);
            yearbookPanel.transform.GetChild(2).gameObject.SetActive(false);
            yearbookPanel.transform.GetChild(1).gameObject.SetActive(true);
            displayAllTroopsYB();
        }
        if(whatPage == "s")
        {
            yearbookPanel.transform.GetChild(0).gameObject.SetActive(false);
            yearbookPanel.transform.GetChild(0).gameObject.SetActive(false);
            yearbookPanel.transform.GetChild(2).gameObject.SetActive(true);
            displayAllEnemiesYB();
        }
        if(whatPage == "first")
        {
            profilePanel.SetActive(false);
            yearbookPanel.transform.GetChild(1).gameObject.SetActive(false);
            yearbookPanel.transform.GetChild(2).gameObject.SetActive(false);
            yearbookPanel.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    public void displayAllTroopsYB()
    {
        //profilePanel.SetActive(false);
        Transform spawnIconsHere = yearbookPanel.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0);
        if(spawnIconsHere.childCount > 0)
        {
            return;
        }
        foreach(troopSO troop in everyTroop.troops)
        {
            var icon = Instantiate(troop.troopIcon, spawnIconsHere);
            var layoutE = icon.AddComponent<LayoutElement>();
            layoutE.preferredHeight = 50f;
            layoutE.preferredWidth = 50f;
            // Disable drag script for selection screen
            var drag = icon.GetComponent<draggableItemScript>();
            if (drag != null)
                Destroy(drag);
            bool unlocked = false;

            foreach (int tID in getData.troopsUnlockedIDs)
            {
                if (tID == troop.troopID)
                {
                    unlocked = true;
                    break;
                }
            }

            if (!unlocked)
            {
                icon.transform.GetChild(0).GetComponent<Image>().color = Color.black;
            }

            troopSO troopRef = troop;

            icon.AddComponent<Button>().onClick.AddListener(()=> showTroopProfile(troopRef));
        }
    }
    public void displayAllEnemiesYB()
    {
        //profilePanel.SetActive(false);
        Transform spawnIconsHere = yearbookPanel.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0);
        if(spawnIconsHere.childCount > 0)
        {
            return;
        }
        foreach(enemySO enemy in everyEnemy.enemies)
        {
            var icon = Instantiate(enemyYBIcon, spawnIconsHere);
            icon.transform.GetChild(0).GetComponent<Image>().sprite = enemy.enemySprite;
            icon.SetActive(false);
            icon.SetActive(true);
            bool discovered = false;

            foreach (int eID in getData.enemiesDiscoveredIDs)
            {
                if (eID == enemy.enemyID)
                {
                    discovered = true;
                    break;
                }
            }

            if (!discovered)
            {
                icon.transform.GetChild(0).GetComponent<Image>().color = Color.black;
            }

            enemySO enemyRef = enemy;
            icon.AddComponent<Button>().onClick.AddListener(()=> showEnemyProfile(enemyRef));
        }
    }

    public void showTroopProfile(troopSO troopRef)
    {
        audioManager.instance.Play(audioManager.instance.pageTurn);
        if(profilePanel.activeSelf == false)
        {
            profilePanel.SetActive(true);
        }
        bool unlocked = false;

        foreach (int tID in getData.troopsUnlockedIDs)
        {
            if (tID == troopRef.troopID)
            {
                unlocked = true;
                break;
            }
        }
        if (!unlocked)
        {
            profilePanel.transform.GetChild(1).GetComponent<Image>().sprite = troopRef.troopSprite;
            profilePanel.transform.GetChild(1).GetComponent<Image>().color = Color.black; //make silouhette
            profilePanel.transform.GetChild(0).GetComponent<TMP_Text>().text = "Locked";
            profilePanel.transform.GetChild(2).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(3).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(6).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(7).GetComponent<TMP_Text>().text = "???";
            return;
        }
        profilePanel.transform.GetChild(0).GetComponent<TMP_Text>().text = troopRef.troopName; //troop name
        profilePanel.transform.GetChild(1).GetComponent<Image>().sprite = troopRef.troopSprite; //troop sprite
        profilePanel.transform.GetChild(1).GetComponent<Image>().color = Color.white; //make visible
        profilePanel.transform.GetChild(2).GetComponent<TMP_Text>().text = "Type: "+ troopRef.type; //troop type
        profilePanel.transform.GetChild(3).GetComponent<TMP_Text>().text = "Health: "+ troopRef.maxHealth; //troop health
        //*** variables for calculating total damage
        troopBehaviors tBehave = troopRef.troopPrefab.GetComponent<entity>() as troopBehaviors;
        float totalDamage = 0f;
        switch(troopRef.type)
        {
            case troopType.shooter:
                totalDamage = tBehave.calculateTotalDamage(troopRef.isShooter.projectile.GetComponent<projectileScript>().baseDamage, troopRef.isShooter.shootForce);
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+totalDamage;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(s): "+troopRef.isShooter.tickBeforeNextShot/10f;
                break;
            case troopType.lobber:
                if(troopRef.isLobber.projectile == null)
                    profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: Varies, modded by "+ troopRef.isLobber.lobForce;
                else
                {
                    totalDamage = tBehave.calculateTotalDamage(troopRef.isLobber.projectile.GetComponent<projectileScript>().baseDamage, troopRef.isLobber.lobForce);
                    profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+totalDamage;
                }
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(s): "+troopRef.isLobber.tickBeforeNextLob/10f;
                break;
            case troopType.producer:
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Production Amnt: "+troopRef.isProducer.productionAmount;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(s): "+troopRef.isProducer.productionTicks/10f;
                break;
            case troopType.logistics:
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Retrieve Amnt: "+troopRef.isLogistics.retrieveAmount;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(s): "+troopRef.isLogistics.retrieveTicks/10f;
                break;
            case troopType.intelligence:
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Buff Duration(s): "+troopRef.isIntelligence.myBuff.duration/10f;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Buff Rate(s): "+troopRef.isIntelligence.ticksBetweenBuffs/10f;
                break;
            case troopType.tank:
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: None(Full Tank)";
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(s): 0(Full Tank)";
                break;
            case troopType.healer:
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Heal Amount: "+troopRef.isHealer.healAmount;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Heal Lasting(s): "+troopRef.isHealer.ticksLasting/10f;
                break;
            case troopType.magdumper:
                totalDamage = tBehave.calculateTotalDamage(troopRef.isMagdumper.projectile.GetComponent<projectileScript>().baseDamage, troopRef.isMagdumper.shootForce);
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+ totalDamage;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Reload Speed(s): "+troopRef.isMagdumper.reloadTicks/10f;
                break;
            case troopType.clickshot:
                totalDamage = tBehave.calculateTotalDamage(troopRef.isShooter.projectile.GetComponent<projectileScript>().baseDamage, troopRef.isShooter.shootForce);
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+totalDamage;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(s): Player CPS";
                break;
            case troopType.dynamic:
                string damageDisp = ""; 
                foreach(GameObject proj in troopRef.isDynamic.projectiles)
                {
                    damageDisp = damageDisp + proj.GetComponent<projectileScript>().baseDamage;
                    if(troopRef.isDynamic.projectiles.IndexOf(proj) != troopRef.isDynamic.projectiles.Count - 1)
                    {
                        damageDisp = damageDisp + ", ";
                    }
                }
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+damageDisp;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(s): "+ troopRef.isShooter.tickBeforeNextShot;
                break;
            case troopType.chemical:
                totalDamage = tBehave.calculateTotalDamage(troopRef.isLobber.projectile.GetComponent<projectileScript>().baseDamage, troopRef.isLobber.lobForce);
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+totalDamage+" + "+troopRef.isChemical.damageDisp.ToString();
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(s): "+ troopRef.isLobber.tickBeforeNextLob/10f;
                break;
        }
        profilePanel.transform.GetChild(6).GetComponent<TMP_Text>().text = troopRef.functionDesc;
        profilePanel.transform.GetChild(7).GetComponent<TMP_Text>().text = troopRef.troopDesc;
    }
    public void showEnemyProfile(enemySO enemyRef)
    {
        audioManager.instance.Play(audioManager.instance.pageTurn);
        if(profilePanel.activeSelf == false)
        {
            profilePanel.SetActive(true);
        }
        bool discovered = false;

        foreach (int eID in getData.enemiesDiscoveredIDs)
        {
            if (eID == enemyRef.enemyID)
            {
                discovered = true;
                break;
            }
        }
        if (!discovered)
        {
            profilePanel.transform.GetChild(1).GetComponent<Image>().sprite = enemyRef.enemySprite;
            profilePanel.transform.GetChild(1).GetComponent<Image>().color = Color.black; //make silouhette
            profilePanel.transform.GetChild(0).GetComponent<TMP_Text>().text = "Not Yet Discovered";
            profilePanel.transform.GetChild(2).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(3).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(6).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(7).GetComponent<TMP_Text>().text = "???";
            return;
        }
        foreach(int eID in getData.enemiesDiscoveredIDs) //check if discovered
        {
            if(eID == enemyRef.enemyID)
            {
                break;
            }
            profilePanel.transform.GetChild(1).GetComponent<Image>().sprite = enemyRef.enemySprite;
            profilePanel.transform.GetChild(1).GetComponent<Image>().color = Color.black; //make silouhette
            profilePanel.transform.GetChild(2).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(3).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(6).GetComponent<TMP_Text>().text = "???";
            profilePanel.transform.GetChild(7).GetComponent<TMP_Text>().text = "???";
        }
        profilePanel.transform.GetChild(0).GetComponent<TMP_Text>().text = enemyRef.enemyName; //enemy name
        profilePanel.transform.GetChild(1).GetComponent<Image>().sprite = enemyRef.enemySprite; //enemy sprite
        profilePanel.transform.GetChild(1).GetComponent<Image>().color = Color.white; //make visible
        profilePanel.transform.GetChild(2).GetComponent<TMP_Text>().text = "Type: "+ enemyRef.type; //enemy type
        profilePanel.transform.GetChild(3).GetComponent<TMP_Text>().text = "Health: "+ enemyRef.maxHealth; //enemy health
        //*** variables for calculating total damage
        enemyBehaviors eBehave = enemyRef.enemyPrefab.GetComponent<entity>() as enemyBehaviors;
        float totalDamage = 0f;
        switch(enemyRef.type)
        {
            case enemyType.shooter:
                totalDamage = eBehave.calculateTotalDamage(enemyRef.isShooter.projectile.GetComponent<projectileScript>().baseDamage, enemyRef.isShooter.shootForce);
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+totalDamage;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(s): "+enemyRef.isShooter.tickBeforeNextShot/10f;
                break;
            case enemyType.lobber:
                totalDamage = eBehave.calculateTotalDamage(enemyRef.isLobber.projectile.GetComponent<projectileScript>().baseDamage, enemyRef.isLobber.lobForce);
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+totalDamage;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(s): "+enemyRef.isLobber.tickBeforeNextLob/10f;
                break;
            case enemyType.tank:
                if(enemyRef.isTank.shieldHealth != 0)
                {
                    profilePanel.transform.GetChild(3).GetComponent<TMP_Text>().text = "Health: "+ enemyRef.maxHealth+"+"+enemyRef.isTank.shieldHealth+"(Shield)";
                }
                if(enemyRef.isShooter.shootForce != 0)
                {
                    totalDamage = eBehave.calculateTotalDamage(enemyRef.isShooter.projectile.GetComponent<projectileScript>().baseDamage, enemyRef.isShooter.shootForce);
                    profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+totalDamage;
                    profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(s): "+enemyRef.isShooter.tickBeforeNextShot/10f;
                }
                else
                {
                    profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: 0(Full Tank)";
                    profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(s): 0(Full Tank)";
                }
                break;
            case enemyType.footman:
                if(enemyRef.isTank.shieldHealth != 0)
                {
                    profilePanel.transform.GetChild(3).GetComponent<TMP_Text>().text = "Health: "+ enemyRef.maxHealth+"+"+enemyRef.isTank.shieldHealth+"(Shield)"; 
                }
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+enemyRef.isFootman.damage;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(steps/s): "+enemyRef.isFootman.speed;
                break;
            case enemyType.boss:
                if(enemyRef.isTank.shieldHealth != 0)
                {
                    profilePanel.transform.GetChild(3).GetComponent<TMP_Text>().text = "Health: "+ enemyRef.maxHealth+"+"+enemyRef.isTank.shieldHealth+"(Shield)"; 
                }
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+enemyRef.isBoss.damageDisplay;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Phases: "+enemyRef.isBoss.phases;
                break;
            case enemyType.barrage:
                if(enemyRef.isTank.shieldHealth != 0)
                {
                    profilePanel.transform.GetChild(3).GetComponent<TMP_Text>().text = "Health: "+ enemyRef.maxHealth+"+"+enemyRef.isTank.shieldHealth+"(Shield)"; 
                }
                totalDamage = eBehave.calculateTotalDamage(enemyRef.isShooter.projectile.GetComponent<projectileScript>().baseDamage, enemyRef.isBarrage.shootForce);
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+ totalDamage+" x"+enemyRef.isBarrage.ammoInBarrage;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed: "+enemyRef.isBarrage.secsBetweenAttacks+"(Secs between attacks), "
                +enemyRef.isBarrage.ticksBetweenBarr/10f+"(Secs between barrages)";
                break;
            case enemyType.compound:
                if(enemyRef.isTank.shieldHealth != 0)
                {
                    profilePanel.transform.GetChild(3).GetComponent<TMP_Text>().text = "Health: "+ enemyRef.maxHealth+"+"+enemyRef.isTank.shieldHealth+"(Shield)"; 
                }
                totalDamage = eBehave.calculateTotalDamage(enemyRef.isShooter.projectile.GetComponent<projectileScript>().baseDamage, enemyRef.isShooter.shootForce);
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+totalDamage+"(Shooting), "
                +enemyRef.isFootman.damage+"(Punching)";
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed: "+enemyRef.isShooter.tickBeforeNextShot+"(Shooting), "
                +enemyRef.isFootman.speed/10f+"(Walking(s))";
                break;
            case enemyType.summoner:
                if(enemyRef.isTank.shieldHealth != 0)
                {
                    profilePanel.transform.GetChild(3).GetComponent<TMP_Text>().text = "Health: "+ enemyRef.maxHealth+"+"+enemyRef.isTank.shieldHealth+"(Shield)"; 
                }
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Damage: "+ enemyRef.isFootman.damage;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed: "+enemyRef.isBarrage.secsBetweenAttacks+"(Secs between attacks), "
                +enemyRef.isBarrage.ticksBetweenBarr/10f+"(Secs between barrages)";
                break;
            case enemyType.sabotage:
                profilePanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Amount Take/Damage: "+ enemyRef.isSabotage.amountTaking;
                profilePanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Speed(steps/s): "+enemyRef.isFootman.speed;
                break;
        }
        profilePanel.transform.GetChild(6).GetComponent<TMP_Text>().text = enemyRef.functionDesc;
        profilePanel.transform.GetChild(7).GetComponent<TMP_Text>().text = enemyRef.enemyDesc;
    }

    public void backButton()
    {
        audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.click);
        if(creditsPanel.activeSelf && !mainPanel.activeSelf)
        {
            creditsPanel.SetActive(false);
            mainPanel.SetActive(true);
        }
        if(lvlSelect.activeSelf && !chapterSelect.activeSelf)
        {
            lvlSelect.SetActive(false);
            chapterSelect.SetActive(true);
        }
        else if(chapterSelect.activeSelf && !lvlSelect.activeSelf)
        {
            lvlSelect.SetActive(false);
            chapterSelect.SetActive(false);
            selectPanel.SetActive(false);
        }
    }

    IEnumerator tipsCoroutine()
    {
        yield return new WaitForSeconds(tipFrequency);
        var randomTip = Random.Range(0, tips.Count);
        tipText.text = "TIP: "+tips[randomTip];
        StartCoroutine(tipsCoroutine());
    }

    private IEnumerator loadInAFewFrames(bool applyDelay)
    {
        if (applyDelay)
        {
            yield return null;
            yield return null;
            yield return null; //wait 3 frames
        }

        SceneManager.LoadScene("orangewars");
    }

    public void rollCredits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
        creditsAnim.Play("rollCredits");
    }

    public void callDownloadSave()
    {
        audioManager.instance.Play(audioManager.instance.click);
        saveSystem.downloadSave();
    }

    public void callImportSave()
    {
        audioManager.instance.Play(audioManager.instance.click);
        saveSystem.LoadSaveFromFile();
    }

    public void skipToLevel(int levelNum)
    {
        for(int i = 0; i < levelNum; i++)
        {
            getData.levelsUnlockedIDs.Add(getLevels.everyLevel[i].thisLevel.levelNumber);
        }

        saveSystem.Save(getData);
    }

    public void ApplySaveData(userData data)
    {
        // Replace runtime data
        getData = data;

        // Refresh chapter UI
        displayAvailableChapters();

        // Refresh level UI (only if level select is open)
        if (lvlSelect.activeSelf)
        {
            // You need the currently selected chapter
            // If none is selected, just skip
            chapter currentChapter = chapters.allChapters[0].thisChapter;
            displayAvailableLevels(currentChapter);
        }

        // Refresh yearbook troop icons
        displayAllTroopsYB();

        // Refresh yearbook enemy icons
        displayAllEnemiesYB();

        // Refresh sound settings
        musicManager.instance.musicSource.volume = data.musicVol;
        audioManager.instance.sfxSource.volume = data.sfxVol;
    }

}
