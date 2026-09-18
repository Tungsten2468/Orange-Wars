using System.Runtime.InteropServices;
using UnityEngine;

public class importJsonSave : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName);
#endif

    public void StartImport()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UploadFile("saveManager", "OnFileLoaded");
#else
        // Editor fallback
        string path = Application.dataPath + "/OW_Save.json";

        if (!System.IO.File.Exists(path))
        {
            Debug.LogError("OW_Save.json not found in Assets folder.");
            return;
        }

        string json = System.IO.File.ReadAllText(path);
        OnFileLoaded(json);
#endif
    }

    public void OnFileLoaded(string json)
    {
        Debug.Log("RAW JSON FROM IMPORT: " + json);

        // Fix BOM + hidden characters
        json = json.Trim('\uFEFF', '\u200B');

        // Parse JSON
        userData data = JsonUtility.FromJson<userData>(json);

        // Save for future sessions
        saveSystem.Save(data);

        // Apply NOW (WebGL-safe)
        titlescreenManager.instance.getData = data;

        // Refresh UI / progression
        titlescreenManager.instance.ApplySaveData(saveSystem.Load());

        Debug.Log("Save file imported successfully!");
    }
}
