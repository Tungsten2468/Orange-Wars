using System.Collections.Generic;
using UnityEngine;
/* Saba The Sphinx makes a clone of himself to handle the top lanes while his real self handles
the bottom lanes. His clone self will switch lanes in the upper portion and shoot orange peels.
His real self will summon spirit-imbued footmen that when dead return their spirit to him, in which
case he reincarnates them as mummies. His real self is invulnerable in this phase and the only way to
hurt him is through hurting his clone.*/
public class SabaPhase2 : bossPhase
{
    public GameObject clonePrefab;
    public GameObject myClone;
    public List<enemySO> allowedEnemies;
    public List<int> allowedLaneIDs;
    public int minEnemies;
    public int maxEnemies;
    public GameObject mummyGraphic;
    public GameObject spiritPrefab;
    private enum BossState { entering, normal }
    private BossState currentState;
    public override void enterPhase()
    {
        manager.hitbox.enabled = false;
        clone();
        currentState = BossState.entering;
        GoToMiddleLane();
        manager.onMinionDeath = summonMummy;
    }
    public void clone()
    {
        myClone = Instantiate(clonePrefab, transform.position, Quaternion.identity);
        myClone.GetComponent<sabaClone1>().mainBoss = manager;
    }

    public override void onBossTick()
    {
        if(phaseDefeated)
            return;
        base.onBossTick(); //phase ending logic in parent class

        manager.currentHealth = myClone.GetComponent<sabaClone1>().currentHealth; //sync health w/ clone
        switch(currentState)
        {
            case BossState.entering:
                if(Mathf.Abs(transform.position.y - manager.targetLanePos.y) > 0.05f)
                    manager.walkVertical(manager.targetLanePos.y, manager.factorInModifier(manager.baseMovementSpeed, manager.movementSpeedModifier));
                else
                    manager.moving = false;
                    manager.animator.Play("idle");
                    currentState = BossState.normal;
                break;
            case BossState.normal:
                checkMyEnemies();
                break;
        }  
    }

    public void checkMyEnemies()
    {
        if(phaseDefeated)
            return;
        if(manager.myMinionsAlive.Count > 0)
            return;
        manager.animator.Play("summon");
        summonFootmen();
    }

    private void GoToMiddleLane()
    {
        manager.animator.Play("walk");

        manager.game.laneMngr.removeEnemyFromLane(manager.thisEnemy, gameObject, manager.currentLaneID);
        manager.game.laneMngr.addEnemyToLane(manager.thisEnemy, gameObject, 2);

        manager.currentLaneID = 2;

        manager.targetLanePos = new Vector2(manager.rb.position.x, manager.game.laneMngr.lanes[2].laneYPos);
    }

    public void summonFootmen()
    {
        if(phaseDefeated)
            return;
        manager.CacheValidLanes(allowedLaneIDs);
        var randomEAmnt = Random.Range(minEnemies, maxEnemies);
        for(var i = 0; i < randomEAmnt; i++)
        {
            var summoned = manager.SpawnOneEnemy(manager.randomEnemy(allowedEnemies));
            var spiritBearer = summoned.gameObject.AddComponent<spiritBearing>();
            spiritBearer.myMaster = gameObject;
            spiritBearer.mySpirit = spiritPrefab;
            //manager.myMinionsAlive.Add(summoned);
        }
    }

    public void summonMummy(enemySO enemy)
    {
        if(phaseDefeated)
            return;
        audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.mummyGroan);
        var summoned = manager.SpawnOneEnemy(enemy);
        var lastEnemy = summoned.GetComponent<entity>();
        var oldGraphic = summoned.GetComponent<entity>().myGraphics;
        var newGraphic = Instantiate(mummyGraphic, lastEnemy.transform);
        lastEnemy.myGraphics = newGraphic.GetComponent<SpriteRenderer>();
        lastEnemy.animator = newGraphic.GetComponent<Animator>();
        Destroy(oldGraphic);
        //manager.myMinionsAlive.Add(summoned);
    }

    public override void CleanupPhase()
    {
        manager.destroyAllMinions();
        Destroy(myClone);
        manager.hitbox.enabled = true;
    }
}
