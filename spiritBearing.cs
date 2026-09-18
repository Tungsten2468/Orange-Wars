using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spiritBearing : MonoBehaviour
{
    public GameObject myMaster;
    public entity me;
    public GameObject mySpirit;
    bool quitting = false;

    
    void Start()
    {
        me = GetComponent<entity>();
    }
    void OnApplicationQuit()
    {
        quitting = true;
    }

    void OnDestroy()
    {
        if (!gameObject.scene.IsValid())
            return;
        if(quitting) return; // Prevent spawning during scene unload

        if(myMaster != null)
        {
            var bm = myMaster.GetComponent<bossManager>();
            if(!bm.phases[bm.currentPhase].phaseDefeated)
            {
                var sp = Instantiate(mySpirit, transform.position, Quaternion.identity);
                var enbe = me as enemyBehaviors;
                var spScr = sp.GetComponent<weepingSpiritScript>();
                spScr.iWas = enbe.thisEnemy;
                spScr.master = myMaster;
            }
        }
    }

}
