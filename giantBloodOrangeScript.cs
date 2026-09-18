using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class giantBloodOrangeScript : MonoBehaviour
{
    public Rigidbody2D rb;
    public Vector2 nextPos;
    public float damage;
    public float chompInterval;
    private float chompTimer;
    void Start()
    {
        nextPos = new Vector2(Random.Range(-10, 10), Random.Range(-8, 8));
        tickManager.onTick += bloodOrangeTick;
    }
    public void bloodOrangeTick()
    {
         // ROTATE ONLY ON Z AXIS
            Vector2 dir = nextPos - (Vector2)transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        if (Vector2.Distance(rb.position, nextPos) > 0.05f) //walk until reached destination
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, nextPos, 0.5f); 
            rb.MovePosition(newPos);
        }
        else
        {
            nextPos = new Vector2(Random.Range(-10, 10), Random.Range(-8, 8));
        }
    }
    public void OnDestroy()
    {
        tickManager.onTick -= bloodOrangeTick;
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        entity troop = other.GetComponent<entity>();
        if(troop != null && troop.thisEntity == entityType.Troop)
        {
            troop.takeDamage(damage);
        }
    }

    void Update()
    {
        chompTimer -= Time.deltaTime;

        if (chompTimer <= 0f)
        {
            audioManager.instance.sfxSource.pitch = Random.Range(0.9f, 1.3f);
            audioManager.instance.Play(audioManager.instance.giantBloodOrangeChomp);
            audioManager.instance.sfxSource.pitch = 1f;
            chompTimer = chompInterval;
        }
        
    }
}
