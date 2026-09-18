using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class troopBehaviors : entity
{
    public troopSO thisTroop;
    [Header("Usuable Base Stats")]
    public float baseRangedForce;
    public float baseMeleeDamage; //for footmen
    public int baseTicksBetweenProduction;
    public int baseTicksBeforeShoot;
    public float baseProductionAmount;
    public float baseMovementSpeed;
    public float baseLobAngle;
    public int baseLogisticsTicks;
    public int baseAutoProduceTicks;
    public float peelUsage;
    [Header("Stat modifers")]
    public Dictionary<string, float> modifierMap;
    public float rangedForceModifier;
    public float meleeDamageModifer;
    public float ticksBetweenProductionModifier;
    public float ticksBeforeShootModifier;
    public float logisticsTicksModifier;
    public float movementSpeedModifier;
    public float autoProduceTicksModifier;
    public float lobAngleModifier;
    [Header("Tick ends")]
    public int finishEatingTick;
    [Header("Misc")]
    public Transform shootPoint;
    public int ammoLeft;
    //public Vector2 currentDirection;
    public GameObject boostedParticles;
    public statusEffectManager thisEffects;
    public GameObject orangePeelPrefab;
    public tableHealth juniorTable;
    public float currentTargetX;
    public bool facingRight = true;
    public System.Action clickLogic;
    [Header("Flags")]
    public bool moving;
    public bool shooting;
    public bool eating;
    public bool healing;

    public abstract void initStats();
    public virtual void Start()
    {
        modifierMap = new Dictionary<string, float>()
        {
            {"movementSpeed", movementSpeedModifier},
            {"rangedSpeed", ticksBeforeShootModifier},
            {"rangedForce", rangedForceModifier},
            {"meleeDamage",meleeDamageModifer },
            {"productionSpeed", ticksBetweenProductionModifier},
            {"logisticsSpeed", logisticsTicksModifier},
            {"autoProductionSpeed", autoProduceTicksModifier},
            {"lobAngle", lobAngleModifier}
        };
        initStats();
        juniorTable = GameObject.Find("juniorTable").GetComponent<tableHealth>();

        if(myHealthBar != null)
        {
            myHealthBar.maxValue = thisTroop.maxHealth;
            myHealthBar.value = myHealthBar.maxValue;
        }

        //graphics
        myGraphics.sortingLayerName = myTile.customSortingLayer;
        SpriteRenderer[] renderersInChildren = GetComponentsInChildren<SpriteRenderer>(true);
        foreach(SpriteRenderer sRenderer in renderersInChildren)
        {
            sRenderer.sortingLayerName = myGraphics.sortingLayerName;
        }
        if(!keepLayerOrderID)
        {
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
        
        maxHealth = thisTroop.maxHealth;
        currentHealth = maxHealth;
        deathLogic = troopDeath;
    }
    public bool checkForEnemies()
    {
        if(laneMngr.lanes[currentLaneID].enemies.Count > 0)
        {
            return true;
        }
        return false;
    }

    public void randomDynamicAttack(Vector2 shtpoint)
    {
        if(stunned)
            return;
        GameObject randomProjToSpawn = thisTroop.isDynamic.projectiles[Random.Range(0, thisTroop.isDynamic.projectiles.Count)];
        GameObject proj = Instantiate(randomProjToSpawn, shtpoint, Quaternion.identity);
        projectileScript p = proj.GetComponent<projectileScript>();
        p.enemyProjectile = false;

        myAudio.PlayOneShot(thisTroop.isDynamic.sounds[Random.Range(0, thisTroop.isDynamic.sounds.Count)]);
    }
    public void eatOrange()
    {
        if(stunned)
            return;
        if(eating == true)
        {
            return;
        }
        eating = true;
        finishEatingTick = tickManager.tickCount + modifiedTick(baseTicksBetweenProduction, ticksBetweenProductionModifier);
        animator.SetBool("eating", eating);
        tickManager.onTick += finishEating;
    }
    public void getPeels()
    {
        var peel = Instantiate(orangePeelPrefab, transform.position, Quaternion.identity);
        peel.GetComponent<peelCollectedScript>().peelsToAdd = thisTroop.isProducer.productionAmount;
    }
    public void finishEating()
    {
        if(tickManager.tickCount >= finishEatingTick)
        {
            eating = false;
            animator.SetBool("eating", eating);
            tickManager.onTick -= finishEating;
            getPeels();
        }
    }
    public bool haveEnoughPeels()
    {
        if(game.orangePeels >= peelUsage)
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
    public void shoot(GameObject projectile, float waste, string lockLane = "n")
    {
        if(stunned)
            return;
        if(animator != null)
            animator.Play("shoot");
        myAudio.pitch = Random.Range(0.1f, 1.1f);
        myAudio.PlayOneShot(thisTroop.isShooter.shootSound);
        myAudio.pitch = 1f;
        game.consumePeels(waste);
        var proj = Instantiate(projectile, transform.position, Quaternion.identity);
        var p = proj.GetComponent<projectileScript>();
        p.myLane = currentLaneID;
        if(lockLane == "y")
        {
            p.myLaneOnly = true;
        }
        
        float fShootForce = factorInModifier(baseRangedForce, rangedForceModifier);
        p.totalDamage = 
        calculateTotalDamage(projectile.GetComponent<projectileScript>().baseDamage, fShootForce);

        if(facingRight)
        {
            proj.GetComponent<Rigidbody2D>().velocity = Vector2.right * baseRangedForce;
        }
        if(!facingRight)
        {
            proj.GetComponent<Rigidbody2D>().velocity = Vector2.left * baseRangedForce;
        }
    }
    //mode = "n" (normal projectile); mode = "w" (weighted projectile)
    public void lob(GameObject projectile, float waste, string mode = "n")
    {
        if(stunned)
            return;
        if (animator != null)
            animator.Play("shoot");

        myAudio.pitch = Random.Range(0.1f, 1.1f);
        myAudio.PlayOneShot(thisTroop.isLobber.lobSound);
        myAudio.pitch = 1f;

        game.consumePeels(waste);

        var proj = Instantiate(projectile, transform.position, Quaternion.identity);
        var projRB = proj.GetComponent<Rigidbody2D>();
        var p = proj.GetComponent<projectileScript>();
        p.myLane = currentLaneID;

        if(mode == "w")
            p.isWeighted = true;

        // SAFETY CHECK — prevents ArgumentOutOfRangeException
        var enemies = game.laneMngr.lanes[currentLaneID].enemyObjects;

        if (enemies == null || enemies.Count == 0)
        {
            Destroy(proj);
            return;
        }

        // Find the enemy closest to the left (smallest X position)
        GameObject leftmostEnemy = null;
        float smallestX = float.MaxValue;

        foreach (var e in enemies)
        {
            if (e == null) continue;

            float ex = e.transform.position.x;
            if (ex < smallestX)
            {
                smallestX = ex;
                leftmostEnemy = e;
            }
        }

        if (leftmostEnemy == null)
        {
            Destroy(proj);
            return;
        }

        // Safe target access
        var target = leftmostEnemy.transform;

        //don't shoot if the enemy is past you already
        if(target.position.x < transform.position.x)
        {
            //return the peel amnt
            game.addPeels(waste);
            Destroy(proj);
            return;
        }
            

        p.specificTarget = target.gameObject;

        // Damage + physics
        float fLobForce = factorInModifier(baseRangedForce, rangedForceModifier);

        p.totalDamage = calculateTotalDamage(
            projectile.GetComponent<projectileScript>().baseDamage,
            fLobForce
        );

        projRB.mass = 1.2f;
        projRB.gravityScale = 1;

        projRB.AddForce(
            calculateShot() *
            calculateVelocity(target.position),
            ForceMode2D.Impulse
        );

    }

    public void lobBackward(GameObject projectile, float waste, string mode = "n")
    {
        if(stunned)
            return;
        if (animator != null)
            animator.Play("shoot");

        myAudio.pitch = Random.Range(0.1f, 1.1f);
        myAudio.PlayOneShot(thisTroop.isLobber.lobSound);
        myAudio.pitch = 1f;

        game.consumePeels(waste);

        var proj = Instantiate(projectile, transform.position, Quaternion.identity);
        var projRB = proj.GetComponent<Rigidbody2D>();
        var p = proj.GetComponent<projectileScript>();
        p.myLane = currentLaneID;

        if (mode == "w")
            p.isWeighted = true;

        // SAFETY CHECK
        var enemies = game.laneMngr.lanes[currentLaneID].enemyObjects;
        if (enemies == null || enemies.Count == 0)
        {
            Destroy(proj);
            return;
        }

        // Find the enemy closest to the RIGHT (largest X)
        GameObject rightmostEnemy = null;
        float largestX = float.MinValue;

        foreach (var e in enemies)
        {
            if (e == null) continue;

            float ex = e.transform.position.x;
            if (ex > largestX)
            {
                largestX = ex;
                rightmostEnemy = e;
            }
        }

        if (rightmostEnemy == null)
        {
            Destroy(proj);
            return;
        }

        var target = rightmostEnemy.transform;

        // Don't shoot backward if the enemy is still in front
        if (target.position.x > transform.position.x)
        {
            game.addPeels(waste);
            Destroy(proj);
            return;
        }

        p.specificTarget = target.gameObject;

        // Damage
        float fLobForce = factorInModifier(baseRangedForce, rangedForceModifier);

        p.totalDamage = calculateTotalDamage(
            projectile.GetComponent<projectileScript>().baseDamage,
            fLobForce
        );

        projRB.mass = 1.2f;
        projRB.gravityScale = 1;

        // TRUE backward shot direction
        float angleRad = baseLobAngle * Mathf.Deg2Rad;

        // backward = negative X
        Vector2 backwardDir = new Vector2(-Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;

        // backward velocity calculation
        float dx = Mathf.Abs(target.position.x - transform.position.x);
        float dy = target.position.y - transform.position.y;
        float g = Mathf.Abs(Physics2D.gravity.y);

        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        float numerator = g * dx * dx;
        float denominator = 2f * cos * cos * (dx * Mathf.Tan(angleRad) + dy);

        float vel = (denominator <= 0f) ? 0f : Mathf.Sqrt(numerator / denominator);

        projRB.AddForce(backwardDir * vel, ForceMode2D.Impulse);
    }




    public void modifiedLob(GameObject projectile)
    {
        if(stunned)
            return;
        myAudio.PlayOneShot(thisTroop.isMagdumper.attackSound);
        ammoLeft -= 1;

        var proj = Instantiate(projectile, shootPoint.position, Quaternion.identity);
        var projRB = proj.GetComponent<Rigidbody2D>();

        var p = proj.GetComponent<projectileScript>();
        p.isWeighted = true;


        if (!checkForEnemies())
        {
            Destroy(proj);
            return;
        }

        var target = new Vector2(currentTargetX, transform.position.y);
        if (target == null)
        {
            Destroy(proj);
            return;
        }

        float fLobForce = factorInModifier(baseRangedForce, rangedForceModifier);
        p.totalDamage = 
        calculateTotalDamage(projectile.GetComponent<projectileScript>().baseDamage, fLobForce);
        projRB.mass = 1.215f; //holy fine tuning
        projRB.gravityScale = 1;
        projRB.AddForce(calculateShot() * 
        calculateVelocity(target), ForceMode2D.Impulse);
    }
     
    public void step(Vector2 currentDirection)
    {
        if(stunned)
            return;
        float finalSpeedPerTick = baseMovementSpeed * (1 + movementSpeedModifier);
        Vector2 newPos = Vector2.MoveTowards(rb.position, currentDirection, finalSpeedPerTick); 
        rb.MovePosition(newPos);
    }
    public Vector2 calculateShot()
    {
        var angleInRads = baseLobAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRads), Mathf.Sin(angleInRads));
        return direction;
    }

    public float calculateVelocity(Vector2 target)
    {
        float g = Mathf.Abs(Physics2D.gravity.y);

        float x = target.x - transform.position.x;
        if (x < 0.1f) x = 0.1f;

        // This recreates the old behavior EXACTLY:
        // Use degrees, not radians.
        float damping = Mathf.Sin(2f * baseLobAngle);

        // Prevent zero or negative damping
        if (damping < 0.1f)
            damping = 0.1f;

        float velocity = Mathf.Sqrt((x * g) / damping);

        return velocity;
    }



    public void troopDeath()
    {
        laneMngr.removeTroopFromLane(thisTroop, gameObject, currentLaneID);
        myTile.removeTroop();
        Destroy(gameObject);
    }

    void OnMouseDown()
    {
        // 1. Removal mode takes priority
        if(thisTroop != null && game.troopRemovalMode)
        {
            audioManager.instance.Play(audioManager.instance.removeTroop);
            audioManager.instance.sfxSource.pitch = Random.Range(0.5f, 1f);
            audioManager.instance.Play(audioManager.instance.orangeSpawned);
            audioManager.instance.sfxSource.pitch = 1f;

            var org = Instantiate(orangeDrop, new Vector3(transform.position.x, transform.position.y, -4), Quaternion.identity);
            var orgVal = Mathf.RoundToInt(thisTroop.troopCost) / 3;
            org.GetComponent<collectableOrangeScript>().value = orgVal;

            laneMngr.removeTroopFromLane(thisTroop, gameObject, currentLaneID);
            myTile.removeTroop();

            game.troopRemovalMode = false;
            game.troopRemovalInst.SetActive(false);

            Destroy(gameObject);
            return; // STOP HERE
        }

        // 2. If not removing → flip troop
        if(thisTroop.type == troopType.shooter)
        {
            facingRight = !facingRight;

            transform.localScale = new Vector2(
                transform.localScale.x * -1,
                transform.localScale.y
            );
        }

        clickLogic?.Invoke();
    }


    public float factorInModifier(float baseVal, float modifier)
    {
        float newVal = baseVal + (1 * modifier);
        return newVal;
    }

    public int modifiedTick(int baseTicks, float modifier)
    {
        int newTicks = (int)(baseTicks * (1f + modifier));
        newTicks = Mathf.Max(newTicks, 1);
        return newTicks;
    }



}
