using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class musicManager : MonoBehaviour
{
    public static musicManager instance;

    public AudioSource musicSource;
    private userData getMusicPreference;
    public Slider volumeDisp;

    [Header("Music")]
    public AudioClip titlescreenTheme;
    public bool playTitleMusic = true; //for when switching from title scene to game scene

    void Awake()
    {
        Debug.Log($"{name} Awake in scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

        getMusicPreference = saveSystem.Load();
        musicSource.volume = getMusicPreference.musicVol;

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "titleScreen")
        {
            playTitleMusic = true;
            StartCoroutine(AssignSliderDelayed());
        }
    }

    IEnumerator AssignSliderDelayed()
    {
        Slider found = null;

        // Wait until the UI is fully spawned
        while (found == null)
        {
            yield return null;

            GameObject obj = GameObject.Find("musicVolumeSlider");
            if (obj != null)
            {
                found = obj.GetComponent<Slider>();
            }
        }

        volumeDisp = found;

        // Reconnect the slider event
        volumeDisp.onValueChanged.RemoveAllListeners();
        volumeDisp.onValueChanged.AddListener((float v) => changeVol());

        updateVolDisplay();
    }

    public void PlaySongLooped(AudioClip track)
    {
        if (musicSource.clip != null)
        {
            musicSource.Stop();
        }

        musicSource.clip = track;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySongOnce(AudioClip track)
    {
        if (musicSource.clip != null)
        {
            musicSource.Stop();
        }

        musicSource.clip = track;
        musicSource.loop = false;
        musicSource.Play();
    }


    public void playRandom(List<AudioClip> playlist)
    {
        if(!playTitleMusic)
        {
            musicSource.Stop();
            musicSource.clip = null;
            return;
        }
            
        var randomSong = playlist[Random.Range(0, playlist.Count)];
        while(musicSource.clip == randomSong)
        {
            randomSong = playlist[Random.Range(0, playlist.Count)];
        }
        PlaySongOnce(randomSong);
    }

    public void changeVol()
    {
        musicSource.volume = volumeDisp.value;
        getMusicPreference.musicVol = musicSource.volume;
        saveSystem.Save(getMusicPreference);

        volumeDisp.transform.GetChild(3).GetComponent<TMP_Text>().text =
            "Music Volume: " + Mathf.RoundToInt(musicSource.volume * 100);
    }

    public void updateVolDisplay()
    {
        if (volumeDisp == null)
        {
            return;
        }

        volumeDisp.value = musicSource.volume;
        volumeDisp.transform.GetChild(3).GetComponent<TMP_Text>().text =
            "Music Volume: " + Mathf.RoundToInt(musicSource.volume * 100);
    }
}
