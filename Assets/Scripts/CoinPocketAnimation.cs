using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class CoinPocketAnimation : MonoBehaviour
{
    [Header("Sink Animation")]
    public float moveDuration = 0.15f;   // time to reach pocket center
    public float dropDuration = 0.2f;    // time to "fall" through the hole
    public float dropDistance = 0.3f;    // how far down it falls visually
    public float spinSpeed = 720f;       // degrees per second while dropping
    public string pocketedSortingLayer = "Pocketed";

    private SpriteRenderer sr;
    private Collider2D col;
    private Rigidbody2D rb;

    private Coroutine activeSinkRoutine;
    private Vector3 originalPosition;
    private Color originalColor;
    private bool originalsCaptured = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Pocket(Vector2 pocketCenter)
    {
        // Capture the "resting" state once, before any sink animation
        // ever runs, so Cancel can restore it reliably.
        if (!originalsCaptured)
        {
            originalPosition = transform.position;
            originalColor = sr.color;
            originalsCaptured = true;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        if (col != null)
            col.enabled = false;

        sr.sortingLayerName = pocketedSortingLayer;
        sr.sortingOrder = 10;

        if (activeSinkRoutine != null)
            StopCoroutine(activeSinkRoutine);

        activeSinkRoutine = StartCoroutine(SinkRoutine(pocketCenter));
    }

    // NEW: called by GameManager when a queen's sink animation needs
    // to be interrupted (e.g. it's being returned to center for an
    // extra shot before the shrink/fade/drop finished playing out)
    public void CancelPocketAnimation()
    {
        if (activeSinkRoutine != null)
        {
            StopCoroutine(activeSinkRoutine);
            activeSinkRoutine = null;
        }

        // Restore visual/physics state back to normal
        transform.rotation = Quaternion.identity;

        if (originalsCaptured)
            sr.color = originalColor;

        if (rb != null)
            rb.simulated = true;

        if (col != null)
            col.enabled = true;
    }

    private IEnumerator SinkRoutine(Vector2 pocketCenter)
    {
        // ---- Phase 1: slide to pocket center ----
        Vector3 startPos = transform.position;
        Vector3 centerPos = pocketCenter;

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float progress = t / moveDuration;
            transform.position = Vector3.Lerp(startPos, centerPos, progress);
            yield return null;
        }

        transform.position = centerPos;

        // ---- Phase 2: spin + drop + fade ----
        Color startColor = sr.color;
        Vector3 dropStart = centerPos;
        Vector3 dropEnd = centerPos + Vector3.down * dropDistance;

        t = 0f;
        while (t < dropDuration)
        {
            t += Time.deltaTime;
            float progress = t / dropDuration;

            transform.position = Vector3.Lerp(dropStart, dropEnd, progress);
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, progress);
            sr.color = c;

            yield return null;
        }

        gameObject.SetActive(false);

        // Reset for reuse if pooling
        transform.rotation = Quaternion.identity;
        transform.position = startPos; // reset position too if pooled
        sr.color = startColor;
        if (rb != null) rb.simulated = true;
        if (col != null) col.enabled = true;

        activeSinkRoutine = null;
    }
}