using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class enemyBehaviors : entity
{
    public enemySO thisEnemy;
    [Header("Usuable Base Stats")]
    public float baseRangedForce;
    public float baseMeleeDamage; //for footmen
    public int baseTickBeforeMelee;
    public int baseTicksBeforeShoot;
    public float baseMovementSpeed;
    public float baseLobAngle;
    [Header("Stat modifers")]
    public Dictionary<string, float> modifierMap;
    public float rangedForceModifier;
    public float meleeDamageModifer;
    public float ticksBeforeMeleeModifier;
    public float ticksBeforeShootModifier;
    public float movementSpeedModifier;
    public float lobAngleModifier;
    [Header("Tick ends")]
    [Header("Misc")]
    public Transform shootPoint;
    public int ammoLeft;
    //public Vector2 currentDirection;
    public GameObject boostedParticles;
    public statusEffectManager thisEffects;
    public GameObject orangePeelPrefab;
    public GameObject juniorTable;
    public bossManager bossSource;
    public troopDetector getTroopDetected;
    public entity myShieldObj;
    public GameObject enemyInRange;
    public int nextStep;
    public GameObject leader;
    public Vector2 targetLanePos;
    public int nextLane;
    public bool quitting;
    public GameObject mainProjectile;
    //public System.Action deploy;
    [Header("Flags")]
    public bool moving;
    public bool shooting;
    public bool deployed; //means deployment has started
    public bool deploymentComplete = false; //deployment is done, logic can now start
    public bool punching;
    public bool hasSummoned = false;
    public bool angry;

    public abstract void initStats();
    public virtual void Start()
    {
        modifierMap = new Dictionary<string, float>()
        {
            {"movementSpeed", movementSpeedModifier},
            {"rangedSpeed", ticksBeforeShootModifier},
            {"rangedForce", rangedForceModifier},
            {"meleeDamage",meleeDamageModifer},
            {"meleeSpeed", ticksBeforeMeleeModifier},
            {"lobAngle", lobAngleModifier}
        };
        initStats();

        if(myHealthBar != null)
        {
            myHealthBar.maxValue = thisEnemy.maxHealth;
            myHealthBar.value = myHealthBar.maxValue;
        }

        //graphics
        if(!keepSortingLayerName)
        {
            if(myGraphics != null && myTile != null)
            {
                myGraphics.sortingLayerName = myTile.customSortingLayer;
            }
        }
            
        if(!strictKeepGraphicOrder)
        {
            SpriteRenderer[] renderersInChildren = GetComponentsInChildren<SpriteRenderer>(true);
            foreach(SpriteRenderer sRenderer in renderersInChildren)
            {
                sRenderer.sortingLayerName = myGraphics.sortingLayerName;
            }
        }
        if(!keepLayerOrderID)
        {
            SpriteRenderer[] renderersInChildren = GetComponentsInChildren<SpriteRenderer>(true);
            foreach(SpriteRenderer sRenderer in renderersInChildren)
            {
                sRenderer.sortingOrder = myGraphics.sortingOrder + 1;
            }
            SpriteRenderer[] renderersInGraphic = myGraphics.GetComponentsInChildren<SpriteRenderer>(true);
            foreach(SpriteRenderer sRenderer in renderersInGraphic)
            {
                sRenderer.sortingLayerName = myGraphics.sortingLayerName;
                sRenderer.sortingOrder = myGraphics.sortingOrder + 1;
            }
        }

        if(!thisEnemy.iAmBoss)
            deathLogic = enemyDeath;
        maxHealth = thisEnemy.maxHealth;
        currentHealth = maxHealth;
        juniorTable = GameObject.Find("juniorTable");
        if(myShieldObj != null)
        {
            myShieldObj.maxHealth = thisEnemy.isTank.shieldHealth;
            myShieldObj.currentHealth = maxHealth;
        }
        if(shootPoint == null)
            shootPoint = transform;
        if(!thisEnemy.iAmBoss)
            deploy();
    }
    public void deploy()
    {
        if(!deployed)
        {
            tickManager.onTick += deploy;
            deployed = true;
        }
        if (Vector2.Distance(rb.position, myTile.transform.position) > 0.05f) //walk until reached destination
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, myTile.transform.position, 0.8f); 
            rb.MovePosition(newPos);
        }
        else
        {
            tickManager.onTick -= deploy;
            audioManager.instance.sfxSource.pitch = Random.Range(0.9f, 1.1f);
            audioManager.instance.Play(audioManager.instance.enemySpawns);
            audioManager.instance.sfxSource.pitch = 1f;
            deploymentComplete = true;
        }
    }

    public bool checkForTroops()
    {
        if(laneMngr.lanes[currentLaneID].troops.Count > 0)
        {
            return true;
        }
        return false;
    }

    public bool arrivedAtTroop()
    {
        if(getTroopDetected.troopDetected != null)
        {
            return true;
        }
        return false;
    }
    public float calculateTotalDamage(float baseDamage, float throwForce)
    {
        float modForce = 1 + (throwForce/4.5f);
        float modifier = 1f + rangedForceModifier;
        float finalDamage = baseDamage * modForce * modifier;
        finalDamage = Mathf.Round(finalDamage * 2f) / 2f;
        return finalDamage;
    }
    public void shoot(GameObject projectile)
    {
        if(animator != null)
            animator.Play("shoot");
        else
        {
            return;
        }
        myAudio.pitch = Random.Range(0.9f, 1.1f);
        myAudio.PlayOneShot(thisEnemy.isShooter.shootSound);
        myAudio.pitch = 1f;
        var proj = Instantiate(projectile, shootPoint.position, Quaternion.identity);
        var p = proj.GetComponent<projectileScript>();
        p.myLane = currentLaneID;
        p.enemyProjectile = true;

        float fShootForce = factorInModifier(baseRangedForce, rangedForceModifier);
        p.totalDamage = 
        calculateTotalDamage(projectile.GetComponent<projectileScript>().baseDamage, fShootForce);
        proj.GetComponent<Rigidbody2D>().velocity = Vector2.left * fShootForce;
    }
    //for choosing where to shoot(lane)
    public void choreographedShoot(float chosenY)
    {
        if(animator != null)
            animator.Play("shoot");
        else
        {
            return;
        }
        myAudio.pitch = Random.Range(0.9f, 1.1f);
        myAudio.PlayOneShot(thisEnemy.isShooter.shootSound);
        myAudio.pitch = 1f;
        var proj = Instantiate(thisEnemy.isShooter.projectile, new Vector2(shootPoint.position.x, chosenY), Quaternion.identity);
        var p = proj.GetComponent<projectileScript>();
        p.enemyProjectile = true;

        float fShootForce = factorInModifier(baseRangedForce, rangedForceModifier);
        p.totalDamage = 
        calculateTotalDamage(thisEnemy.isShooter.projectile.GetComponent<projectileScript>().baseDamage, fShootForce);
        proj.GetComponent<Rigidbody2D>().velocity = Vector2.left * thisEnemy.isShooter.shootForce;
    }
    public void lob()
    {
        if(animator != null)
            animator.Play("shoot");
        else
        {
            return;
        }
        myAudio.pitch = Random.Range(0.9f, 1.1f);
        myAudio.PlayOneShot(thisEnemy.isLobber.lobSound);
        myAudio.pitch = 1f;
        var proj = Instantiate(thisEnemy.isLobber.projectile, transform.position, Quaternion.identity);
        var p = proj.GetComponent<projectileScript>();
        var projRB = proj.GetComponent<Rigidbody2D>();
        p.enemyProjectile = true;
        p.myLane = currentLaneID;
        var target = juniorTable.transform;
        if(laneMngr.lanes[currentLaneID].troopObjects.Count > 0)
        {
            target = laneMngr.lanes[currentLaneID].troopObjects[0].transform;
        }
        if(thisEnemy.isLobber.targetLobs)
        {
            p.specificTarget = target.gameObject;
        }
        else
        {
            p.specificTarget = null;
        }
        float fLobForce = factorInModifier(baseRangedForce, rangedForceModifier);
        p.totalDamage = 
        calculateTotalDamage(thisEnemy.isLobber.projectile.GetComponent<projectileScript>().baseDamage, fLobForce);

        projRB.mass = 1;
        projRB.gravityScale = 1;
        projRB.AddForce(calculateShot(target) * 
        calculateVelocity(target), ForceMode2D.Impulse);
    }

    public Vector2 calculateShot(Transform target)
    {
        float angleRad = baseLobAngle * Mathf.Deg2Rad;

        float xDisplacement = target.position.x - rb.position.x;
        float dirSign = Mathf.Sign(xDisplacement);

        // DO NOT normalize — keep the real angle
        return new Vector2(dirSign * Mathf.Cos(angleRad),
                        Mathf.Sin(angleRad));
    }


    public float calculateVelocity(Transform target)
    {
        float g = Mathf.Abs(Physics2D.gravity.y);

        float dx = Mathf.Abs(target.position.x - rb.position.x);
        float angleRad = baseLobAngle * Mathf.Deg2Rad;

        float sin2 = Mathf.Sin(2f * angleRad);

        if (sin2 <= 0.01f)
            sin2 = 0.01f;

        return Mathf.Sqrt(dx * g / sin2);
    }

    public void walkContinuous()
    {
        if(rb == null)
            return;

        moving = true;
        Vector2 newPos = rb.position + Vector2.left * factorInModifier(baseMovementSpeed, movementSpeedModifier);
        rb.MovePosition(newPos);
    }
    public void walkToward(Vector2 where, float speed)
    {
        if(rb == null)
            return;

        moving = true;
        Vector2 newPos = Vector2.MoveTowards(rb.position, where, speed); 
        rb.MovePosition(newPos);
    }
    public void walkVertical(float targetY, float speed)
    {
        if(rb == null)
            return;
            
        
        moving = true;
        float newY = Mathf.MoveTowards(transform.position.y, targetY, speed);
        rb.MovePosition(new Vector2(rb.position.x, newY));
    }
    public bool reachedTarget(Vector2 target)
    {
        if(Vector2.Distance(rb.position, target) > 0.05f)
            return false;
        return true;
    }

    void OnApplicationQuit()
    {
        quitting = true;
    }
    public void punch(entity hurt)
    {
        audioManager.instance.Play(thisEnemy.isFootman.attackSound);

        // FIX: handle destroyed targets
        if (hurt == null || hurt.gameObject == null)
        {
            tableHealth table = getTroopDetected.troopDetected?.GetComponent<tableHealth>();
            laneMngr.allEnemiesAlive.Remove(gameObject);
            //removeEnemyFromLane(thisEnemy, gameObject, currentLaneID);

            if (table != null)
            {
                animator.Play("punch");
                table.takeDamage(thisEnemy.isFootman.damage);
            }

            animator.Play("wait");
            return;
        }

        // Normal punch
        animator.Play("punch");
        hurt.takeDamage(thisEnemy.isFootman.damage);

        animator.Play("wait");
    }


    //switchMode dictates how the enemy will switch- either by using the tile's Y ('t') or the lane's Y ('l')
    public void SwitchLanes(int newLane, string switchMode = "t")
    {
        nextLane = newLane;

        game.laneMngr.removeEnemyFromLane(thisEnemy, gameObject, currentLaneID);
        game.laneMngr.addEnemyToLane(thisEnemy, gameObject, newLane);
        currentLaneID = newLane;

        float fixedX = rb.position.x;
        
        if(switchMode == "t")
            targetLanePos = new Vector2(fixedX, laneMngr.lanes[newLane].possibleTroopTiles[0].transform.position.y);
        else if(switchMode == "l")
            targetLanePos = new Vector2(fixedX, laneMngr.lanes[newLane].laneYPos);

        //currentState = state.switching;
    }

    public IEnumerator summonMinions(int times, float timeBetween)
    {
        //one second before it actually begins
        yield return new WaitForSeconds(timeBetween);

        for(int t = 0; t < times; t++)
        {
            var minion = game.summonEnemy(thisEnemy.isSummoner.minion, new Vector2(23, -1.5f), myTile.gameObject);
            enemyBehaviors minionBehavior = minion.GetComponent<entity>() as enemyBehaviors;
            minionBehavior.leader = gameObject;
            if(bossSource != null)//if a boss spawned me, register my minion with my boss
            {
                minionBehavior.bossSource = bossSource;
                minionBehavior.bossSource.myMinionsAlive.Add(minion);
            }   
            if(t % 2 == 0)
            {
                minionBehavior.nextStep = 0;
            }
            else if(t % 2 == 1)
            {
                minionBehavior.nextStep = 1;
            }
            yield return new WaitForSeconds(timeBetween);
        }
    }

    public void enemyDeath()
    {
        OnDeath();
        myTile.occupied = false;
        laneMngr.removeEnemyFromLane(thisEnemy, this.gameObject, currentLaneID);

        var lane = laneMngr.lanes[currentLaneID];

        // Prevent duplicates and index errors
        if (!lane.availableEnemyPositions.Contains(myTile))
            lane.availableEnemyPositions.Add(myTile);

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

    public float factorInModifier(float baseVal, float modifier)
    {
        float newVal = baseVal + (1 * baseVal * modifier);
        return newVal;
    }

    public int modifiedTick(int baseTicks, float modifier)
    {
        int newTicks = (int)(baseTicks * (1f + modifier));
        newTicks = Mathf.Max(newTicks, 1);
        return newTicks;
    }

    public virtual void OnDestroy()
    {
        if(laneMngr.allEnemiesAlive.Contains(gameObject))
            laneMngr.allEnemiesAlive.Remove(gameObject);
        if(myTile != null)
            myTile.GetComponent<tileScript>().occupied = false;
    }

    public virtual void OnDeath()
    {
        
    }

}
