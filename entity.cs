using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface laneComponent
{
    int interfaceLaneID {get; set;}
}

[System.Serializable]
public enum entityType
{
    Troop,
    Enemy,
}
public abstract class entity : MonoBehaviour, laneComponent
{
    public entityType thisEntity;
    public bool isShieldObj;
    [Header("Stats")]
    public float maxHealth;
    public float currentHealth;
    public int currentLaneID;
    public int interfaceLaneID
    {
        get => currentLaneID;
        set => currentLaneID = value;
    } 
    public bool invincible;
    public bool stunned;
    public bool keepSortingLayerName = false;
    public bool keepLayerOrderID = false;
    public bool strictKeepGraphicOrder = false;
    [Header("Utilities")]
    public tileScript myTile;
    [HideInInspector] public Animator animator;
    [HideInInspector] public BoxCollider2D hitbox;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public AudioSource myAudio;
    [HideInInspector] public SpriteRenderer myGraphics;
    [HideInInspector] public Slider myHealthBar;
    public List<statusEffect> currentEffects = new();
    [Header("Runtime")]
    public GameSequence game;
    public laneManager laneMngr;
    public GameObject hurtText;
    public GameObject healText;
    public GameObject orangeDrop;
    public System.Action deathLogic;
    public static event System.Action onTookDamage;

    public void Awake()
    {
        game = GameObject.Find("gameManager").GetComponent<GameSequence>();
        laneMngr = GameObject.Find("gameManager").GetComponent<laneManager>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        hitbox = gameObject.GetComponent<BoxCollider2D>();
        myAudio = gameObject.GetComponent<AudioSource>();
        myGraphics = GetComponentInChildren<SpriteRenderer>(true);
        myHealthBar = GetComponentInChildren<Slider>(true);
        if (myGraphics == null)
        {
            myGraphics = GetComponent<SpriteRenderer>();
        }
        if(myHealthBar != null)
        {
            myHealthBar.gameObject.SetActive(false);
        }
        animator = gameObject.GetComponentInChildren<Animator>(true);
    }

    public void takeDamage(float amount)
    {
        if(invincible)
            return;
        
        damageAction();

        currentHealth -= amount;

        if(!isShieldObj)
        {
            var hText = Instantiate(hurtText, new Vector2(transform.position.x, transform.position.y + 0.5f), Quaternion.identity);
            hText.GetComponent<hurtTextScript>().damageTaken = amount;
            hText.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 2f, ForceMode2D.Impulse);       
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            deathLogic?.Invoke();
        }

        if(myHealthBar != null)
        {
            if(myHealthBar.gameObject.activeSelf == false)
                myHealthBar.gameObject.SetActive(true);
            myHealthBar.value = currentHealth;
        }
    }

    public static void damageAction()
    {
        onTookDamage?.Invoke();
    }
}
