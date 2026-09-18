using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class peelCollectedScript : MonoBehaviour
{
    public Transform peelNapkin;
    public Rigidbody2D rb;
    public GameSequence game;
    public float peelsToAdd;
    void Start()
    {
        game = GameObject.Find("gameManager").GetComponent<GameSequence>();
        peelNapkin = GameObject.Find("peelNapkin").transform;
        StartCoroutine(flyPeel(rb, peelNapkin.position));
    }

    public IEnumerator flyPeel(Rigidbody2D peelRb, Vector2 peelTarget)
    {
        while (Vector2.Distance(peelRb.position, peelTarget) > 0.05f)
        {
            peelRb.MovePosition(Vector2.MoveTowards(
                peelRb.position,
                peelTarget,
                10f * Time.deltaTime
            ));

            yield return null; // THIS prevents freezing
        }
        audioManager.instance.sfxSource.pitch = Random.Range(0.9f, 1.1f);
        audioManager.instance.Play(audioManager.instance.peelCollected);
        audioManager.instance.sfxSource.pitch = 1f;
        Destroy(peelRb.gameObject);
        game.addPeels(peelsToAdd);
    }
}
