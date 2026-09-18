using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
public enum changeToObserve
{
    general,
    increase,
    decrease
}
public enum trigger
{
    ticksPassed,
    troopPlaced,
    enemyKilled,
    buttonPressed,
    valueChanged,
    eventOver
}
[System.Serializable]
public class ticksPassedSettings
{
    public float ticksNeeded;
}
[System.Serializable]
public class troopPlacedSettings
{
    public troopSO triggerTroop;
}
[System.Serializable]
public class enemyKilledSettings
{
    public enemySO triggerEnemy;
}
[System.Serializable]
public class valueChangedSettings
{
    public bool hasInitialized;
    public valueToWatch triggerValue;
    public float valueChecking;
    public changeToObserve changeType;
}
[CreateAssetMenu(fileName = "New Level", menuName = "Orange Wars/Levels")]
public class levelData : ScriptableObject
{
    public int levelNumber;
    public string levelName;
    public Sprite levelImage;
    public bool allowTroopSelection;
    public List<enemySO> enemiesInvolved;
    public List<troopSO> predeterminedTroops;
    public List<preplacedTroop> preplacedTroops;
    public List<int> troopsNotAllowedIDs;
    public List<levelEvent> eventsInLevel;
    public int startingOranges = 20;
    public int startingPeels;
    public int maxTroopsAllowed = 6;
    public bool enemiesDropOranges;
    public levelType whatTypeOfLevel;
    public float levelTime; //for levels that are based on time
    public int rewardTroopID;
    public int unlockChapterID; //for levels that unlock next chapters
    public AudioClip levelTheme;
    public bool hasCountdown = true;
    public bool usesCards;
    public Vector2 peelStorageLocation;
    public bool useSecondTable;
    public weatherType levelWeather = weatherType.clear;
    
}

[System.Serializable]
public enum levelType
{
    waves,
    time,
    bossBattle
}

[System.Serializable]
public class preplacedTroop
{
    public troopSO troopToPreplace;
    public int laneID;
    public int tileID;
}
