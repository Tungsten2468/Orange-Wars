using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class orangeNapkinVisual : MonoBehaviour
{
    //Intended to work with both orange and peel piles 
    public List<GameObject> visibleOranges;
    public GameObject draggableOrange; //for the peel pile, this wont be draggable, just the peel visual that shows one individual peel
    public GameSequence game;
    public void Start()
    {
        updateOrangesVisual();
        updateOrangePeelVisual();
    }
    public void updateOrangesVisual()
    {
        // Hide all oranges first
        foreach (var o in visibleOranges)
            o.SetActive(false);

        // Handle draggable orange
        draggableOrange.SetActive(game.oranges > 0);

        // If we have fewer visuals than oranges, stop early
        if (visibleOranges.Count == 0)
            return;

        // Show the correct number of oranges
        float countToShow = Mathf.Min(game.oranges, visibleOranges.Count);

        for (int i = 0; i < countToShow; i++)
            visibleOranges[i].SetActive(true);
    }


    public void updateOrangePeelVisual()
    {
        foreach (var o in visibleOranges)
            o.SetActive(false);

        draggableOrange.SetActive(game.orangePeels > 0);

        if (visibleOranges.Count == 0)
            return;

        float countToShow = Mathf.Min(game.orangePeels, visibleOranges.Count);

        for (int i = 0; i < countToShow; i++)
            visibleOranges[i].SetActive(true);
    }

}
