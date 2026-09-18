using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class runeScript : MonoBehaviour
{
    public List<Vector2> positions;
    public int currentPosID;
    void Start()
    {
        currentPosID = 0;
        transform.position = positions[currentPosID];
    }

    public void OnMouseDown()
    {
        audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.runeMove);
        if(currentPosID < positions.Count - 1)
            currentPosID += 1;
        else
            currentPosID = 0;
        transform.position = positions[currentPosID];
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        var proj = other.GetComponent<projectileScript>();
        if(proj == null)
            return;
        
        if(proj.enemyProjectile)
        {
            audioManager.instance.sfxSource.PlayOneShot(audioManager.instance.runeBlock);
        }
    }
}
