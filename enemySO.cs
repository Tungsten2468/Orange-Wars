using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Orange Wars/Enemies")]
public class enemySO : ScriptableObject
{
    public string enemyName;
    public int enemyID;
    public enemyType type;
    public string functionDesc; //what does he do?
    public string enemyDesc; //fun
    public float maxHealth;
    public Sprite enemySprite;
    public GameObject enemyPrefab;
    public float spawnChance;
    public bool doNotCountInEnemyRegistry;
    public bool takeFirstAvailable; //do not take a random seat, take the front-most available one
    public bool dontTakeTile; //dont use up a tile
    public shooterModule isShooter;
    public lobberModule isLobber;
    public footmanModule isFootman;
    public tankModule isTank;
    public barrageModule isBarrage;
    public compoundModule isCompound;
    public summonerModule isSummoner;
    public sabotageModule isSabotage;
    public dynamicModule isDynamic;
    public chemicalModule isChemical;
    public bossModule isBoss;
    public int difficultyTier; //0 = easy; 1 = medium; 2 = hard; 3 = very hard
    public bool iAmBoss;
}

[System.Serializable]
public enum enemyType
{
    shooter,
    lobber,
    footman,
    tank,
    boss,
    barrage,
    compound,
    summoner,
    sabotage,
    dynamic,
    chemical
}

[System.Serializable]
public class footmanModule
{
    public float speed;
    public float damage;
    public int ticksBetweenAttacks;
    public AudioClip attackSound;
    public AudioClip walkingSound;
}

[System.Serializable]
public class bossModule
{
    public float bossEntranceTicks;
    public AudioClip entranceSound;
    public AudioClip defeatSound;
    public string damageDisplay; //for yearbook, not actual damage
    public int phases;
}

[System.Serializable]
public class barrageModule
{
    public int ticksBetweenBarr;
    public float secsBetweenAttacks;
    public int ammoInBarrage;
    public float shootForce;
    public AudioClip attack;
    public AudioClip startAttack;
    public AudioClip stopAttack;
    public int peelsWaste;
}
[System.Serializable]
public class compoundModule
{
    public float secondAttackHealthThresholdPercent;
    public string attackDisplay; //also for yearbook
    public AudioClip enrageSound;
}
[System.Serializable]
public class summonerModule
{
    public enemySO minion;
    public int summonAmount;
    public float secsBetweenSummon;
    public AudioClip summonSound;
}

[System.Serializable]
public class sabotageModule
{
    public float amountTaking;
}
