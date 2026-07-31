using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Prediction Settings")]
    public int maxBounces = 3;
    public float maxDistance = 15f;
    public LayerMask collisionMask; // set this to Walls + Coins layers in Inspector

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void ShowPrediction(Vector2 origin, Vector2 direction, float strikerRadius)
    {
        line.enabled = true;

        Vector2 currentPos = origin;
        Vector2 currentDir = direction.normalized;
        float remainingDistance = maxDistance;

        line.positionCount = 1;
        line.SetPosition(0, currentPos);

        for (int bounce = 0; bounce < maxBounces; bounce++)
        {
            RaycastHit2D hit = Physics2D.CircleCast(
                currentPos,
                strikerRadius,
                currentDir,
                remainingDistance,
                collisionMask
            );

            line.positionCount++;

            if (hit.collider != null)
            {
                Vector2 hitPoint = hit.point + hit.normal * strikerRadius * 0.01f;
                line.SetPosition(line.positionCount - 1, hitPoint);

                remainingDistance -= hit.distance;

                // Stop bending the line if it hit a coin, not a wall
                if (!hit.collider.CompareTag("Board") &&
                    (hit.collider.CompareTag("Coin") || hit.collider.CompareTag("Queen")))
                {
                    break;
                }

                // Reflect direction off the wall normal for next bounce
                currentDir = Vector2.Reflect(currentDir, hit.normal);
                currentPos = hitPoint;
            }
            else
            {
                // No hit — draw straight to max distance and stop
                Vector2 endPoint = currentPos + currentDir * remainingDistance;
                line.SetPosition(line.positionCount - 1, endPoint);
                break;
            }
        }
    }

    public void HidePrediction()
    {
        line.enabled = false;
        line.positionCount = 0;
    }
}