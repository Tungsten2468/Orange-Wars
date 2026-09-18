using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum valueToWatch
{
    orangeCount,
    peelCount
}

[CreateAssetMenu(fileName = "New Level Event", menuName = "Orange Wars/Level Events")]
public abstract class levelEvent : ScriptableObject
{
    public int eventOrder;
    public trigger thisEventTrigger;
    public float startTick;
    public ticksPassedSettings ticksPassedTrigger;
    public troopPlacedSettings troopPlacedTrigger;
    public enemyKilledSettings enemyKilledTrigger;
    public valueChangedSettings valueChangedTrigger;
    public List<troopSO> dontAllowDeploy;
    public bool dontAllowDeployAny;

    public float timeScale;

    public abstract void execute(GameSequence game, laneManager laneMng, GameObject instructionText, int initialTick);

    public virtual void tickUpdate(GameSequence game, laneManager laneMngr)
    {
        // jackshit
    }

    public void clearUI(GameSequence game, GameObject instructionText, GameObject graphics)
    {

        instructionText.GetComponent<TMP_Text>().text = "";
        instructionText.transform.parent.gameObject.SetActive(false);
        game.advButtonObj.SetActive(false);

        for (int i = graphics.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(graphics.transform.GetChild(i).gameObject);
        }
    }

    public bool didValueChange(GameSequence game)
    {
        float currentValue = 0f;

        switch (valueChangedTrigger.triggerValue)
        {
            case valueToWatch.orangeCount:
                currentValue = game.oranges;
                break;

            case valueToWatch.peelCount:
                currentValue = game.orangePeels;
                break;
        }

        if (!valueChangedTrigger.hasInitialized)
        {
            valueChangedTrigger.valueChecking = currentValue;
            valueChangedTrigger.hasInitialized = true;
            return false;
        }

        switch (valueChangedTrigger.changeType)
        {
            case changeToObserve.general:
                if (currentValue != valueChangedTrigger.valueChecking)
                {
                    valueChangedTrigger.valueChecking = currentValue;
                    valueChangedTrigger.hasInitialized = false;
                    return true;
                }
                break;

            case changeToObserve.increase:
                if (currentValue > valueChangedTrigger.valueChecking)
                {
                    valueChangedTrigger.valueChecking = currentValue;
                    valueChangedTrigger.hasInitialized = false;
                    return true;
                }
                break;

            case changeToObserve.decrease:
                if (currentValue < valueChangedTrigger.valueChecking)
                {
                    valueChangedTrigger.valueChecking = currentValue;
                    valueChangedTrigger.hasInitialized = false;
                    return true;
                }
                break;
        }

        return false;
    }
}
