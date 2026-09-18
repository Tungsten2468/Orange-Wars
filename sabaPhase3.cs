using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

/* Saba The Sphinx attacks all lanes randomly while a giant floating eye
looks at lanes and shoots a piercing high damage projectile at it (can be blocked by
phase-provided rune). After a certain amount of time Saba will kill all troops and 
spawn in one Juan E.(clickshot) in each lane and spawn a clone of himself in each lane,
with one being the real him. If a clone is shot, blood oranges spawn as well as a non-destructable
Giant Blood Orange, and Saba kicks
the table to damage it.(high damage) If the real saba is shot, he is stunned temporarly, allowing the user
to land as many shots as they can in the time window to progress the phase. */
public class sabaPhase3 : bossPhase
{
    public List<int> allowedLanes; //lanes where this saba can attack
    public List<int> allowedCloneLanes; //anes where saba can spawn his clones and the Juan E.s
    public List<GameObject> myDupes;
    public int changeLaneFreq; 
    public int shootCooldownTicks;
    private int shootTick;
    private int laneChangeTick;
    private enum BossState {Switching, Shooting, Cloned, Kicking}
    private BossState state;  
    public Vector2 eyePos;
    public troopSO juanE;
    public enemySO gamalGamal;
    public int ticksBeforeClone;
    public int cloneTick;
    public bool recovering;
    public List<enemySO> allowedMinions;
    public bool shotDupe;
    public Animator weatherAnim;
    
    [Header("Prefab instances")]
    public GameObject myEye;
    public GameObject myRune;
    public GameObject myGiantBloodOrange;

    [Header("Prefabs")]
    public GameObject giantBloodOrangePrefab;
    public GameObject dupes;
    public GameObject poofs;
    public GameObject shockwavePrefab;
    public GameObject runePrefab;
    public GameObject eyePrefab;
    public GameObject projectile;
    public GameObject mummy;

    public override void enterPhase()
    {
        weatherAnim = manager.weatherManager.globalLight.gameObject.GetComponent<Animator>();
        recovering = false;
        manager.hitbox.enabled = true;
        state = BossState.Switching;
        myEye = Instantiate(eyePrefab, eyePos, Quaternion.identity);
        myRune = Instantiate(runePrefab, transform.position, Quaternion.identity);
        cloneTick = tickManager.tickCount + ticksBeforeClone;
        manager.baseRangedForce = manager.thisEnemy.isLobber.lobForce;
        manager.baseLobAngle = manager.thisEnemy.isLobber.angle;
        spawnGarrison();
    }

    public override void onBossTick()
    {
        if(phaseDefeated)
            return;
        base.onBossTick(); //phase ending logic in parent class
        HandleLaneChangeTimer();
        HandleCloneTimer();
        switch(state)
        {
            case BossState.Switching:
                handleWalking();
                break;
            case BossState.Shooting:
                break;
        }
    }

    private void HandleLaneChangeTimer()
    {
        if(state == BossState.Cloned || state == BossState.Kicking)
            return;
        if (tickManager.tickCount >= laneChangeTick)
        {
            int randomLane = allowedLanes[Random.Range(0, allowedLanes.Count)];
            if(manager.currentLaneID != randomLane)
                manager.animator.Play("charge");
            manager.SwitchLanes(randomLane, "l");
            state = BossState.Switching;

            if(randomLane == 7 || randomLane == 2)
                laneChangeTick = tickManager.tickCount + changeLaneFreq/2;
            else
                laneChangeTick = tickManager.tickCount + changeLaneFreq;
        }
    }
    private void HandleCloneTimer()
    {
        if(state == BossState.Cloned || state == BossState.Kicking)
            return;
        if(tickManager.tickCount >= cloneTick)
        {
            state = BossState.Cloned;
            clearField();
        }
    }
    public void handleWalking()
    {
        if(state == BossState.Cloned || state == BossState.Kicking)
            return;
        if (Mathf.Abs(transform.position.y - manager.targetLanePos.y) > 0.05f)
        {
            manager.walkVertical(manager.targetLanePos.y, 40f);
        }
        else
        {
            manager.moving = false;
            manager.animator.Play("idle");
            state = BossState.Shooting;
            manager.lob();
            shootTick = tickManager.tickCount + manager.modifiedTick(shootCooldownTicks, manager.baseTicksBeforeShoot);
        }
    }
    public void spawnGarrison()
    {
        for(int i = 0; i < 2; i++)
        {
            foreach(int id in allowedCloneLanes)
            {  
                var chosenTile = manager.laneMngr.closestTileToLane(id, "e");
                var en = manager.game.spawnEnemy(manager.randomEnemy(allowedMinions), chosenTile.transform.position, chosenTile.gameObject, manager);
                manager.myMinionsAlive.Add(en);
            }
        } 
    }

    public void handleShooting()
    {
        if(state == BossState.Cloned || state == BossState.Kicking)
            return;
        if(tickManager.tickCount >= shootTick)
        {
            manager.shoot(projectile);
            shootTick = tickManager.tickCount + manager.modifiedTick(shootCooldownTicks, manager.baseTicksBeforeShoot);
        }
    }

    public void clearField()
    {
        weatherAnim.Play("lightOut");
        manager.destroyAllMinions();
        Destroy(myEye);
        myRune.SetActive(false);
        if(myGiantBloodOrange != null)
            Destroy(myGiantBloodOrange);
        var sw = Instantiate(shockwavePrefab, new Vector2(0, 8), Quaternion.identity);
        sw.GetComponent<projectileScript>().enemyProjectile = true;
        sw.GetComponent<projectileScript>().avoid = manager.juniorTable;
        StartCoroutine(spawnJuansAndClones());
    }
    public IEnumerator spawnJuansAndClones()
    {
        List<int> currentAvailableClonePos = new List<int>(allowedCloneLanes);

        //move the real saba to his place
        var myRealPlace = currentAvailableClonePos[Random.Range(0, currentAvailableClonePos.Count)];
        manager.SwitchLanes(myRealPlace, "l");
        transform.position = new Vector2(transform.position.x, manager.laneMngr.lanes[myRealPlace].laneYPos);

        //spawn clones and place them in random lanes left
        currentAvailableClonePos.Remove(myRealPlace);
        yield return new WaitForSeconds(1.5f);
        for(int laneAmnt = 0; laneAmnt < currentAvailableClonePos.Count; laneAmnt++)
        {
            var cloneY = manager.laneMngr.lanes[currentAvailableClonePos[laneAmnt]].laneYPos;
            var cl = Instantiate(dupes, new Vector2(transform.position.x, cloneY), Quaternion.identity);
            cl.GetComponent<sabaDupe1>().mainBoss = this;
            cl.GetComponent<sabaDupe1>().interfaceLaneID = currentAvailableClonePos[laneAmnt];
            myDupes.Add(cl);
        }
        //then spawn Juan E-s
        yield return new WaitForSeconds(0.8f);
        for(int laneAmnt = 0; laneAmnt < allowedCloneLanes.Count; laneAmnt++)
        {
            var tl = manager.laneMngr.lanes[allowedCloneLanes[laneAmnt]].possibleTroopTiles[6];
            manager.game.spawnTroop(juanE, tl.transform.position, tl);
            Instantiate(poofs, tl.transform.position, Quaternion.identity);
        }
        manager.game.addPeels(5);
    }
    public IEnumerator dupeChosen()
    {
        if(shotDupe)
            yield break;
        if(state == BossState.Kicking)
            yield break;
        //Destroy dupes
        shotDupe = true;
        destroyAllDupes();
        yield return new WaitForSeconds(0.2f);
        cloneTick = tickManager.tickCount + ticksBeforeClone;
        state = BossState.Kicking;
        
        StartCoroutine(kickStraight());
    }
    public IEnumerator kickStraight()
    {
        audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.sonicBoom);
        manager.animator.Play("kick");
        Vector2 returnPos = transform.position;
        Vector2 kickTarget = new Vector2(manager.juniorTable.transform.position.x, transform.position.y);
        while (Vector2.Distance(transform.position, kickTarget) > 0.05f)
        {
            manager.rb.MovePosition(Vector2.MoveTowards(
                transform.position,
                kickTarget,
                100f * Time.deltaTime
            ));

            yield return null; // THIS prevents freezing
        }
        manager.animator.Play("kickback");
        while (Vector2.Distance(transform.position, returnPos) > 0.05f)
        {
            manager.rb.MovePosition(Vector2.MoveTowards(
                transform.position,
                returnPos,
                20f * Time.deltaTime
            ));

            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        //spawn 4 blood oranges
        for(int i = 0; i < 4; i++)
        {
            manager.shoot(manager.thisEnemy.isShooter.projectile);
        }
        myGiantBloodOrange = Instantiate(giantBloodOrangePrefab, transform.position, Quaternion.identity);
        state = BossState.Shooting;
        shotDupe = false;
    }

    //hurt troops that are kicked only when kicking
    public void OnTriggerEnter2D(Collider2D other)
    {
        if(state == BossState.Kicking)
        {
            var tableKicked = other.GetComponent<tableHealth>();
            var kicked = other.GetComponent<entity>();
            if(kicked != null && kicked.thisEntity == entityType.Troop)
            {
                audioManager.instance.sfxSource.PlayOneShot(manager.thisEnemy.isFootman.attackSound);
                kicked.takeDamage(150);
            }
            if(tableKicked != null)
            {
                audioManager.instance.sfxSource.PlayOneShot(manager.thisEnemy.isFootman.attackSound);
                tableKicked.takeDamage(150);
            }
        } 
        
        if(state == BossState.Cloned)
        {
            var projHitMe = other.GetComponent<projectileScript>();
            if(projHitMe != null && !projHitMe.enemyProjectile && recovering == false)
            {
                destroyAllDupes();
                StartCoroutine(recover());
            }
        }
    }

    public IEnumerator recover()
    {
        recovering = true;
        manager.animator.Play("mad");
        yield return new WaitForSeconds(2.5f);
        cloneTick = tickManager.tickCount + ticksBeforeClone;
        myEye = Instantiate(eyePrefab, eyePos, Quaternion.identity);
        myRune.SetActive(true);
        state = BossState.Shooting;
        recovering = false;
        spawnGarrison();
    }

    public void destroyAllDupes()
    {
        if(myDupes.Count < 1)
            return;
        foreach(GameObject dupe in myDupes)
        {
            if(dupe == null)
                continue;
            var scr = dupe.GetComponent<sabaDupe1>();
            scr.StartCoroutine(scr.destroyMyself());
        }
        myDupes.Clear();
    }

    public override void CleanupPhase()
    {
        base.CleanupPhase();
        manager.destroyAllMinions();
        weatherAnim.Play("shine");
    }
}
