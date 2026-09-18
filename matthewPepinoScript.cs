using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class matthewPepinoScript : enemyBehaviors
{
    public Sprite quarterNote;
    public Sprite halfNote;
    public Sprite wholeNote;
    public List<AudioClip> noteSounds;
    public int playTick;
    public override void initStats()
    {
        baseRangedForce = thisEnemy.isShooter.shootForce;
        baseTicksBeforeShoot = thisEnemy.isShooter.tickBeforeNextShot;
    }
    public override void Start()
    {
        base.Start();
        tickManager.onTick += trumpetTick;
        playTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
    }

    public void trumpetTick()
    {
        if(tickManager.tickCount >= playTick)
        {
            playNote();
            playTick = tickManager.tickCount + modifiedTick(baseTicksBeforeShoot, ticksBeforeShootModifier);
        }
    }

    public void playNote()
    {
        GameObject note = Instantiate(thisEnemy.isShooter.projectile, shootPoint.position, Quaternion.identity);
        projectileScript p = note.GetComponent<projectileScript>();
        p.enemyProjectile = true;
        //0 = quarter note, 1 = half note, 2 = whole note
        int randomNote = Random.Range(0, 3);

        switch(randomNote)
        {
            case 0:
                note.GetComponentInChildren<SpriteRenderer>().sprite = quarterNote;
                p.totalDamage = 1;
                break;
            case 1:
                note.GetComponentInChildren<SpriteRenderer>().sprite = halfNote;
                p.totalDamage = 2;
                break;
            case 2:
                note.GetComponentInChildren<SpriteRenderer>().sprite = wholeNote;
                p.totalDamage = 4;
                break;
        }

        myAudio.PlayOneShot(noteSounds[Random.Range(0, noteSounds.Count)]);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        tickManager.onTick -= trumpetTick;
    }
}
