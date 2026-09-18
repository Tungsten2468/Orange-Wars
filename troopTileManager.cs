using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class troopTile
{
    public Vector2 worldCoords;
    public Vector2 center;
    public bool occupied;

}
public class troopTileManager : MonoBehaviour
{
    public List<troopTile> troopTiles;
}
