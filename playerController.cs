using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class playerController : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    public Rigidbody2D rb;
    public Animator animator;
    [HideInInspector]public Vector3 rightFacing;
    [HideInInspector]public Vector3 leftFacing;
    public LayerMask groundLayer;
    void Start()
    {
        rightFacing = transform.localScale;
        leftFacing = transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }
    void Update()
    {
        rb.velocity = new Vector2(Input.GetAxis("Horizontal") * speed, rb.velocity.y);

        if(Input.GetAxis("Horizontal") < 0)
        {
            transform.localScale = leftFacing;
        }
        else
        {
            transform.localScale = rightFacing;
        }
        animator.SetBool("move", rb.velocity != Vector2.zero);
        RaycastHit2D aboveGround = Physics2D.Raycast(transform.position, Vector2.down, 1, groundLayer);
        animator.SetBool("air", !aboveGround);

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(aboveGround)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }
}
