using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class userData
{
    public List<int> troopsUnlockedIDs = new List<int>();
    public List<int> enemiesDiscoveredIDs = new List<int>();
    public List<int> levelsUnlockedIDs = new List<int>();
    public List<int> chaptersUnlockedIDs = new List<int>();
    [Range(0f, 1f)]
    public float musicVol = 1;
    [Range(0f, 1f)]
    public float sfxVol = 1;
}

