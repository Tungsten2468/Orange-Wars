using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class manualSaving : MonoBehaviour
{
    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadFile(string filename, string content);
    #endif

    public void ExportSave(userData data)
    {
        string json = JsonUtility.ToJson(data);

        #if UNITY_WEBGL && !UNITY_EDITOR
        DownloadFile("OW_Save.json", json);
        #else
        // Editor fallback for testing
        System.IO.File.WriteAllText(Application.dataPath + "/OW_Save.json", json);
        #endif
    }
}
