using UnityEngine;

public class OuterPocketTrigger : MonoBehaviour
{
    public Transform pocketCenter;

    [Header("Rim Physics")]
    public float captureSpeedThreshold = 2.5f; // slower than this = starts getting pulled in

    private void OnTriggerEnter2D(Collider2D other)
    {
        PocketCoinTracker tracker = other.GetComponent<PocketCoinTracker>();
        if (tracker == null) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        if (rb.linearVelocity.magnitude <= captureSpeedThreshold)
        {
            tracker.insidePocket = true;
            tracker.currentPocketCenter = pocketCenter;
        }
        // else: too fast, leave it alone — it skips across naturally
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PocketCoinTracker tracker = other.GetComponent<PocketCoinTracker>();
        if (tracker == null) return;

        if (tracker.currentPocketCenter == pocketCenter)
            tracker.ClearCapture();
    }
}