using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Orange Wars/Level Events/Countdown")]
public class countdown : levelEvent
{
    public GameObject countdownGraphic;
    public Animator animator;
    public float ticksLasting = 30; //3 seconds basically
    public float tickToEnd;
    private GameSequence gameSeq;

    public override void execute(GameSequence game, laneManager laneMngr, GameObject instructionsObj, int initialTick)
    {
        gameSeq = game;
        countdownGraphic = game.countdown;
        animator = countdownGraphic.GetComponent<Animator>();

        countdownGraphic.SetActive(true);
        audioManager.instance.Play(audioManager.instance.countdownSound);
        animator.Play("countdown");

        // Delay until AFTER startGame finishes
        tickToEnd = tickManager.tickCount + ticksLasting;

        // Remove early subscription
        tickManager.onTick -= endCountdown;
        tickManager.onTick += endCountdown;
    }


    public void endCountdown()
    {
        // Only run AFTER startGame has initialized the event system
        if (tickManager.tickCount < gameSeq.tickSinceLastEvent)
            return;

        if (tickManager.tickCount >= tickToEnd)
        {
            tickManager.onTick -= endCountdown;

            countdownGraphic.SetActive(false);
            gameSeq.gameUIS.SetActive(true);

            // Correct way: advance the event safely
            gameSeq.progressEvents();
        }
    }

}
