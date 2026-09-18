using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
/* Saba The Sphinx summons enemies behind him and conjures a barrier to protect them from troop fire.
He moves to a random lane, releases three blood oranges and then stands still, leaving him
vulnerable to attack but not his minions. If all blood oranges are destroyed, his barrier is
broken, leaving his minions vulnerable for a short while until he switches to a new random lane,
summons three more blood oranges and conjures a new barrier.*/
public class sabaPhase1 : bossPhase
{
    public int minEnemyDiff;
    public int maxEnemyDiff;
    public int minEnemyAmnt;
    public int maxEnemyAmnt;

    public List<enemySO> allowedEnemies;
    private List<lane> cachedValidLanes;
    private enum BossState{idle, shooting, moving, protecting}
    private BossState state = BossState.idle;
    public int currentLane;
    public List<int> possibleLaneIDs = new List<int>{0, 4, 5, 9};
    public GameObject barrierPrefab;
    private GameObject currentBarrier;
    public int shootAmount = 2;
    public List<GameObject> apparitionsManifest; //blood orange tracker
    public bool angryIdle; //flag to check if he's currently angry and not doing anything

    public override void enterPhase()
    {
        state = BossState.idle;
        manager.hitbox.enabled = true;

        manager.myMinionsAlive = new List<GameObject>();

        manager.animator.Play("charge");
        manager.myAudio.PlayOneShot(audioManager.instance.sabaCharge);
        manager.SwitchLanes(possibleLaneIDs[Random.Range(0, possibleLaneIDs.Count)], "l");
        state = BossState.moving;//change lanes INSTANTLY

        summonEnemies(); //summon a batch of enemies right away
    }
    public override void onBossTick()
    {
        if(phaseDefeated)
            return;
        base.onBossTick(); //phase ending logic in parent class

        switch (state)
        {
            case BossState.moving:
                HandleWalking();
                break;
            case BossState.protecting:
                checkApparitions();
                break;
            case BossState.shooting:
                handleShooting();
                break;
        }
    }

    public void checkApparitions()
    {
        if(state == BossState.shooting)
            return;
        List<GameObject> tempApparitions = new List<GameObject>(apparitionsManifest);
        foreach(GameObject apparition in tempApparitions)
        {
            if(apparition == null)
            apparitionsManifest.Remove(apparition);
        }

        if(apparitionsManifest.Count == 0)
        {
            if(angryIdle == false)
            {
                angryIdle = true;
                audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.barrierDestroyed);
                Destroy(currentBarrier);
                manager.animator.Play("mad");
                StartCoroutine(waitBeforeSwitching(3));
            }
        }
    }

    public IEnumerator waitBeforeSwitching(float tm)
    {
        yield return new WaitForSeconds(tm);
        manager.SwitchLanes(possibleLaneIDs[Random.Range(0, possibleLaneIDs.Count - 1)]);
        state = BossState.moving;
        manager.animator.Play("charge");
        angryIdle = false;
    }

    private void HandleWalking()
    {
        if(currentBarrier != null)
        {
            Destroy(currentBarrier);
        }
        if (Vector2.Distance(manager.rb.position, manager.targetLanePos) > 0.05f)
        {
            manager.walkVertical(manager.targetLanePos.y, manager.factorInModifier(manager.baseMovementSpeed, manager.movementSpeedModifier));
        }
        else
        {
            manager.moving = false;
            state = BossState.shooting;
        }
    }

    private void handleShooting()
    {
        if(manager.shooting)
            return;
        manager.shooting = true;
        for(int i = 0; i < shootAmount; i++)
        {
            callApparition();
        }
        manager.animator.Play("aurafarm");
        state = BossState.protecting;
        audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.barrierAppear);
        currentBarrier = Instantiate(barrierPrefab, new Vector2(transform.position.x + 1.2f, transform.position.y), Quaternion.identity);
        manager.shooting = false;
    }

    private void callApparition()
    {
        manager.animator.Play("shoot");

        if (audioManager.instance != null)
        {
            audioManager.instance.sfxSource.pitch = Random.Range(0.9f, 1.1f);
            audioManager.instance.Play(manager.thisEnemy.isShooter.shootSound);
            audioManager.instance.sfxSource.pitch = 1f;
        }

        var proj = Instantiate(manager.thisEnemy.isShooter.projectile, manager.shootPoint.position, Quaternion.identity);
        var p = proj.GetComponent<projectileScript>();
        p.enemyProjectile = true;

        float multiplier = 1f + manager.thisEnemy.isShooter.shootForce / 4.5f;
        multiplier = Mathf.Round(multiplier * 100f) / 100f;

        p.totalDamage = p.baseDamage * multiplier;

        proj.GetComponent<Rigidbody2D>().velocity = Vector2.left * manager.thisEnemy.isShooter.shootForce;

        apparitionsManifest.Add(proj);
    }

    public override void CleanupPhase()
    {
        if(currentBarrier != null)
            Destroy(currentBarrier);
        manager.destroyAllMinions();
    }

    public void summonEnemies()
    {

        int randomEnemyAmnt = Random.Range(minEnemyAmnt, maxEnemyAmnt);

        //endSummoningTick = tickManager.tickCount + summoningLasting;

        StartCoroutine(SpawnEnemiesOverTime(randomEnemyAmnt));
    }

    public enemySO randomEnemy()
    {
        return allowedEnemies[Random.Range(0, allowedEnemies.Count)];
    }

    private IEnumerator SpawnEnemiesOverTime(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnOneEnemy();
            yield return new WaitForSeconds(1.5f);
        }

        // Do NOT change state here.
        // Summoning ends via waitForSummoningOver().
    }

    private void SpawnOneEnemy()
    {
        CacheValidLanes(possibleLaneIDs);

        if (cachedValidLanes.Count == 0)
            return;

        var chosenLane = cachedValidLanes[Random.Range(0, cachedValidLanes.Count)];
        if (chosenLane == null || chosenLane.availableEnemyPositions.Count == 0)
            return;

        var randomTile = chosenLane.availableEnemyPositions[
            Random.Range(0, chosenLane.availableEnemyPositions.Count)
        ];

        if (randomTile == null || randomTile.gameObject == null)
            return;

        // CAPTURE THE SPAWNED ENEMY
        var spawned = manager.game.spawnEnemy(
            randomEnemy(),
            randomTile.tilePosition,
            randomTile.gameObject,
            manager
        );

        // ADD IT TO THE LIST SO CLEANUP CAN REMOVE IT
        manager.myMinionsAlive.Add(spawned);
    }


    private void CacheValidLanes(List<int> allowedLaneIDs)
    {
        cachedValidLanes = new List<lane>();

        foreach(int laneID in allowedLaneIDs)
        {
            lane ln = manager.game.laneMngr.lanes[laneID];
            if (ln.availableEnemyPositions.Count > 0)
                cachedValidLanes.Add(ln);
        }
        /*foreach (var lane in manager.game.laneMngr.lanes)
        {
            if (lane.availableEnemyPositions.Count > 0)
                cachedValidLanes.Add(lane);
        }*/
    }

}
