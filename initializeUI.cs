using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class initializeUI : MonoBehaviour, IPointerClickHandler
{
    public void Awake()
    {
        
    }
    public enemySO enemy;
    public titlescreenManager titleScreen;
    public void Start()
    {
        titleScreen = GameObject.Find("titleManager").GetComponent<titlescreenManager>();
    }
    public void OnPointerClick(PointerEventData eventData)
    { 
        FindObjectOfType<titlescreenManager>().showEnemyProfile(enemy);
    }
}
