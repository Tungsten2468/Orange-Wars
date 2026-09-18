using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class levelCardScript : MonoBehaviour
{
    public allLevels getLevels;
    public levelData thisLevelData;
    //public int levelDataID;
    public titlescreenManager tlt;
    public Button thisPlayButton;
    public TMP_Text buttonTxt;
    public void Start()
    {
        thisLevelData = getLevels.everyLevel[thisLevelData.levelNumber].thisLevel; //redirect reference to the level in the SO
        thisPlayButton.onClick.AddListener(() => tlt.loadLevel(thisLevelData, this));
    }
}
