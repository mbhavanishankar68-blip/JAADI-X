using UnityEngine;
using JaadiX.Core;

public class PocketScript : MonoBehaviour
{
    [Header("Rim Physics")]
    public float maxPocketSpeed = 4f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;

        //--------------------------------------------------
        // POCKET SPEED CHECK
        //--------------------------------------------------

        if (rb != null && rb.linearVelocity.magnitude > maxPocketSpeed)
            return;


        //--------------------------------------------------
        // CLEAR POCKET CAPTURE
        //--------------------------------------------------

        PocketCoinTracker tracker =
            other.GetComponent<PocketCoinTracker>();

        if (tracker != null)
            tracker.ClearCapture();


        //--------------------------------------------------
        // NORMAL COIN
        //--------------------------------------------------

        if (other.CompareTag("Coin"))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoinPocket();
            }

            CoinPocketAnimation coinAnim =
                other.GetComponent<CoinPocketAnimation>();

            if (coinAnim != null)
            {
                coinAnim.Pocket(transform.position);
            }
            else
            {
                other.gameObject.SetActive(false);
            }

            //--------------------------------------------------
            // Records that a normal coin was pocketed THIS shot
            //--------------------------------------------------

            GameManager.Instance.coinPocketed = true;

            //--------------------------------------------------
            // QUEEN COVER
            //
            // Handles: Queen -> Coin (same shot), and
            //          Queen -> [bonus shot] -> Coin
            //--------------------------------------------------

            if (GameManager.Instance.waitingForQueenCover)
            {
                GameManager.Instance.queenCovered = true;

                Debug.Log("Queen Covered!");
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddPoint();
            }

            return;
        }


        //--------------------------------------------------
        // QUEEN
        //--------------------------------------------------

        if (other.CompareTag("Queen"))
        {
            //--------------------------------------------------
            // DETERMINE IF THIS IS LAST QUEEN
            //
            // Last queen = no normal coins remain AND none was
            // pocketed in THIS shot either (otherwise a coin
            // from this same stroke already covers it — treat
            // as a normal queen instead).
            //--------------------------------------------------

            bool isLastQueen =
                ScoreManager.Instance.GetNormalCoinsLeft() == 0 &&
                !GameManager.Instance.coinPocketed;


            //--------------------------------------------------
            // LAST QUEEN
            //--------------------------------------------------

            if (isLastQueen)
            {
                //--------------------------------------------------
                // IGNORE DUPLICATE TRIGGER
                //--------------------------------------------------

                if (GameManager.Instance.lastQueenPendingExtraTurn)
                {
                    Debug.Log(
                        "Last Queen: Duplicate trigger ignored."
                    );

                    return;
                }


                //--------------------------------------------------
                // FIRST LAST-QUEEN POCKET
                //
                // IMPORTANT: do NOT reposition or reactivate the
                // queen here. Just play the normal sink animation
                // and raise the pending flag. GameManager.EndTurn()
                // -> StartLastQueenExtraTurn() performs the actual
                // reposition, only once the striker and all coins
                // have fully stopped moving.
                //--------------------------------------------------

                if (!GameManager.Instance.lastQueenExtraShot)
                {
                    Debug.Log(
                        "Last Queen Pocketed — Waiting For Shot To Settle"
                    );

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayQueenPocket();
                    }

                    GameManager.Instance.queenCovered = false;
                    GameManager.Instance.waitingForQueenCover = false;
                    GameManager.Instance.queenPocketed = false;
                    GameManager.Instance.queenReturnedOnce = true;

                    CoinPocketAnimation queenReturnAnim =
                        other.GetComponent<CoinPocketAnimation>();

                    if (queenReturnAnim != null)
                    {
                        queenReturnAnim.Pocket(transform.position);
                    }
                    else
                    {
                        other.gameObject.SetActive(false);
                    }

                    GameManager.Instance.lastQueenPendingExtraTurn = true;

                    return;
                }


                //--------------------------------------------------
                // SECOND LAST-QUEEN POCKET (the bonus shot itself)
                // -> WIN
                //--------------------------------------------------

                if (GameManager.Instance.lastQueenExtraShot)
                {
                    Debug.Log(
                        "Last Queen Pocketed Again — GAME OVER"
                    );

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayQueenPocket();
                    }

                    CoinPocketAnimation queenWinAnim =
                        other.GetComponent<CoinPocketAnimation>();

                    if (queenWinAnim != null)
                    {
                        queenWinAnim.Pocket(transform.position);
                    }
                    else
                    {
                        other.gameObject.SetActive(false);
                    }

                    GameManager.Instance.queenPocketed = true;

                    return;
                }

                return;
            }


            //--------------------------------------------------
            // NORMAL QUEEN
            //
            // Handles both orders within the same shot:
            //   Queen -> Coin   (Coin branch sets queenCovered)
            //   Coin -> Queen   (set here via coinPocketed)
            //
            // If neither happens this shot, GameManager grants
            // one bonus shot before respotting (official rule).
            //--------------------------------------------------

            Debug.Log("Queen Pocketed");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayQueenPocket();
            }

            CoinPocketAnimation normalQueenAnim =
                other.GetComponent<CoinPocketAnimation>();

            if (normalQueenAnim != null)
            {
                normalQueenAnim.Pocket(transform.position);
            }
            else
            {
                other.gameObject.SetActive(false);
            }

            GameManager.Instance.queenPocketed = true;
            GameManager.Instance.waitingForQueenCover = true;

            // If a normal coin was already pocketed BEFORE the
            // queen in this same shot, it's covered immediately.
            GameManager.Instance.queenCovered =
                GameManager.Instance.coinPocketed;

            if (GameManager.Instance.queenCovered)
            {
                Debug.Log(
                    "Queen Covered — Normal Coin Was Pocketed First"
                );
            }
            else
            {
                Debug.Log(
                    "Queen Waiting For Cover"
                );
            }

            return;
        }
    }
}