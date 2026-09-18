using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
public class saveSystem
{
    private const string save = "OW_Save_V1_UserData";

    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadFile(string filename, string content);
    #endif

    public static void Save(userData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(save, json);
        PlayerPrefs.Save();
    }

    public static userData Load()
    {
        if (!PlayerPrefs.HasKey(save))
            return new userData();

        string json = PlayerPrefs.GetString(save);
        return JsonUtility.FromJson<userData>(json);
    }

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteKey(save);
        PlayerPrefs.Save();
    }

    public static void resetSoundSettings(userData data)
    {
        data.sfxVol = 100;
        data.musicVol = 100;
        Save(data);
    }

    public static void downloadSave()
    {
        string json = JsonUtility.ToJson(Load());

        #if UNITY_WEBGL && !UNITY_EDITOR
        DownloadFile("OW_Save.json", json);
        #else
        System.IO.File.WriteAllText(Application.dataPath + "/OW_Save.json", json);
        #endif
    }

    public static void LoadSaveFromFile()
    {
        GameObject.Find("saveManager").GetComponent<importJsonSave>().StartImport();
    }

}
