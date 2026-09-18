using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chapter", menuName = "Orange Wars/Chapters")]
public class chapter : ScriptableObject
{
    public string chapterName;
    public string location;
    public string difficulty;
    public int chapterID;
    public Sprite chapterSprite;
    public List<levelEntry> levelsInChapter;
}
