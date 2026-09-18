using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class conveyorItem : MonoBehaviour
{
    public converyBeltScript conveyorBelt;
    public float moveSpeed = 0.3f;
    public RectTransform rt;
    public void Start()
    {
        rt = gameObject.GetComponent<RectTransform>();
    }
    
    void Update()
    {
        if(rt.anchoredPosition.y < conveyorBelt.currentmaxYLevel)
        {
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + moveSpeed);
        }
    }
}
