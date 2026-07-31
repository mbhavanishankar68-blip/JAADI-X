using UnityEngine;

public class StrikerController : MonoBehaviour
{
    [Header("Position Settings")]
    public float leftLimit = -2.5f;
    public float rightLimit = 2.5f;
    public float fixedY = -2.5f;

    [Header("Shoot Settings")]
    public float shootPower = 10f;

    [Header("Trajectory Prediction")]
    public TrajectoryPredictor trajectoryPredictor;
    public float strikerRadius = 0.25f; // match your striker's actual collider radius

    private Rigidbody2D rb;
    private LineRenderer line;

    // Positioning
    private bool positioning = false;

    // Aiming
    private bool aiming = false;
    private Vector2 dragStart;
    private Vector2 currentMouse;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        line = GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.enabled = false;
    }

    void Update()
    {
        currentMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // ---------- POSITION STRIKER ----------
        if (GameManager.Instance.CurrentState == GameManager.GameState.Positioning &&
            !GameManager.Instance.strikerPlaced)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Collider2D hit = Physics2D.OverlapPoint(currentMouse);

                if (hit != null && hit.gameObject == gameObject)
                {
                    positioning = true;
                }
            }

            if (positioning && Input.GetMouseButton(0))
            {
                Vector3 pos = transform.position;

                pos.x = Mathf.Clamp(currentMouse.x, leftLimit, rightLimit);
                pos.y = fixedY;

                transform.position = pos;
            }

            if (positioning && Input.GetMouseButtonUp(0))
            {
                positioning = false;
                GameManager.Instance.strikerPlaced = true;

                Debug.Log("Striker Positioned");
            }

            return;
        }

        // ---------- AIM ----------
        if (GameManager.Instance.CurrentState == GameManager.GameState.Positioning &&
            GameManager.Instance.strikerPlaced)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Collider2D hit = Physics2D.OverlapPoint(currentMouse);

                if (hit != null && hit.gameObject == gameObject)
                {
                    aiming = true;
                    dragStart = currentMouse;

                    line.enabled = true;
                }
            }

            if (aiming && Input.GetMouseButton(0))
            {
                line.SetPosition(0, transform.position);
                line.SetPosition(1, currentMouse);

                // NEW: show predicted bounce path while dragging
                Vector2 direction = dragStart - currentMouse;

                if (direction.sqrMagnitude > 0.0001f && trajectoryPredictor != null)
                {
                    trajectoryPredictor.ShowPrediction(transform.position, direction, strikerRadius);
                }
            }

            if (aiming && Input.GetMouseButtonUp(0))
            {
                aiming = false;

                line.enabled = false;

                // NEW: hide predicted path once shot is fired
                if (trajectoryPredictor != null)
                    trajectoryPredictor.HidePrediction();

                Vector2 direction = dragStart - currentMouse;

                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0;

                rb.AddForce(direction * shootPower, ForceMode2D.Impulse);

                GameManager.Instance.CurrentState = GameManager.GameState.Waiting;

                Debug.Log("Shot Fired");
            }
        }
    }
}