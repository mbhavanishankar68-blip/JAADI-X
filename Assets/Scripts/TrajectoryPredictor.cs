using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Prediction Settings")]
    public int maxBounces = 3;

    [Tooltip("Maximum trajectory distance at full power.")]
    public float maxDistance = 15f;

    public LayerMask collisionMask;

    [Header("Line Appearance")]
    public Color lineColor = Color.white;

    public float dotSpacing = 0.30f;

    public float lineWidth = 0.08f;

    [Header("Trajectory Glow")]
    public LineRenderer trajectoryGlow;

    public float glowWidth = 0.13f;

    [Header("Arrowhead")]
    public SpriteRenderer arrowHead;

    public float arrowSize = 0.3f;

    [Header("Impact Guide")]
    public SpriteRenderer impactGuide;

    public float impactGuideSize = 1.0f;

    private LineRenderer line;

    //======================================================
    // AWAKE
    //======================================================

    void Awake()
    {
        line = GetComponent<LineRenderer>();

        //--------------------------------------------------
        // MAIN LINE
        //--------------------------------------------------

        line.startColor = lineColor;
        line.endColor = lineColor;

        line.textureMode =
            LineTextureMode.Tile;

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        //--------------------------------------------------
        // GLOW
        //--------------------------------------------------

        if (trajectoryGlow != null)
        {
            trajectoryGlow.enabled = false;

            trajectoryGlow.startWidth =
                glowWidth;

            trajectoryGlow.endWidth =
                glowWidth;

            Color glowColor =
                new Color(
                    lineColor.r,
                    lineColor.g,
                    lineColor.b,
                    0.25f
                );

            trajectoryGlow.startColor =
                glowColor;

            trajectoryGlow.endColor =
                glowColor;

            trajectoryGlow.textureMode =
                LineTextureMode.Tile;
        }

        //--------------------------------------------------
        // ARROW
        //--------------------------------------------------

        if (arrowHead != null)
        {
            arrowHead.enabled = false;
        }

        //--------------------------------------------------
        // IMPACT GUIDE
        //--------------------------------------------------

        if (impactGuide != null)
        {
            impactGuide.enabled = false;

            impactGuide.transform.localScale =
                Vector3.one *
                impactGuideSize;
        }
    }

    //======================================================
    // SHOW PREDICTION
    //======================================================

    public void ShowPrediction(
        Vector2 origin,
        Vector2 direction,
        float strikerRadius,
        float power01
    )
    {
        //--------------------------------------------------
        // SAFETY
        //--------------------------------------------------

        if (direction.sqrMagnitude < 0.0001f)
        {
            HidePrediction();
            return;
        }

        //--------------------------------------------------
        // CLAMP POWER
        //--------------------------------------------------

        power01 =
            Mathf.Clamp01(power01);

        //--------------------------------------------------
        // POWER BASED DISTANCE
        //--------------------------------------------------

        // Minimum visible prediction distance
        float minimumDistance =
            maxDistance * 0.20f;

        float predictionDistance =
            Mathf.Lerp(
                minimumDistance,
                maxDistance,
                power01
            );

        //--------------------------------------------------
        // ENABLE MAIN LINE
        //--------------------------------------------------

        line.enabled = true;

        Vector2 currentPos =
            origin;

        Vector2 currentDir =
            direction.normalized;

        float remainingDistance =
            predictionDistance;

        //--------------------------------------------------
        // MAIN LINE INITIALIZATION
        //--------------------------------------------------

        line.positionCount = 1;

        line.SetPosition(
            0,
            currentPos
        );

        //--------------------------------------------------
        // GLOW INITIALIZATION
        //--------------------------------------------------

        if (trajectoryGlow != null)
        {
            trajectoryGlow.enabled = true;

            trajectoryGlow.positionCount = 1;

            trajectoryGlow.SetPosition(
                0,
                currentPos
            );
        }

        //--------------------------------------------------
        // TRACKING
        //--------------------------------------------------

        float totalLength = 0f;

        Vector2 lastSegmentEnd =
            currentPos;

        Vector2 lastDirection =
            currentDir;

        //--------------------------------------------------
        // HIDE IMPACT GUIDE INITIALLY
        //--------------------------------------------------

        if (impactGuide != null)
        {
            impactGuide.enabled = false;
        }

        //--------------------------------------------------
        // PREDICTION LOOP
        //--------------------------------------------------

        for (int bounce = 0;
             bounce < maxBounces;
             bounce++)
        {
            if (remainingDistance <= 0.001f)
                break;

            //--------------------------------------------------
            // CIRCLE CAST
            //--------------------------------------------------

            RaycastHit2D hit =
                Physics2D.CircleCast(
                    currentPos,
                    strikerRadius,
                    currentDir,
                    remainingDistance,
                    collisionMask
                );

            //--------------------------------------------------
            // COLLISION FOUND
            //--------------------------------------------------

            if (hit.collider != null)
            {
                Vector2 hitPoint =
                    hit.point +
                    hit.normal *
                    strikerRadius *
                    0.01f;

                //--------------------------------------------------
                // MAIN LINE
                //--------------------------------------------------

                line.positionCount++;

                line.SetPosition(
                    line.positionCount - 1,
                    hitPoint
                );

                //--------------------------------------------------
                // GLOW
                //--------------------------------------------------

                if (trajectoryGlow != null)
                {
                    trajectoryGlow.positionCount++;

                    trajectoryGlow.SetPosition(
                        trajectoryGlow.positionCount - 1,
                        hitPoint
                    );
                }

                //--------------------------------------------------
                // DISTANCE
                //--------------------------------------------------

                totalLength +=
                    hit.distance;

                remainingDistance -=
                    hit.distance;

                //--------------------------------------------------
                // FINAL SEGMENT
                //--------------------------------------------------

                lastSegmentEnd =
                    hitPoint;

                lastDirection =
                    currentDir;

                //--------------------------------------------------
                // COIN / QUEEN
                //--------------------------------------------------

                bool isCoin =
                    hit.collider.CompareTag("Coin") ||
                    hit.collider.CompareTag("Queen");

                if (isCoin)
                {
                    //--------------------------------------------------
                    // IMPACT GUIDE
                    //--------------------------------------------------

                    if (impactGuide != null)
                    {
                        impactGuide.enabled =
                            true;

                        impactGuide.transform.position =
                            hit.collider.transform.position;

                        impactGuide.transform.localScale =
                            Vector3.one *
                            impactGuideSize;
                    }

                    //--------------------------------------------------
                    // STOP AT COIN
                    //--------------------------------------------------

                    break;
                }

                //--------------------------------------------------
                // WALL REFLECTION
                //--------------------------------------------------

                currentDir =
                    Vector2.Reflect(
                        currentDir,
                        hit.normal
                    ).normalized;

                currentPos =
                    hitPoint;

                continue;
            }

            //--------------------------------------------------
            // NO COLLISION
            //--------------------------------------------------

            Vector2 endPoint =
                currentPos +
                currentDir *
                remainingDistance;

            //--------------------------------------------------
            // MAIN LINE
            //--------------------------------------------------

            line.positionCount++;

            line.SetPosition(
                line.positionCount - 1,
                endPoint
            );

            //--------------------------------------------------
            // GLOW
            //--------------------------------------------------

            if (trajectoryGlow != null)
            {
                trajectoryGlow.positionCount++;

                trajectoryGlow.SetPosition(
                    trajectoryGlow.positionCount - 1,
                    endPoint
                );
            }

            //--------------------------------------------------
            // DISTANCE
            //--------------------------------------------------

            totalLength +=
                remainingDistance;

            //--------------------------------------------------
            // FINAL SEGMENT
            //--------------------------------------------------

            lastSegmentEnd =
                endPoint;

            lastDirection =
                currentDir;

            break;
        }

        //--------------------------------------------------
        // DASH SPACING
        //--------------------------------------------------

        if (line.material != null &&
            dotSpacing > 0f)
        {
            float tileCount =
                totalLength /
                dotSpacing;

            Vector2 textureScale =
                line.material.mainTextureScale;

            textureScale.x =
                tileCount;

            line.material.mainTextureScale =
                textureScale;
        }

        //--------------------------------------------------
        // ARROWHEAD
        //--------------------------------------------------

        if (arrowHead != null)
        {
            arrowHead.enabled = true;

            arrowHead.transform.position =
                lastSegmentEnd;

            float angle =
                Mathf.Atan2(
                    lastDirection.y,
                    lastDirection.x
                ) *
                Mathf.Rad2Deg -
                90f;

            arrowHead.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle
                );

            arrowHead.transform.localScale =
                Vector3.one *
                arrowSize;
        }
    }

    //======================================================
    // HIDE PREDICTION
    //======================================================

    public void HidePrediction()
    {
        //--------------------------------------------------
        // MAIN LINE
        //--------------------------------------------------

        line.enabled = false;

        line.positionCount = 0;

        //--------------------------------------------------
        // GLOW
        //--------------------------------------------------

        if (trajectoryGlow != null)
        {
            trajectoryGlow.enabled = false;

            trajectoryGlow.positionCount = 0;
        }

        //--------------------------------------------------
        // ARROW
        //--------------------------------------------------

        if (arrowHead != null)
        {
            arrowHead.enabled = false;
        }

        //--------------------------------------------------
        // IMPACT GUIDE
        //--------------------------------------------------

        if (impactGuide != null)
        {
            impactGuide.enabled = false;
        }
    }
}