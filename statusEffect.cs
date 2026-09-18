using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public enum effect
{
    slow,
    freeze,
    boost,
    invincible,
    suffocation,
    sunscreened,
    eyesting
}
[CreateAssetMenu(fileName = "New Status Effect", menuName = "Orange Wars/Status Effects")]
public class statusEffect : ScriptableObject
{
    public float duration;
    public effect thisEffect;
    public float magnitude;
    public int ticksBetweenAction;
    public AudioClip effectSound;
    public GameObject swappedItem; //for when swapping stuff
}
