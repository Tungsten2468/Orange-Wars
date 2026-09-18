using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Troop", menuName = "Orange Wars/Troops")]
public class troopSO : ScriptableObject
{
    public string troopName;
    public int troopID;
    public troopType type;
    public string functionDesc; //what does he do?
    public string troopDesc; //fun 
    public float maxHealth;
    public float troopCost;
    public Sprite troopSprite;
    public bool dontTakeTile;
    public List<surfaceType> allowedSurfaces;
    public GameObject troopPrefab;
    public GameObject troopIcon;
    public producerModule isProducer;
    public logisticsModule isLogistics;
    public shooterModule isShooter;
    public lobberModule isLobber;
    public intelligenceModule isIntelligence;
    public healerModule isHealer;
    public barrageModule isBarrage;
    public magdumperModule isMagdumper;
    public dynamicModule isDynamic;
    public chemicalModule isChemical;
}
[System.Serializable]
public enum troopType
{
    producer,
    shooter,
    lobber,
    logistics,
    tank,
    intelligence,
    healer,
    barrage,
    magdumper,
    clickshot,
    dynamic,
    chemical
}

[System.Serializable]
public class producerModule
{
    public float productionAmount;
    public int productionTicks;
    public int autoTicks; //for troops like daniel who generate peels automatically
}
[System.Serializable]
public class logisticsModule
{
    public float retrieveAmount;
    public int retrieveTicks;
}
[System.Serializable]
public class shooterModule
{
    public GameObject projectile;
    public float shootForce;
    public int tickBeforeNextShot;
    public float peelsWaste;
    public AudioClip shootSound;
    public bool requirePeels = true;
    
}

[System.Serializable]
public class lobberModule
{
    public GameObject projectile;
    public float lobForce;
    public int tickBeforeNextLob;
    public float peelsWaste;
    public float angle;
    public AudioClip lobSound;
    public bool requirePeels = true;
    public bool targetLobs = true;
}

[System.Serializable]
public class intelligenceModule
{
    public statusEffect myBuff;
    public int ticksBetweenBuffs;
    public AudioClip sound;
}

[System.Serializable]
public class tankModule
{
    public float shieldHealth;
}

[System.Serializable]
public class healerModule
{  
    public int ticksLasting;
    public int ticksPerHeal;
    public float speed;
    public float healAmount;
}
[System.Serializable]
public class magdumperModule
{
    public GameObject projectile;
    public float shootForce;
    public int magCapacity;
    public int reloadTicks;
    public float secsBetweenShots;
    public AudioClip attackSound;
    public AudioClip reloadSound;
}

[System.Serializable]
public class dynamicModule
{
    public List<GameObject> projectiles;
    public List<AudioClip> sounds;
}

[System.Serializable]
public class chemicalModule
{
    public int chemicalTicksLasting;
    public float prepareTime;
    public float damageDisp;
}
