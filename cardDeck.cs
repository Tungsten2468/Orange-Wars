using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class cardDeck : MonoBehaviour
{
    public GameSequence game;
    public List<GameObject> cardHolders;
    public GameObject cardAboutToChange; 
    public int currentRerollCost = 10; 
    public TMP_Text peelCostDisp;

    public void startDeck()
    {
        peelCostDisp.text = "Peel Cost: " + currentRerollCost;

        foreach (GameObject cHolder in cardHolders)
        {
            replaceCard(cHolder);
        }
    }

    public void replaceCard(GameObject cardHolder)
    {
        foreach (Transform child in cardHolder.transform)
        {
            Destroy(child.gameObject);
        }

        audioManager.instance.Play(audioManager.instance.flipCard);

        var cardVersion = randomCard().troopIcon.transform.Find("cardVersion").gameObject;
        var cardSpawned = Instantiate(cardVersion, cardHolder.transform);

        cardSpawned.transform.SetAsLastSibling();

        cardSpawned.SetActive(true);
    }

    public void removeCard(GameObject cardHolder)
    {
        if (cardHolder.transform.childCount == 0)
            return;

        foreach (Transform child in cardHolder.transform)
        {
            Destroy(child.gameObject);
        }

        replaceCard(cardHolder);
    }

    public troopSO randomCard()
    {
        troopSO card;

        do
        {
            int id = Random.Range(0, game.thisLevel.predeterminedTroops.Count);
            card = game.thisLevel.predeterminedTroops[id];
        }
        while (CardAlreadyInHand(card.troopID));

        return card;
    }

    bool CardAlreadyInHand(int ID)
    {
        foreach (GameObject holder in cardHolders)
        {
            if (holder.transform.childCount > 0)
            {
                troopSO tData = holder.transform.GetChild(0)
                    .GetComponent<draggableItemScript>().troopToSpawnSO;

                if (tData != null && tData.troopID == ID)
                    return true;
            }
        }
        return false;
    }

    public void reroll()
    {
        if (game.orangePeels >= currentRerollCost)
        {
            audioManager.instance.Play(audioManager.instance.rollDice);

            foreach (GameObject cHolder in cardHolders)
            {
                replaceCard(cHolder);
            }

            game.consumePeels(currentRerollCost);
            currentRerollCost += 5;
            peelCostDisp.text = "Peel Cost: " + currentRerollCost;
        }
    }
}
