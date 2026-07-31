using UnityEngine;

public class PocketScript : MonoBehaviour
{
    [Header("Rim Physics")]
    public float maxPocketSpeed = 4f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null && rb.linearVelocity.magnitude > maxPocketSpeed)
            return;

        PocketCoinTracker tracker = other.GetComponent<PocketCoinTracker>();
        if (tracker != null)
            tracker.ClearCapture();

        //--------------------------------------------------
        // NORMAL COIN
        //--------------------------------------------------
        if (other.CompareTag("Coin"))
        {
            AudioManager.Instance.PlayCoinPocket();

            CoinPocketAnimation coinAnim = other.GetComponent<CoinPocketAnimation>();
            if (coinAnim != null)
                coinAnim.Pocket(transform.position);
            else
                other.gameObject.SetActive(false);

            GameManager.Instance.coinPocketed = true;

            // If a queen-cover is pending, this coin covers it —
            // whether the queen fell before OR after it in this shot.
            if (GameManager.Instance.waitingForQueenCover)
            {
                GameManager.Instance.queenCovered = true;
                Debug.Log("Queen Covered!");
            }

            ScoreManager.Instance.AddPoint();
            return;
        }

        //--------------------------------------------------
        // QUEEN
        //--------------------------------------------------
        if (other.CompareTag("Queen"))
        {
            //--------------------------------------------------
            // LAST QUEEN (DISC POOL RULE)
            //--------------------------------------------------
            if (ScoreManager.Instance.GetNormalCoinsLeft() == 0)
            {
                if (!GameManager.Instance.queenReturnedOnce)
                {
                    // FIRST pocket this cycle: bounce back to centre and
                    // request a bonus shot. Do NOT touch CurrentState
                    // here — GameManager.EndTurn() performs the actual
                    // transition once physics has settled.
                    Debug.Log("Last Queen Pocketed — Returning To Centre");

                    AudioManager.Instance.PlayQueenPocket();

                    GameManager.Instance.queenReturnedOnce = true;

                    CoinPocketAnimation queenReturnAnim = other.GetComponent<CoinPocketAnimation>();
                    if (queenReturnAnim != null)
                        queenReturnAnim.CancelPocketAnimation();

                    Rigidbody2D queenRb = other.GetComponent<Rigidbody2D>();
                    if (queenRb != null)
                    {
                        queenRb.linearVelocity = Vector2.zero;
                        queenRb.angularVelocity = 0f;
                    }

                    other.transform.position =
                        GameManager.Instance.queenStartPosition.position;

                    other.gameObject.SetActive(true);

                    GameManager.Instance.lastQueenPendingExtraTurn = true;

                    return;
                }

                //--------------------------------------------------
                // SECOND pocket (the bonus shot itself) -> WIN
                //--------------------------------------------------
                Debug.Log("Last Queen Pocketed Again");

                AudioManager.Instance.PlayQueenPocket();

                CoinPocketAnimation queenWinAnim = other.GetComponent<CoinPocketAnimation>();
                if (queenWinAnim != null)
                    queenWinAnim.Pocket(transform.position);
                else
                    other.gameObject.SetActive(false);

                GameManager.Instance.queenPocketed = true;

                return;
            }

            //--------------------------------------------------
            // NORMAL QUEEN
            //--------------------------------------------------
            Debug.Log("Queen Pocketed");

            AudioManager.Instance.PlayQueenPocket();

            CoinPocketAnimation normalQueenAnim = other.GetComponent<CoinPocketAnimation>();
            if (normalQueenAnim != null)
                normalQueenAnim.Pocket(transform.position);
            else
                other.gameObject.SetActive(false);

            GameManager.Instance.queenPocketed = true;
            GameManager.Instance.waitingForQueenCover = true;

            return;
        }
    }
}