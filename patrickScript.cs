using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class patrickScript : troopBehaviors
{
    public GameObject floatingPeel;
    public Transform peelNapkin;
    public GameObject bubble; //for the boost
    public int invincibilityOverTick;
    public override void initStats()
    {
        
    }
    public override void Start()
    {
        base.Start();
        peelNapkin = GameObject.Find("peelNapkin").transform;
        thisEffects.functionToCallback = activateInvincibility;
                                                                         
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        var projectile = other.gameObject.GetComponent<projectileScript>();
        if(projectile != null && projectile.projectilName == "Orange Peel" && projectile.enemyProjectile == true)
        {
            myAudio.pitch = Random.Range(0.5f, 1f);
            myAudio.PlayOneShot(audioManager.instance.patrickCatchPeel);
            myAudio.pitch = 1f;
            animator.Play("getPeel");
            var peel = Instantiate(floatingPeel, transform.position, Quaternion.identity);
            peel.GetComponent<peelCollectedScript>().peelsToAdd = 1;
            //StartCoroutine(flyPeel(peel.GetComponent<Rigidbody2D>(), peelNapkin.position));
        }
    }

    public void invincibleTick()
    {
        if(tickManager.tickCount >= invincibilityOverTick)
        {
            tickManager.onTick -= invincibleTick;
            invincible = false;
            bubble.SetActive(false);
        }
        
    }

    public void activateInvincibility()
    {
        invincibilityOverTick = tickManager.tickCount + (int)thisEffects.lastEffectApplied.duration;
        tickManager.onTick += invincibleTick;
        invincible = true;
        bubble.SetActive(true);
    }

    /*public IEnumerator flyPeel(Rigidbody2D peelRb, Vector2 peelTarget)
    {
        while (Vector2.Distance(peelRb.position, peelTarget) > 0.05f)
        {
            peelRb.MovePosition(Vector2.MoveTowards(
                peelRb.position,
                peelTarget,
                10f * Time.deltaTime
            ));

            yield return null; // THIS prevents freezing
        }

        Destroy(peelRb.gameObject);
        game.addPeels(1);
    }*/


}
