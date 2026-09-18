using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collectMyEnemyFriends : MonoBehaviour
{
    public List<enemyBehaviors> enemiesInArea = new List<enemyBehaviors>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        enemyBehaviors enemyFound = other.GetComponent<enemyBehaviors>();

        if (enemyFound == null)
            return;

        if (!enemiesInArea.Contains(enemyFound))
            enemiesInArea.Add(enemyFound);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        enemyBehaviors enemyFound = other.GetComponent<enemyBehaviors>();

        if (enemyFound == null)
            return;

        enemiesInArea.Remove(enemyFound);
    }

    private void Update()
    {
        // Clean up dead enemies
        for (int i = enemiesInArea.Count - 1; i >= 0; i--)
        {
            if (enemiesInArea[i] == null)
                enemiesInArea.RemoveAt(i);
        }
    }
}
