using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class tilePosition
{
    public Vector2 tilePos;
    public bool occupied;
}

[System.Serializable]
public class lane
{
    public int laneID;

    public List<enemySO> enemies = new List<enemySO>();
    public List<GameObject> enemyObjects = new List<GameObject>();

    public List<GameObject> troopObjects = new List<GameObject>();
    public List<troopSO> troops = new List<troopSO>();

    public List<GameObject> possibleTroopTiles = new List<GameObject>();

    // IMPORTANT: initialize this so Unity never shares prefab references
    public List<tileScript> availableEnemyPositions = new List<tileScript>();

    public float laneYPos;
}

public class laneManager : MonoBehaviour
{
    public List<lane> lanes;
    //public List<troopSO> allTroopsAlive = new List<troopSO>();
    public List<GameObject> allTroopsAlive = new List<GameObject>();
    //public List<enemySO> allEnemiesAlive = new List<enemySO>();
    public List<GameObject> allEnemiesAlive = new List<GameObject>();

    public troopSO lastTroopPlaced;
    public enemySO lastEnemyKilled;

    void Start()
    {
        // DO NOT wipe Inspector lists.
        // Only rebuild availableEnemyPositions safely.

        foreach (lane ln in lanes)
        {
            if (ln.availableEnemyPositions == null)
                ln.availableEnemyPositions = new List<tileScript>();
            else
                ln.availableEnemyPositions.Clear();
        }

        initializeTiles();
    }

    private void initializeTiles()
    {
        tileScript[] allTiles = FindObjectsOfType<tileScript>();

        foreach (tileScript t in allTiles)
        {
            int id = t.laneID;

            if (id < 0 || id >= lanes.Count)
            {
                Debug.LogError("Tile " + t.name + " has invalid laneID: " + id);
                continue;
            }

            //register enemy tiles
            if(!t.gameObject.CompareTag("enemyTile"))  
                continue; 

            lanes[id].availableEnemyPositions.Add(t);
        }
    }

    public void addEnemyToLane(enemySO enemy, GameObject enemyObj, int whatLane)
    {
        lanes[whatLane].enemies.Add(enemy);
        lanes[whatLane].enemyObjects.Add(enemyObj);
        allEnemiesAlive.Add(gameObject);
    }

    public void removeEnemyFromLane(enemySO enemy, GameObject enemyObj, int whatLane)
    {
        lanes[whatLane].enemies.Remove(enemy);
        lanes[whatLane].enemyObjects.Remove(enemyObj);
        lastEnemyKilled = enemy;
        allEnemiesAlive.Remove(gameObject);
    }

    public void addTroopToLane(troopSO troop, GameObject troopObj, int whatLane)
    {
        lanes[whatLane].troops.Add(troop);
        lanes[whatLane].troopObjects.Add(troopObj);
        lastTroopPlaced = troop;
        allTroopsAlive.Add(gameObject);
    }

    public void removeTroopFromLane(troopSO troop, GameObject troopObj, int whatLane)
    {
        lanes[whatLane].troops.Remove(troop);
        lanes[whatLane].troopObjects.Remove(troopObj);
        allTroopsAlive.Remove(gameObject);
    }

    //gets the available tile closest to the front of the entire lane
    //entityLane = "t" for troop; "e" for enemy
    public tileScript closestTileToLane(int lane, string entityLane)
    {
        var troopTiles = lanes[lane].possibleTroopTiles;
        var enemyTiles = lanes[lane].availableEnemyPositions;

        tileScript closestTile = null;

        // Troops (left side)
        if (entityLane == "t")
        {
            float greatestX = float.NegativeInfinity;

            foreach (GameObject tile in troopTiles)
            {
                float x = tile.transform.position.x;
                if (x > greatestX)
                {
                    greatestX = x;
                    closestTile = tile.GetComponent<tileScript>();
                }
            }
        }
        // Enemies (right side)
        else if (entityLane == "e")
        {
            float leastX = float.PositiveInfinity;

            foreach (tileScript tile in enemyTiles)
            {
                float x = tile.transform.position.x;
                if (x < leastX && tile.occupied == false)
                {
                    leastX = x;
                    closestTile = tile;
                }
            }
        }

        return closestTile;
    }
}
