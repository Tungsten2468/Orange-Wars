using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class randomLoading : MonoBehaviour
{
    public List<Sprite> loadingBGs;
    public Image thisImage;

    public void OnEnable()
    {
        thisImage.sprite = loadingBGs[Random.Range(0, loadingBGs.Count)];
    }
}
