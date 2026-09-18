using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weepingSpiritScript : MonoBehaviour
{
    public enemySO iWas;
    public GameObject master;
    public Rigidbody2D rb;
    
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        rb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
        StartCoroutine(floatToMaster());
    }
    IEnumerator floatToMaster()
    {
        yield return new WaitForSeconds(0.8f);
        while (Vector2.Distance(rb.position, master.transform.position) > 0.05f)
        {
            if(master == null)
                Destroy(gameObject);
            rb.MovePosition(Vector2.MoveTowards(
                rb.position,
                master.transform.position,
                25f * Time.deltaTime
            ));
            yield return null; 
        }
        master.GetComponent<bossManager>().onMinionDeath(iWas);
        Destroy(gameObject);
    }
}
