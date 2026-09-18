using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Orange Wars/Level Events/Idle Moment")]
public class idleMoment : levelEvent
{
    public float tickStarted;
    public float ticksLasting;
    public GameSequence theGame;

    public override void execute(GameSequence game, laneManager laneMng, GameObject instructionsObj, int initialTick)
    {
        clearUI(game, instructionsObj, game.spawnGraphicsHere);
        game.disabledTroops = new List<troopSO>(dontAllowDeploy);

        tickStarted = initialTick;
        theGame = game;
        tickManager.onTick += checkMomentOver;
    }

    public void checkMomentOver()
    {
        if (tickManager.tickCount >= tickStarted + ticksLasting)
        {
            tickManager.onTick -= checkMomentOver;
            theGame.tickSinceLastEvent = tickManager.tickCount;
            theGame.progressEvents();
        }
    }
}
