using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class chapterCardScript : MonoBehaviour
{

    public chapterList getChapters;
    public chapter thisChapter;
    //public int levelDataID;
    public titlescreenManager tlt;
    public Button thisPlayButton;
    public TMP_Text buttonTxt;
    public void Start()
    {
        thisChapter = getChapters.allChapters[thisChapter.chapterID].thisChapter; //redirect reference to the level in the SO
        thisPlayButton.onClick.AddListener(() => tlt.displayAvailableLevels(thisChapter));
    }
}
