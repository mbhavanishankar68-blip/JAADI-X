    using UnityEngine;

    public class StopMoving : MonoBehaviour
    {
        Rigidbody2D rb;

        public float stopSpeed = 0.05f;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            if (rb.linearVelocity.magnitude < stopSpeed)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }