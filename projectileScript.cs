using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class projectileScript : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  BASIC INFO
    // ─────────────────────────────────────────────
    public string projectilName;
    public bool enemyProjectile;
    public float baseDamage;
    public float totalDamage;
    public statusEffect projectileEffect;
    public AudioClip sound;

    // ─────────────────────────────────────────────
    //  TARGETING
    // ─────────────────────────────────────────────
    public GameObject specificTarget;
    public GameObject avoid;
    public int myLane;
    public bool myLaneOnly;

    // ─────────────────────────────────────────────
    //  LIFETIME
    // ─────────────────────────────────────────────
    public int lifeTime = -1;
    public int lifeEndTick;

    // ─────────────────────────────────────────────
    //  VISUALS / FEEDBACK
    // ─────────────────────────────────────────────
    public GameObject myParticles;
    public GameObject missMessage;

    // ─────────────────────────────────────────────
    //  FRAGMENTING
    // ─────────────────────────────────────────────
    public GameObject fragments;
    public int fragmentAmounts;
    public float fragmentSpeed;

    // ─────────────────────────────────────────────
    //  PHYSICS / MOVEMENT
    // ─────────────────────────────────────────────
    public bool applyGravity;
    public bool isWeighted;
    public bool selfMoving;
    public float speed;

    // ─────────────────────────────────────────────
    //  PIERCING
    // ─────────────────────────────────────────────
    public int maxSurviveHits = 0;

    // ─────────────────────────────────────────────
    //  SPLASH DAMAGE
    // ─────────────────────────────────────────────
    public GameObject splashArea;
    public float splashRadius;
    public int splashLastingTicks;

    // ─────────────────────────────────────────────
    //  NEUTRALIZATION
    // ─────────────────────────────────────────────
    public List<string> neutralizableProjs;

    // ─────────────────────────────────────────────
    //  INITIALIZATION
    // ─────────────────────────────────────────────
    public void Awake()
    {
        totalDamage = baseDamage;
    }

    public void Start()
    {
        if (enemyProjectile)
            transform.localScale = new Vector2(transform.localScale.x * -1f, transform.localScale.y);

        if (lifeTime != -1)
        {
            lifeEndTick = tickManager.tickCount + lifeTime;
            tickManager.onTick += projectileLifeCountdown;
        }

        if (selfMoving)
            tickManager.onTick += projectileSelfMove;
    }

    // ─────────────────────────────────────────────
    //  COLLISION HANDLING
    // ─────────────────────────────────────────────
    public void OnTriggerEnter2D(Collider2D other)
    {
        var takeHealth = other.GetComponent<entity>();
        var takeTableHealth = other.GetComponent<tableHealth>();
        var inflictStatus = other.GetComponent<statusEffectManager>();
        var otherProj = other.GetComponent<projectileScript>();

        // ─────────────────────────────────────────────
        //  PROJECTILE NEUTRALIZATION
        // ─────────────────────────────────────────────
        if (otherProj != null)
        {
            bool isEnemyVsTroop = (!enemyProjectile && otherProj.enemyProjectile) ||
                                  (enemyProjectile && !otherProj.enemyProjectile);

            if (isEnemyVsTroop && neutralizableProjs.Contains(otherProj.projectilName))
                Destroy(otherProj.gameObject);
        }

        // ─────────────────────────────────────────────
        //  ENTITY HIT LOGIC
        // ─────────────────────────────────────────────
        if (takeHealth != null)
        {
            bool hitNotSpecificTarget =
                specificTarget != null &&
                other.gameObject != specificTarget &&
                takeHealth.thisEntity == entityType.Enemy;

            bool hitAvoidTarget =
                avoid != null &&
                other.gameObject == avoid &&
                takeHealth.thisEntity == entityType.Enemy;

            int targetLane = other.GetComponent<laneComponent>().interfaceLaneID;
            bool laneTargetHit = !myLaneOnly || myLane == targetLane;

            if (hitNotSpecificTarget || hitAvoidTarget || !laneTargetHit)
            {
                if ((hitNotSpecificTarget && !enemyProjectile) || !laneTargetHit)
                    Instantiate(missMessage, transform.position, Quaternion.identity);

                return;
            }

            // ─────────────────────────────────────────────
            //  DAMAGE APPLICATION
            // ─────────────────────────────────────────────
            bool validHit =
                (!enemyProjectile && takeHealth.thisEntity == entityType.Enemy) ||
                (enemyProjectile && takeHealth.thisEntity == entityType.Troop) ||
                (!enemyProjectile && takeHealth.isShieldObj);

            if (validHit)
            {
                if (myParticles != null && splashArea == null)
                    Instantiate(myParticles, transform.position, Quaternion.identity);

                audioManager.instance.sfxSource.pitch = Random.Range(0.9f, 1.1f);
                audioManager.instance.Play(sound);
                audioManager.instance.sfxSource.pitch = 1f;

                takeHealth.takeDamage(totalDamage);

                if (projectileEffect != null && inflictStatus != null)
                    inflictStatus.beginEffect(projectileEffect);

                if (fragments != null)
                    Fragment(other.gameObject);

                if (maxSurviveHits == 0)
                    Destroy(gameObject);
                else if (maxSurviveHits != -1)
                    maxSurviveHits--;

                // ─────────────────────────────────────────────
                //  SPLASH DAMAGE
                // ─────────────────────────────────────────────
                if (splashArea != null)
                {
                    GameObject splash = Instantiate(splashArea, transform.position, Quaternion.identity);
                    var dmg = splash.GetComponent<damageArea>();
                    dmg.radius = splashRadius;
                    dmg.damage = baseDamage;

                    if(projectileEffect != null)
                    {
                        dmg.effect = projectileEffect;
                    }
                    if(myParticles != null)
                    {
                        dmg.particles = myParticles;
                    }

                    if (enemyProjectile)
                        dmg.targetTroopsOnly = true;
                }
            }
        }

        // ─────────────────────────────────────────────
        //  TABLE HIT LOGIC
        // ─────────────────────────────────────────────
        if (takeTableHealth != null)
        {
            if (avoid != null && other.gameObject == avoid)
                return;

            if (selfMoving)
                tickManager.onTick -= projectileSelfMove;

            if (enemyProjectile)
            {
                if (myParticles != null && splashArea == null)
                    Instantiate(myParticles, transform.position, Quaternion.identity);

                if (audioManager.instance != null)
                {
                    audioManager.instance.sfxSource.pitch = Random.Range(0.9f, 1.1f);
                    audioManager.instance.Play(sound);
                    audioManager.instance.sfxSource.pitch = 1f;
                }

                takeTableHealth.takeDamage(totalDamage);

                if (fragments != null)
                    Fragment(other.gameObject);

                if (splashArea != null)
                {
                    GameObject splash = Instantiate(splashArea, transform.position, Quaternion.identity);
                    Debug.Log("Instantiated splash area called: "+splash);
                    var dmg = splash.GetComponent<damageArea>();
                    dmg.radius = splashRadius;
                    dmg.damage = baseDamage;

                    if (!enemyProjectile)
                        dmg.targetTroopsOnly = false;
                }

                Destroy(gameObject);
            }
        }

        // ─────────────────────────────────────────────
        //  DESTROY TAGS
        // ─────────────────────────────────────────────
        if (other.gameObject.CompareTag("destroyProj"))
            Destroy(gameObject);

        if (other.gameObject.CompareTag("destroyTroopProj") && !enemyProjectile)
            Destroy(gameObject);

        if (other.gameObject.CompareTag("destroyEnemyProj") && enemyProjectile)
            Destroy(gameObject);
    }

    // ─────────────────────────────────────────────
    //  LIFETIME HANDLING
    // ─────────────────────────────────────────────
    void OnDestroy()
    {
        if (!gameObject.scene.IsValid())
            return;
    }

    public void projectileLifeCountdown()
    {
        if (tickManager.tickCount >= lifeEndTick)
        {
            tickManager.onTick -= projectileLifeCountdown;
            Destroy(gameObject);
        }
    }

    // ─────────────────────────────────────────────
    //  FRAGMENTING
    // ─────────────────────────────────────────────
    public void Fragment(GameObject avoidThis)
    {
        for (int f = 0; f < fragmentAmounts; f++)
        {
            var frag = Instantiate(fragments, transform.position, Quaternion.identity);
            var fproj = frag.GetComponent<projectileScript>();
            var rbfrag = frag.GetComponent<Rigidbody2D>();

            fproj.avoid = avoidThis;

            if (enemyProjectile)
                fproj.enemyProjectile = true;

            if (applyGravity)
                rbfrag.gravityScale = 1;

            Vector2 randomDir = Random.insideUnitCircle.normalized;
            rbfrag.velocity = randomDir * fragmentSpeed;
        }
    }

    // ─────────────────────────────────────────────
    //  UPDATE (WEIGHTED ROTATION)
    // ─────────────────────────────────────────────
    void Update()
    {
        if (isWeighted)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();

            if (rb.velocity.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
    }

    // ─────────────────────────────────────────────
    //  SELF MOVEMENT
    // ─────────────────────────────────────────────
    public void projectileSelfMove()
    {
        var rb = GetComponent<Rigidbody2D>();

        if (enemyProjectile)
            rb.MovePosition(rb.position + Vector2.left * speed);
        else
            rb.MovePosition(rb.position + Vector2.right * speed);
    }
}
