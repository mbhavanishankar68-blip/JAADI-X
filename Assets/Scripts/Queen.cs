using UnityEngine;

public class Queen : MonoBehaviour
{
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public bool IsMoving()
    {
        return rb.linearVelocity.magnitude > 0.05f;
    }
}