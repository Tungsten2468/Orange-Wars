using System.Collections;
using UnityEngine;

public class jaidenScript : enemyBehaviors
{
    private enum state { Shooting, Hiding };
    private state myState;

    public int barrageTick;

    public override void initStats()
    {
        baseRangedForce = thisEnemy.isBarrage.shootForce;
        baseTicksBeforeShoot = thisEnemy.isBarrage.ticksBetweenBarr;
    }

    public override void Start()
    {
        base.Start();
        tickManager.onTick += UnifiedTick;

        myState = state.Hiding;
    }

    private void UnifiedTick()
    {
        if (!deploymentComplete)
            return;

        myTicks();
    }

    public void myTicks()
    {
        switch (myState)
        {
            case state.Hiding:
                waitForShoot();
                break;

            case state.Shooting:
                handleShooting();
                break;
        }
    }

    public void waitForShoot()
    {
        if (tickManager.tickCount >= barrageTick)
        {
            myState = state.Shooting;

            myAudio.PlayOneShot(thisEnemy.isBarrage.startAttack);
            animator.Play("hide", 0, 1f);
            animator.speed = -1f;
            hitbox.enabled = true;
        }
    }

    public void handleShooting()
    {
        if (shooting)
            return;

        if (tickManager.tickCount >= barrageTick)
        {
            shooting = true;
            StartCoroutine(shootBarrage(
                thisEnemy.isBarrage.ammoInBarrage,
                thisEnemy.isBarrage.secsBetweenAttacks
            ));
        }
    }

    IEnumerator shootBarrage(int times, float delay)
    {
        for (int t = 0; t < times; t++)
        {
            shoot(thisEnemy.isShooter.projectile);
            yield return new WaitForSeconds(delay);
        }

        barrageTick = tickManager.tickCount +
            modifiedTick(thisEnemy.isBarrage.ticksBetweenBarr, ticksBeforeShootModifier);

        animator.Play("hide");
        myAudio.PlayOneShot(thisEnemy.isBarrage.stopAttack);
        myState = state.Hiding;
        animator.speed = 1f;
        hitbox.enabled = false;
        shooting = false;
    }

    public void OnDestroy()
    {
        tickManager.onTick -= UnifiedTick;
    }
}
