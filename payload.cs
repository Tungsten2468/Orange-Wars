using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[System.Serializable]
public class payLoadEntry
{
    public enemySO enemy;
    public int laneToSpawn;
}

[CreateAssetMenu(fileName = "New Payload", menuName = "Orange Wars/Level Events/Attack Wave/Payload")]
public class payload : ScriptableObject
{
    public List<payLoadEntry> enemies;
    public int ticksTillNextPayload;

}
