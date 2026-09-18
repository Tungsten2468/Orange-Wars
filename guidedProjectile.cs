using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class guidedProjectile : MonoBehaviour
{
    public float speed = 1f;

    public Vector2 start;
    public Vector2 targetLane;
    public Vector2 end;
    public Vector2 control;

    float t = 0f;

    void Start()
    {
        // Assign start from current position
        start = transform.position;

        control = new Vector2(start.x, targetLane.y);
    }

    void Update()
    {
        t += Time.deltaTime * speed;

        // First interpolation
        Vector2 a = Vector2.Lerp(start, control, t);
        // Second interpolation
        Vector2 b = Vector2.Lerp(control, end, t);

        // Final interpolation
        transform.position = Vector2.Lerp(a, b, t);
    }
}
