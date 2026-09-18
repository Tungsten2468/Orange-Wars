using UnityEngine;

public class playerUIScript : MonoBehaviour
{
    public void LateUpdate()
    {
        // Keep scale positive so it never flips
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;

        // Optional: keep rotation fixed
        transform.rotation = Quaternion.identity;
    }
}
