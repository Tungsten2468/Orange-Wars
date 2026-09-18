using UnityEngine;
using UnityEngine.UI;

public class troopHolderFitter : MonoBehaviour
{
    public RectTransform parentRect;
    public VerticalLayoutGroup layoutGroup;

    public float minScale = 0.4f;
    public float maxScale = 2f;

    void Update()
    {
        int childCount = parentRect.childCount;
        if (childCount == 0) return;

        float scale = Mathf.Clamp(1f / childCount, minScale, maxScale);

        // Scale children
        for (int i = 0; i < childCount; i++)
        {
            Transform child = parentRect.GetChild(i);
            child.localScale = new Vector3(scale, scale, 1f);
        }

        // Custom spacing curve:
        // scale = 2.0 → spacing = 50
        // scale = 1.5 → spacing = 30
        float spacing = Mathf.Min(50f, 40f * scale - 30f);
        layoutGroup.spacing = spacing;
    }
}
