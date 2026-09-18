using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sabaEyeScript : MonoBehaviour
{
    public Animator blinkAnim;
    public Animator pupilAnim;
    public GameObject pupil;
    public laneManager laneMngr;
    private enum eyeState {idle, search, shoot};
    public List<int> possibleLanes = new List<int>{0,4,5,9};
    private eyeState state;
    public int ticksBeforeBlink;
    public int blinkTick;
    public int shootTick;
    public int ticksBeforeShoot;
    public GameObject pupilProjectile;
    public float timeGlare;
    public bool glareStarted;
    public GameObject particles;
    void Start()
    {
        Instantiate(particles, transform.position, Quaternion.identity);
        laneMngr = GameObject.Find("gameManager").GetComponent<laneManager>();
        state = eyeState.idle;
        blinkTick = tickManager.tickCount + ticksBeforeBlink;
        shootTick = tickManager.tickCount + ticksBeforeShoot;
        tickManager.onTick += eyeTick;
    }
    public void eyeTick()
    {
        eyeBlink();
        eyeShoot();
    }
    public void eyeBlink()
    {
        if(tickManager.tickCount >= blinkTick)
        {
            blinkAnim.Play("blink");
            blinkTick = tickManager.tickCount + ticksBeforeBlink;
        }  
    }
    public void eyeShoot()
    {
        if(glareStarted == true)
            return;
        if (tickManager.tickCount >= shootTick)
        {
            lane randomLaneShoot = laneMngr.lanes[possibleLanes[Random.Range(0, possibleLanes.Count)]];
            var target = randomLaneShoot.possibleTroopTiles[^1];

            pupilAnim.Play("look");

            // ROTATE ONLY ON Z AXIS
            Vector2 dir = target.transform.position - pupil.transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            pupil.transform.rotation = Quaternion.Euler(0, 0, angle);
            glareStarted = true;
            StartCoroutine(glare(randomLaneShoot));     
        }
    }

    public void OnDestroy()
    {
        tickManager.onTick -= eyeTick;
    }

    public IEnumerator glare(lane whatLane)
    {
        yield return new WaitForSeconds(timeGlare);
        //"shoot" pupil (pupil moves on its own with special behavior)
        audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.glare);
        var pp = Instantiate(pupilProjectile, transform.position, Quaternion.identity);
        pp.GetComponent<projectileScript>().enemyProjectile = true;
        pp.GetComponent<guidedProjectile>().end = new Vector2(GameObject.Find("juniorTable").transform.position.x, whatLane.laneYPos);
        pp.GetComponent<guidedProjectile>().targetLane = new Vector2(0, whatLane.laneYPos);
        shootTick = tickManager.tickCount + ticksBeforeShoot;
        glareStarted = false;
    }
}
