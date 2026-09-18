using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Orange Wars/Level Events/Instructions")]
public class instructions : levelEvent
{
    public string instruction;
    public GameObject graphic;
    public Vector2 graphicPosition;
    public override void execute(GameSequence game, laneManager laneMng, GameObject instructionsObj, int initialTick)
    {
        clearUI(game, instructionsObj, game.spawnGraphicsHere);
        instructionsObj.transform.parent.gameObject.SetActive(true);
        instructionsObj.GetComponent<TMP_Text>().text = instruction;
        game.advButtonObj.SetActive(false);

        //game.disabledTroops = runtimeDontAllowDeploy;

        if (graphic != null)
        {
            var graphicObj = Instantiate(graphic, game.spawnGraphicsHere.transform);
            graphicObj.GetComponent<RectTransform>().anchoredPosition = graphicPosition;
        }

        game.tickSinceLastEvent = tickManager.tickCount;
        game.progressEvents();
    }

}
