using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class draggableOrangeScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    public Transform previousParent;
    public Vector2 previousPos;
    public GameSequence game;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        game = GameObject.Find("gameManager").GetComponent<GameSequence>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
        previousParent = transform.parent;
        previousPos = transform.position;
        transform.parent = canvas.transform;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = rectTransform.position.z; // keep original depth
        rectTransform.position = worldPos;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        transform.parent = previousParent;
        transform.position = previousPos;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        //detect if mouse is over Jayko
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Jayko"))
        {
            if(hit.collider.gameObject.GetComponent<jaykoScript>().eating)
            {
                return;
            }
            game.takeOrange(1);
            hit.collider.GetComponent<jaykoScript>().eatOrange();
        }
    }
}
