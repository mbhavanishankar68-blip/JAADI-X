using UnityEngine;
using JaadiX.Core;

public class StrikerController : MonoBehaviour
{
    [Header("Position Settings")]
    public float leftLimit = -2.5f;
    public float rightLimit = 2.5f;
    public float fixedY = -2.5f;

    [Header("Shoot Settings")]
    public float shootPower = 10f;
    public float maxDragDistance = 2f;

    [Header("Trajectory Prediction")]
    public TrajectoryPredictor trajectoryPredictor;
    public float strikerRadius = 0.25f;

    private Rigidbody2D rb;
    private LineRenderer line;

    private bool positioning = false;
    private bool aiming = false;

    private Vector2 dragStart;
    private Vector2 currentMouse;

    // Current drag information
    private float currentDragDistance = 0f;
    private float currentPower01 = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.enabled = false;
    }

    void Update()
    {
        currentMouse =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //==================================================
        // STRIKER POSITIONING
        //==================================================

        if (GameManager.Instance.CurrentState ==
                GameManager.GameState.Positioning &&
            !GameManager.Instance.strikerPlaced)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Collider2D hit =
                    Physics2D.OverlapPoint(currentMouse);

                if (hit != null && hit.gameObject == gameObject)
                {
                    positioning = true;
                }
            }

            if (positioning && Input.GetMouseButton(0))
            {
                Vector3 pos = transform.position;

                pos.x = Mathf.Clamp(
                    currentMouse.x,
                    leftLimit,
                    rightLimit
                );

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

        //==================================================
        // AIMING / SHOOTING
        //==================================================

        if (GameManager.Instance.CurrentState ==
                GameManager.GameState.Positioning &&
            GameManager.Instance.strikerPlaced)
        {
            //--------------------------------------------------
            // START AIMING
            //--------------------------------------------------

            if (Input.GetMouseButtonDown(0))
            {
                Collider2D hit =
                    Physics2D.OverlapPoint(currentMouse);

                if (hit != null && hit.gameObject == gameObject)
                {
                    aiming = true;

                    dragStart = currentMouse;

                    currentDragDistance = 0f;
                    currentPower01 = 0f;

                    line.enabled = true;

                    if (trajectoryPredictor != null)
                        trajectoryPredictor.HidePrediction();
                }
            }

            //--------------------------------------------------
            // DRAGGING
            //--------------------------------------------------

            if (aiming && Input.GetMouseButton(0))
            {
                Vector2 pullVector =
                    dragStart - currentMouse;

                //--------------------------------------------------
                // DRAG DISTANCE
                //--------------------------------------------------

                float rawDragDistance =
                    pullVector.magnitude;

                currentDragDistance =
                    Mathf.Clamp(
                        rawDragDistance,
                        0f,
                        maxDragDistance
                    );

                //--------------------------------------------------
                // POWER 0 → 1
                //--------------------------------------------------

                currentPower01 =
                    Mathf.Clamp01(
                        currentDragDistance /
                        maxDragDistance
                    );

                //--------------------------------------------------
                // AIM DIRECTION
                //--------------------------------------------------

                Vector2 direction;

                if (pullVector.sqrMagnitude > 0.0001f)
                {
                    direction =
                        pullVector.normalized;
                }
                else
                {
                    direction = Vector2.zero;
                }

                //--------------------------------------------------
                // DRAW PULL INDICATOR
                //--------------------------------------------------

                if (direction != Vector2.zero &&
                    currentDragDistance > 0.001f)
                {
                    Vector2 strikerPosition =
                        transform.position;

                    Vector2 pullEndPoint =
                        strikerPosition -
                        direction *
                        currentDragDistance;

                    line.positionCount = 2;

                    line.SetPosition(
                        0,
                        strikerPosition
                    );

                    line.SetPosition(
                        1,
                        pullEndPoint
                    );
                }
                else
                {
                    line.positionCount = 0;
                }

                //--------------------------------------------------
                // TRAJECTORY PREDICTION
                //--------------------------------------------------

                if (direction != Vector2.zero &&
                    currentPower01 > 0.001f &&
                    trajectoryPredictor != null)
                {
                    trajectoryPredictor.ShowPrediction(
                        transform.position,
                        direction,
                        strikerRadius,
                        currentPower01
                    );
                }
                else if (trajectoryPredictor != null)
                {
                    trajectoryPredictor.HidePrediction();
                }
            }

            //--------------------------------------------------
            // RELEASE / SHOOT
            //--------------------------------------------------

            if (aiming && Input.GetMouseButtonUp(0))
            {
                aiming = false;

                line.enabled = false;
                line.positionCount = 0;

                if (trajectoryPredictor != null)
                    trajectoryPredictor.HidePrediction();

                Vector2 pullVector =
                    dragStart - currentMouse;

                //--------------------------------------------------
                // AIM DIRECTION
                //--------------------------------------------------

                Vector2 direction;

                if (pullVector.sqrMagnitude > 0.0001f)
                {
                    direction =
                        pullVector.normalized;
                }
                else
                {
                    direction = Vector2.zero;
                }

                //--------------------------------------------------
                // DRAG DISTANCE
                //--------------------------------------------------

                float dragDistance =
                    Mathf.Clamp(
                        pullVector.magnitude,
                        0f,
                        maxDragDistance
                    );

                //--------------------------------------------------
                // POWER 0 → 1
                //--------------------------------------------------

                float power01 =
                    Mathf.Clamp01(
                        dragDistance /
                        maxDragDistance
                    );

                //--------------------------------------------------
                // ACTUAL FORCE
                //--------------------------------------------------

                float actualPower =
                    power01 * shootPower;

                //--------------------------------------------------
                // FIRE
                //--------------------------------------------------

                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;

                if (direction != Vector2.zero &&
                    actualPower > 0f)
                {
                    rb.AddForce(
                        direction * actualPower,
                        ForceMode2D.Impulse
                    );
                }

                //--------------------------------------------------
                // GAME STATE
                //--------------------------------------------------

                GameManager.Instance.CurrentState =
                    GameManager.GameState.Waiting;

                Debug.Log(
                    "Shot Fired | " +
                    "Power: " +
                    actualPower.ToString("F2") +
                    " / " +
                    shootPower.ToString("F2")
                );
            }
        }
    }

    //==================================================
    // PUBLIC POWER VALUES
    //==================================================

    public float GetCurrentPower()
    {
        return currentPower01;
    }

    public float GetCurrentDragDistance()
    {
        return currentDragDistance;
    }
}