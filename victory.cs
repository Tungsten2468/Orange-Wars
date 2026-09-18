using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Victory", menuName = "Orange Wars/Level Events/Victories")]
public class victory : levelEvent
{
    //public int troopUnlockedID;
    public override void execute(GameSequence game, laneManager laneMng, GameObject instructionsObj, int initialTick)
    {
        game.disabledTroops = new List<troopSO>(dontAllowDeploy);

        game.gameWon();
    }
}
