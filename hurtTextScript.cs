using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class hurtTextScript : MonoBehaviour
{
    public float damageTaken;
    public TMP_Text myText;
    public int ticksBeforeDisappear;
    private int disappearOnThisTick;
    public bool heal;
    void Start()
    {
        if(!heal)
        {
            myText.text = "-" + damageTaken.ToString();
            myText.color = Color.red;
        }   
        else
        {
            myText.text = "+" + damageTaken.ToString();
            myText.color = Color.green;
        }
        disappearOnThisTick = tickManager.tickCount + ticksBeforeDisappear;
        tickManager.onTick += disappear;
    }
    public void disappear()
    {
        if(tickManager.tickCount >= disappearOnThisTick)
        {
            tickManager.onTick -= disappear;
            Destroy(gameObject);
        }
    }
}
