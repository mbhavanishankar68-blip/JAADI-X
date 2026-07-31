using UnityEngine;
using System.Collections.Generic;

public class PocketGravity : MonoBehaviour
{
    [Header("References")]
    public Transform pocketCenter;       // assign this pocket's PocketCenter child

    [Header("Gravity Settings")]
    public float captureSpeed = 1.5f;    // above this speed, no pull is applied at all
    public float pullStrength = 6f;      // max force applied when nearly stationary
    public float maxPullSpeed = 3f;      // clamp so it doesn't fling coin into center too hard

    private readonly List<Rigidbody2D> coinsInRange = new List<Rigidbody2D>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPocketable(other)) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null && !coinsInRange.Contains(rb))
            coinsInRange.Add(rb);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null) coinsInRange.Remove(rb);
    }

    private void FixedUpdate()
    {
        for (int i = coinsInRange.Count - 1; i >= 0; i--)
        {
            Rigidbody2D rb = coinsInRange[i];

            if (rb == null || !rb.gameObject.activeInHierarchy)
            {
                coinsInRange.RemoveAt(i);
                continue;
            }

            float speed = rb.linearVelocity.magnitude;

            // Too fast -> pocket has no grip on it, let it slide through freely
            if (speed > captureSpeed) continue;

            Vector2 toCenter = (Vector2)pocketCenter.position - rb.position;
            float dist = toCenter.magnitude;

            if (dist < 0.01f) continue;

            // Pull gets stronger the slower & closer the coin is (mimics rim slope)
            float speedFactor = 1f - Mathf.Clamp01(speed / captureSpeed);
            float pull = pullStrength * speedFactor;

            rb.AddForce(toCenter.normalized * pull, ForceMode2D.Force);

            // Clamp so it doesn't accelerate into a slingshot
            if (rb.linearVelocity.magnitude > maxPullSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxPullSpeed;
        }
    }

    private bool IsPocketable(Collider2D other)
    {
        return other.CompareTag("Coin") || other.CompareTag("Queen");
    }
}