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
        // STRIKER FOUL
        //
        // Striker is ALWAYS a foul.
        // Speed does not matter.
        //--------------------------------------------------

        if (other.CompareTag("Striker"))
        {
            if (GameManager.Instance == null)
                return;

            // Ignore duplicate triggers
            if (GameManager.Instance.strikerFoul)
                return;

            Debug.Log("=================================");
            Debug.Log("FOUL — STRIKER POCKETED");
            Debug.Log("=================================");


            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWallHit();
            }


            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }


            GameManager.Instance.strikerFoul = true;

            return;
        }

      
        //--------------------------------------------------
        // POCKET SPEED CHECK
        //
        // Applies only to coins and Queen.
        //--------------------------------------------------

        if (rb != null &&
            rb.linearVelocity.magnitude > maxPocketSpeed)
        {
            return;
        }


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

            GameManager.Instance.coinPocketed = true;

            // NEW: record this coin so it can be returned as a
            // "due" coin if this stroke turns out to be a foul.
            GameManager.Instance.RecordCoinPocketedThisShot(other.gameObject);

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
            bool isLastQueen =
                ScoreManager.Instance.GetNormalCoinsLeft() == 0 &&
                !GameManager.Instance.coinPocketed;


            if (isLastQueen)
            {
                if (GameManager.Instance.lastQueenPendingExtraTurn)
                {
                    Debug.Log(
                        "Last Queen: Duplicate trigger ignored."
                    );

                    return;
                }


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