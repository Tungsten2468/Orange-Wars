using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[System.Serializable]
public enum surfaceType{seat, table}

public class tileScript : MonoBehaviour
{
    public GameSequence game;
    public bool occupied;
    public Vector2 tilePosition;
    public BoxCollider2D thisCollider;
    public int laneID;
    public int tileID; //different in each lane
    public int mySortingOrderID; //what sorting ID to set the troop to when placed on THIS tile
    public string customSortingLayer;
    public surfaceType thisSurface;
    public void Start()
    {
        game = GameObject.Find("gameManager").GetComponent<GameSequence>();
        thisCollider = GetComponent<BoxCollider2D>();
        tilePosition = transform.position;
    }
    public void placeTroop()
    {
        audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.dragEnd);
        occupied = true;
        thisCollider.enabled = false;
    }
    public void removeTroop()
    {
        occupied = false;
        thisCollider.enabled = true;
    }

    public void OnMouseDown()
    {
        if(game.currentTroopSelected == null)
            return;
        
        //if this is not registered in possible troop tiles no troop may be placed here
        if(!game.laneMngr.lanes[laneID].possibleTroopTiles.Contains(gameObject))
            return;

        var troopSO = game.everyTroop.troops[game.currentTroopSelected.troopID];

        if(game.disabledTroops.Contains(troopSO) ||
        game.thisLevel.eventsInLevel[game.currentEvent].dontAllowDeployAny)
        {
            return;
        }

        if(!game.currentTroopSelected.allowedSurfaces.Contains(thisSurface))
        {
            return;
        }

        if(troopSO.troopCost <= game.oranges && !occupied)
        {
            if(!troopSO.dontTakeTile)
                placeTroop();

            game.takeOrange(troopSO.troopCost);
            game.spawnTroop(troopSO, transform.position, gameObject);

            if(customSortingLayer != "")
            {
                var child = GetComponentInChildren<entity>();
                if(child != null)
                    child.myGraphics.sortingLayerName = customSortingLayer;
            }

            if(game.thisLevel.usesCards)
                game.cards.removeCard(game.cards.cardAboutToChange);

            game.currentTroopSelected = null;
        }
    }

}
