using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class orangeConveyorBelt : MonoBehaviour
{
    public GameObject collectibleOrange;
    public Transform spawnPoint;
    public int spawnInterval;
    public int nextOrangeSpawn;
    public void startOrangeConveyor()
    {
        nextOrangeSpawn = tickManager.tickCount + spawnInterval;
        spawnOrange();
    }
    public void waitForNextOrange()
    {
        if(tickManager.tickCount >= nextOrangeSpawn)
        {
            spawnOrange();
        }
    }

    public void spawnOrange()
    {
        var newOrange = Instantiate(collectibleOrange, spawnPoint.position, Quaternion.identity);
        newOrange.GetComponent<collectableOrangeScript>().onConveyor = true;
        nextOrangeSpawn = tickManager.tickCount + spawnInterval;
        tickManager.onTick += waitForNextOrange;
    }
}
