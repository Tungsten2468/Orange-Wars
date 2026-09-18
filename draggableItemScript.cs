using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;

public class draggableItemScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    public Transform previousParent;
    public Vector2 previousPos;
    //public troopSO troopToSpawn;
    public int troopIDToSpawn;
    public troopSO troopToSpawnSO;
    public GameSequence game;
    public Tilemap gameGrid;
    public GameObject notEnough;
    public GameObject selectedBorder;
    [Header("UI stuff")]
    public TMP_Text costDisp;
    public cardDeck deck;
    private void OnValidate()
    {
        
    }
    private void Awake()
    {
        
    }

    public void Start()
    {
        game = GameObject.Find("gameManager").GetComponent<GameSequence>();
        if(game.thisLevel.usesCards)
            deck = GameObject.Find("carddeck").GetComponent<cardDeck>();
        if (game == null)
        {
            Debug.LogError("GameSequence not found!");
            return;
        }
        if (troopToSpawnSO != null)
        {
            troopIDToSpawn = troopToSpawnSO.troopID;
        }
        else
        {
            Debug.LogError("troopToSpawnSO was never assigned on " + gameObject.name);
        }

        if (costDisp != null)
            costDisp.text = troopToSpawnSO.troopCost.ToString();
        else
            Debug.LogWarning("costDisp not assigned on " + gameObject.name);

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        gameGrid = GameObject.Find("Grid").transform.GetChild(0).GetComponent<Tilemap>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (troopIDToSpawn < 0 || troopIDToSpawn >= game.everyTroop.troops.Count)
        {
            Debug.LogError("Invalid troopIDToSpawn: " + troopIDToSpawn);
            return;
        }
        game.troopRemovalMode = false;
        game.troopRemovalInst.SetActive(false);
        audioManager.instance.Play(audioManager.instance.dragStart);
        foreach (var t in game.disabledTroops)
        {
            Debug.Log("Disabled troop: " + t.name + " ID: " + t.GetInstanceID());
        }

        if(game.disabledTroops.Contains(game.everyTroop.troops[troopIDToSpawn]) || 
        game.thisLevel.eventsInLevel[game.currentEvent].dontAllowDeployAny)
        {
            return;
        }
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
        previousParent = transform.parent;
        previousPos = transform.position;
        transform.parent = canvas.transform;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(game.disabledTroops.Contains(game.everyTroop.troops[troopIDToSpawn]) || 
        game.thisLevel.eventsInLevel[game.currentEvent].dontAllowDeployAny)
        {
            return;
        }
        rectTransform.anchoredPosition += eventData.delta/canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(game.disabledTroops.Contains(game.everyTroop.troops[troopIDToSpawn]) || 
        game.thisLevel.eventsInLevel[game.currentEvent].dontAllowDeployAny)
        {
            return;
        }
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        transform.parent = previousParent;
        transform.position = previousPos;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        //detect if mouse is over a valid tile
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("troopTile"))
        {
            var tileHit = hit.collider.GetComponent<tileScript>();
            if(!troopToSpawnSO.allowedSurfaces.Contains(tileHit.thisSurface))
                return;
            if(tileHit.occupied == false && game.everyTroop.troops[troopIDToSpawn].troopCost <= game.oranges )
            {
                if(troopToSpawnSO.dontTakeTile == false)
                {
                    tileHit.placeTroop();
                }
                game.takeOrange(game.everyTroop.troops[troopIDToSpawn].troopCost);
                game.spawnTroop(game.everyTroop.troops[troopIDToSpawn], snapToTile(worldPos), hit.collider.gameObject);
                if(tileHit.customSortingLayer != "")
                {
                    var child = tileHit.GetComponentInChildren<entity>();
                    child.myGraphics.sortingLayerName = tileHit.customSortingLayer;
                }
                if(game.thisLevel.usesCards)
                {
                    deck.removeCard(previousParent.gameObject);
                    Destroy(gameObject);
                }
            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if(game.disabledTroops.Contains(troopToSpawnSO) ||
        game.thisLevel.eventsInLevel[game.currentEvent].dontAllowDeployAny)
        {
            return;
        }
        game.troopRemovalMode = false;
        game.troopRemovalInst.SetActive(false);
        if (game.currentTroopSelected == null)
        {
            audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.dragStart);
            game.currentTroopSelected = troopToSpawnSO;
            if(game.thisLevel.usesCards)
            {
                game.cards.cardAboutToChange = transform.parent.gameObject;
            }
            return;
        }
        if (game.currentTroopSelected.troopID != troopToSpawnSO.troopID)
        {
            game.currentTroopSelected = troopToSpawnSO;
            if(game.thisLevel.usesCards)
            {
                game.cards.cardAboutToChange = transform.parent.gameObject;
            }
            return;
        }

        game.currentTroopSelected = null;
        if(game.thisLevel.usesCards)
        {
            game.cards.cardAboutToChange = null;
        }
    }


    public Vector3 snapToTile(Vector3 worldPos)
    {
        Vector3Int tile = gameGrid.WorldToCell(worldPos);
        Vector3 center = gameGrid.GetCellCenterWorld(tile);
        return center;
    }

    public void Update()
    {
        bool hasEnoughOranges = game.oranges >= game.everyTroop.troops[troopIDToSpawn].troopCost;
        bool deployAllowed = !game.thisLevel.eventsInLevel[game.currentEvent].dontAllowDeployAny;
        var troopSO = game.everyTroop.troops[troopIDToSpawn];
        bool troopEnabled = !game.disabledTroops.Contains(troopSO);


        if(hasEnoughOranges && deployAllowed && troopEnabled)
        {
            notEnough.SetActive(false);
        }
        else
        {
            notEnough.SetActive(true);
        }


        
        if(game.currentTroopSelected != null && game.currentTroopSelected.troopID == troopToSpawnSO.troopID)
        {
            selectedBorder.SetActive(true);
        }
        else
        {
            selectedBorder.SetActive(false);
        }
        
        
    }
}
