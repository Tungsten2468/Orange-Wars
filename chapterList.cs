using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[System.Serializable]
public class chapterEntry
{
    public chapter thisChapter;
    public bool unlocked;
}

[CreateAssetMenu(fileName = "New Chapter List", menuName = "Orange Wars/Chapter Lists")]
public class chapterList : ScriptableObject
{
    public List<chapterEntry> allChapters;
}
