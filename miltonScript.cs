using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class miltonScript : troopBehaviors
{
    public bool readiedArtillery;
    public int lobTick;
    
    public override void initStats()
    {
        baseRangedForce = thisTroop.isLobber.lobForce;
        baseLobAngle = thisTroop.isLobber.angle;
        baseTicksBeforeShoot = thisTroop.isLobber.tickBeforeNextLob;
        peelUsage = thisTroop.isLobber.peelsWaste;
    }

    public override void Start()
    {
        base.Start();
        tickManager.onTick += miltonTick;
        lobTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
    }

    public void miltonTick()
    {
        if(tickManager.tickCount >= lobTick)
        {
            StartCoroutine(attack());
            lobTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
        }
    }

    public void OnDestroy()
    {
        tickManager.onTick -= miltonTick;
    }

    public IEnumerator attack()
    {
        animator.Play("prepare");
        myAudio.PlayOneShot(audioManager.instance.deodorantSpray);
        yield return new WaitForSeconds(thisTroop.isChemical.prepareTime);
        lob(thisTroop.isLobber.projectile, peelUsage);
        lobTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
    }
}
