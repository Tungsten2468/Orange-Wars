using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class tableHealth : MonoBehaviour
{
    public float maxTableHealth;
    public float currentTableHealth;
    public GameSequence game;
    public Slider healthDisplay;
    public GameObject group0Trash;
    public GameObject group1Trash;
    public GameObject group2Trash;
    public GameObject group3Trash;
    public GameObject lossScreen;
    public GameObject lowHealthOverlay;
    void Start()
    {
        currentTableHealth = maxTableHealth;
        healthDisplay.maxValue = maxTableHealth;
        healthDisplay.value = currentTableHealth;
    }
    public void takeDamage(float damage)
    {
        currentTableHealth -= damage;
        healthDisplay.value = currentTableHealth;

        if(currentTableHealth <= maxTableHealth - (maxTableHealth * 0.25f))
        {
            group0Trash.SetActive(true);
        }
        if(currentTableHealth <= maxTableHealth - (maxTableHealth * 0.50f))
        {
            group1Trash.SetActive(true);
        }
        if(currentTableHealth <= maxTableHealth - (maxTableHealth * 0.75f))
        {
            group2Trash.SetActive(true);
        }
        if(currentTableHealth <= 0)
        {
            group3Trash.SetActive(true);
        }

        if(currentTableHealth <= 0)
        {
            game.gameLost(game.lossMessages[1]);
            Destroy(this);
        }

        if(currentTableHealth <= maxTableHealth - (maxTableHealth * 0.90))
        {
            if(!lowHealthOverlay.activeSelf)
            {
                lowHealthOverlay.SetActive(true);
                lowHealthOverlay.GetComponent<Animator>().Play("startLow");
            }
        }
        else
        {
            lowHealthOverlay.SetActive(false);
        }
    }

    public void healTable(float amount)
    {
        currentTableHealth += amount;
        healthDisplay.value = currentTableHealth;

        if(currentTableHealth <= maxTableHealth - (maxTableHealth * 0.25f))
        {
            group0Trash.SetActive(true);
        }
        if(currentTableHealth <= maxTableHealth - (maxTableHealth * 0.50f))
        {
            group1Trash.SetActive(true);
        }
        if(currentTableHealth <= maxTableHealth - (maxTableHealth * 0.75f))
        {
            group2Trash.SetActive(true);
        }
        if(currentTableHealth <= 0)
        {
            group3Trash.SetActive(true);
        }
    }

    
}
