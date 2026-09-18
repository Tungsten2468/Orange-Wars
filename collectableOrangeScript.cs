using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class collectableOrangeScript : MonoBehaviour
{
    public GameSequence game;
    public Transform orangeNapkin;
    public BoxCollider2D thisCollider;
    public Rigidbody2D rb;
    public bool onConveyor;
    public int value = 1;
    public TMP_Text valueDisplay;
    void Start()
    {
        game = GameObject.Find("gameManager").GetComponent<GameSequence>();
        //keep clickable ON TOP of everything
        transform.position = new Vector3(transform.position.x, transform.position.y, -9f);
        orangeNapkin = GameObject.Find("orangeNapkin").transform;
        if(!onConveyor)
            rb.AddForce(Vector2.up * 2, ForceMode2D.Impulse);
        else
        {
            Destroy(transform.GetChild(0).gameObject);
            tickManager.onTick += moveDownConveyor;
        }
        if(value > 1)
        {
            valueDisplay.gameObject.SetActive(true);
            valueDisplay.text = value.ToString();
        }
        else
        {
            valueDisplay.gameObject.SetActive(false);
        }
    }

    public void OnMouseDown()
    {
        if(onConveyor)
        {
            return;
        }
        thisCollider.enabled = false;
        StartCoroutine(flyOrange());
    }

    public void moveDownConveyor()
    {
        rb.MovePosition(rb.position + Vector2.down * 0.2f); 
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("collectOrange"))
        {
            tickManager.onTick -= moveDownConveyor;
            thisCollider.enabled = false;
            StartCoroutine(flyOrange());
        }
    }

    public IEnumerator flyOrange()
    {
        while (Vector2.Distance(rb.position, orangeNapkin.position) > 0.05f)
        {
            rb.MovePosition(Vector2.MoveTowards(
                rb.position,
                orangeNapkin.position,
                40f * Time.deltaTime
            ));

            yield return null; 
        }

        Destroy(gameObject);
        game.addOrange(value);
    }

    void OnDestroy()
    {
        tickManager.onTick -= moveDownConveyor;
    }

}
