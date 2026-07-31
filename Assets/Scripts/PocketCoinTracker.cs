using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PocketCoinTracker : MonoBehaviour
{
    [HideInInspector] public bool insidePocket = false;
    [HideInInspector] public Transform currentPocketCenter;

    [Header("Pocket Gravity")]
    public float pullStrength = 12f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!insidePocket || currentPocketCenter == null)
            return;

        Vector2 toCenter = (Vector2)currentPocketCenter.position - rb.position;
        rb.AddForce(toCenter.normalized * pullStrength, ForceMode2D.Force);
    }

    public void ClearCapture()
    {
        insidePocket = false;
        currentPocketCenter = null;
    }
}