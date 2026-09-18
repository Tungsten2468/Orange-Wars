using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class healthScript : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;
    public laneManager laneMngr;
    public GameSequence game;
    public int laneID;
    public troopSO thisTroop; //only applies to troops
    public enemySO thisEnemy; //only applies to enemies
    public tileScript thisTroopTile; //only applies to troops
    public GameObject hurtText;
    public GameObject protectorObj; //only if they are protected by a certain object
    public bool isShieldObj;
    public GameObject orangeDrop;
    public statusEffectManager thisEffects;
    public bossManager bossSource; //only for enemies that spawned from a boss
    public bossManager isBoss;
    public bool invincible;
    public void Start()
    {
        if(thisTroop != null)
        {
            maxHealth = thisTroop.maxHealth;
            if(isShieldObj)
            {
                maxHealth = thisEnemy.isTank.shieldHealth;
            }
        }
        else if (thisEnemy != null)
        {
            maxHealth = thisEnemy.maxHealth;
            if(isShieldObj)
            {
                maxHealth = thisEnemy.isTank.shieldHealth;
            }
        }
        currentHealth = maxHealth;
        
        game = GameObject.Find("gameManager").GetComponent<GameSequence>();
        laneMngr = GameObject.Find("gameManager").GetComponent<laneManager>();
        /*if(thisEffects != null && thismyAudioSourceEffects. != null)
        {
            thisEffects.myAudioSource.volume = audioManager.instance.sfxSource.volume;
        }

        if(bossSource != null)
        {
            bossSource.fixEnemyDiscrepancy();
            bossSource.myMinionsAlive.Add(gameObject);
        }*/
    }

    public void takeDamage(float amount)
    {
        if(invincible)
        {
            return;
        }
        currentHealth -= amount;

        // If this is a shield object, don't spawn hurt text or die here
        if (isShieldObj)
        {
            if (currentHealth <= 0)
                dieShieldObj();

            return;
        }

        // Spawn hurt text
        var hText = Instantiate(hurtText, new Vector2(transform.position.x, transform.position.y + 0.5f), Quaternion.identity);
        hText.GetComponent<hurtTextScript>().damageTaken = amount;
        hText.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
        if(isBoss != null)
        {
            isBoss.updateBossBar();
        }

        // Break protector object at half HP
        if(currentHealth < (maxHealth / 2) && protectorObj != null)
        {
            protectorObj.SetActive(false);
        }

        // NOW check for death
        if (currentHealth <= 0)
        {
            if (thisEnemy != null && thisEnemy.type != enemyType.boss)
            {
                dieEnemy();
            }
            else if (thisTroop != null)
            {
                dieTroop();
            }
        }
    }

    public void dieEnemy()
    {
        thisTroopTile.occupied = false;
        laneMngr.removeEnemyFromLane(thisEnemy, this.gameObject, laneID);

        var lane = laneMngr.lanes[laneID];

        // Prevent duplicates and index errors
        if (!lane.availableEnemyPositions.Contains(thisTroopTile))
            lane.availableEnemyPositions.Add(thisTroopTile);

        float orangeSpawnChance = Random.Range(0f, 1f);
        if(orangeSpawnChance <= 0.5f && game.thisLevel.enemiesDropOranges)
        {
            audioManager.instance.sfxSource.pitch = UnityEngine.Random.Range(0.5f, 1f);
            audioManager.instance.Play(audioManager.instance.orangeSpawned);
            audioManager.instance.sfxSource.pitch = 1f;
            Instantiate(orangeDrop, new Vector3(transform.position.x, transform.position.y, -4), Quaternion.identity);
        }
        if(bossSource != null)
        {
            bossSource.removeDeadMinion(gameObject);
        }
        Destroy(gameObject);
    }

    public void dieTroop()
    {
        laneMngr.removeTroopFromLane(thisTroop, gameObject, laneID);
        thisTroopTile.removeTroop();
        Destroy(gameObject);
    }
    public void dieShieldObj()
    {
        Destroy(gameObject);
    }

    void OnMouseDown()
    {
        if(thisTroop != null && game.troopRemovalMode)
        {
            audioManager.instance.Play(audioManager.instance.removeTroop);
            audioManager.instance.sfxSource.pitch = Random.Range(0.5f, 1f);
            audioManager.instance.Play(audioManager.instance.orangeSpawned);
            audioManager.instance.sfxSource.pitch = 1f;
            Instantiate(orangeDrop, new Vector3(transform.position.x, transform.position.y, -4), Quaternion.identity);
            laneMngr.removeTroopFromLane(thisTroop, gameObject, laneID);
            thisTroopTile.removeTroop();
            game.troopRemovalMode = false;
            game.troopRemovalInst.SetActive(false);
            Destroy(gameObject);
        }
    }

    public void OnDestroy()
    {
        // SAFETY: only touch lanes if this is a real enemy and we still have a tile
        if (thisEnemy != null && thisTroopTile != null)
        {
            thisTroopTile.occupied = false;

            // Do NOT call removeEnemyFromLane here.
            // Enemy removal is already handled in dieEnemy().
            var lane = laneMngr.lanes[laneID];

            if (!lane.availableEnemyPositions.Contains(thisTroopTile))
                lane.availableEnemyPositions.Add(thisTroopTile);
        }
    }

}
