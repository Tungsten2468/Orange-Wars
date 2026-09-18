using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class chooseTroops : MonoBehaviour
{
    public userData GetUserData;
    public allTroops everyTroop;
    public Transform spawnIconsHere;
    public GameSequence game;
    public GameObject chooseTroopsScreen;
    public GameObject enemyDisplayCard;
    public Transform previewEnemiesHere;
    public TMP_Text troopAmntChosen;
    public Transform spawnTroopHolders;
    public GameObject troopHolderPrefab;

    void Start()
    {
        GetUserData = saveSystem.Load();
        game.thisLevel = game.getLevels.everyLevel[GameSequence.currentLevelID].thisLevel;

        if (game.thisLevel == null)
        {
            Debug.Log("No level loaded");
            return;
        }

        // Always initialize troopSlots
        game.troopSlots = new List<GameObject>();

        // Instantiate troop holders based on maxTroopsAllowed
        int maxSlots = Mathf.Max(0, game.thisLevel.maxTroopsAllowed);
        for (int holder = 0; holder < maxSlots; holder++)
        {
            GameObject h = Instantiate(troopHolderPrefab, spawnTroopHolders);
            game.troopSlots.Add(h);
        }

        // Ensure troopsUsed exists
        if (game.troopsUsed == null)
            game.troopsUsed = new List<troopSO>();

        // Reset waves
        foreach (levelEvent lEvent in game.thisLevel.eventsInLevel)
        {
            if (lEvent is attackWave atk)
                atk.resetWave();
        }

        if (game.thisLevel.allowTroopSelection)
        {
            chooseTroopsScreen.SetActive(true);

            // Start from a clean selection list
            game.troopsUsed.Clear();

            troopAmntChosen.text =
                "Troops selected: " + game.troopsUsed.Count + "/" + game.thisLevel.maxTroopsAllowed;

            previewEnemies();

            // Build ordered troop list
            List<troopSO> troopList = new List<troopSO>(everyTroop.troops);
            var preTroops = game.thisLevel.predeterminedTroops;
            List<troopSO> prioritized = new List<troopSO>();

            foreach (troopSO pt in preTroops)
            {
                var match = troopList.FirstOrDefault(t => t.troopID == pt.troopID);
                if (match != null)
                    prioritized.Add(match);
            }

            foreach (troopSO t in troopList)
            {
                if (!prioritized.Contains(t))
                    prioritized.Add(t);
            }

            troopList = prioritized;

            showAvailableTroops(troopList);
        }
        else
        {
            chooseTroopsScreen.SetActive(false);
            game.go();
        }
    }

    public void previewEnemies()
    {
        if (game.thisLevel.enemiesInvolved.Count > 0)
        {
            foreach (enemySO enemyToPreview in game.thisLevel.enemiesInvolved)
            {
                GameObject enemyCard = Instantiate(enemyDisplayCard, previewEnemiesHere);
                enemyCard.GetComponent<Image>().sprite = enemyToPreview.enemySprite;
                enemyCard.transform.GetChild(0).gameObject
                    .GetComponent<TMP_Text>().text = enemyToPreview.enemyName;
            }
        }
    }

    public void showAvailableTroops(List<troopSO> troopsToShow)
    {
        foreach (troopSO troop in troopsToShow)
        {
            var icon = Instantiate(troop.troopIcon, spawnIconsHere);

            var drag = icon.GetComponent<draggableItemScript>();
            if (drag != null)
                drag.enabled = false;

            troopSO troopRef = troop;
            StartCoroutine(SetupTroopIconNextFrame(icon, troopRef));
        }
    }

    public void troopSelected(GameObject icon, troopSO troop)
    {
        if (game.troopsUsed.Contains(troop))
        {
            game.troopsUsed.Remove(troop);
            icon.transform.GetChild(2).gameObject.SetActive(false);
            icon.transform.GetChild(2).GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            audioManager.instance.Play(audioManager.instance.troopSelected);

            if (game.troopsUsed.Count >= game.thisLevel.maxTroopsAllowed)
                return;

            game.troopsUsed.Add(troop);
            icon.transform.GetChild(2).gameObject.SetActive(true);
            icon.transform.GetChild(2).GetChild(0).gameObject.SetActive(true);
        }

        troopAmntChosen.text =
            "Troops selected: " + game.troopsUsed.Count + "/" + game.thisLevel.maxTroopsAllowed;
    }

    public void doBattle()
    {
        audioManager.instance.Play(audioManager.instance.click);
        chooseTroopsScreen.SetActive(false);
        game.go();
    }

    IEnumerator SetupTroopIconNextFrame(GameObject icon, troopSO troopRef)
    {
        yield return null;

        if (game.thisLevel.predeterminedTroops.Contains(troopRef))
        {
            if (!game.troopsUsed.Contains(troopRef))
                game.troopsUsed.Add(troopRef);

            icon.transform.GetChild(2).gameObject.SetActive(true);
            icon.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
        }
        else if (GetUserData.troopsUnlockedIDs.Contains(troopRef.troopID) &&
                 !game.thisLevel.troopsNotAllowedIDs.Contains(troopRef.troopID))
        {
            icon.transform.GetChild(2).gameObject.SetActive(false);

            var button = icon.AddComponent<Button>();
            GameObject capturedIcon = icon;
            troopSO capturedTroop = troopRef;

            button.onClick.AddListener(() => troopSelected(capturedIcon, capturedTroop));
        }
        else if (GetUserData.troopsUnlockedIDs.Contains(troopRef.troopID) &&
                 game.thisLevel.troopsNotAllowedIDs.Contains(troopRef.troopID))
        {
            icon.transform.GetChild(2).gameObject.SetActive(true);
            icon.transform.GetChild(2).GetChild(2).gameObject.SetActive(true);
        }
        else
        {
            icon.transform.GetChild(2).gameObject.SetActive(true);
            icon.transform.GetChild(2).GetChild(1).gameObject.SetActive(true);
        }

        troopAmntChosen.text =
            "Troops selected: " + game.troopsUsed.Count + "/" + game.thisLevel.maxTroopsAllowed;
    }
}
