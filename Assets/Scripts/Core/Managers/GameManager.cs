using UnityEngine;
using System.Collections.Generic;
using JaadiX.Coins;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    //--------------------------------------------------
    // GAME STATE
    //--------------------------------------------------

    public enum GameState
    {
        Positioning,
        Waiting,
        GameOver
    }

    public GameState CurrentState;


    //--------------------------------------------------
    // STRIKER
    //--------------------------------------------------

    [Header("Striker")]
    public Transform striker;
    public Transform strikerStart;

    [HideInInspector]
    public bool strikerPlaced = false;


    //--------------------------------------------------
    // QUEEN
    //--------------------------------------------------

    [Header("Queen")]
    public Transform queen;
    public Transform queenStartPosition;

    public bool queenPocketed = false;

    public bool waitingForQueenCover = false;

    public bool queenCovered = false;

    public bool queenCoverExtraShotGranted = false;


    //--------------------------------------------------
    // LAST QUEEN
    //--------------------------------------------------

    public bool lastQueenExtraShot = false;

    public bool queenReturnedOnce = false;

    public bool lastQueenPendingExtraTurn = false;


    //--------------------------------------------------
    // TURN
    //--------------------------------------------------

    public bool coinPocketed = false;


    //--------------------------------------------------
    // FOUL
    //--------------------------------------------------

    public bool strikerFoul = false;

    [Header("Foul - Due Coins")]
    [Tooltip("Where due coins are placed back on a foul. If left empty, queenStartPosition is used (standard board centre).")]
    public Transform dueCoinReturnPoint;
    public float dueCoinScatterRadius = 0.3f;

    // Normal coins pocketed during the CURRENT shot. Used to
    // return them as "due" coins if the shot turns out to be a
    // foul. Cleared after every shot resolves, foul or not.
    private readonly List<GameObject> coinsPocketedThisShot = new List<GameObject>();


    //--------------------------------------------------
    // COINS
    //--------------------------------------------------

    private Coin[] coins;


    //--------------------------------------------------
    // AWAKE
    //--------------------------------------------------

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    //--------------------------------------------------
    // START
    //--------------------------------------------------

    void Start()
    {
        CurrentState = GameState.Positioning;

        strikerPlaced = false;

        Debug.Log("=================================");
        Debug.Log("Game Manager Started");
        Debug.Log("=================================");

        InitializeGameplay();
    }


    //--------------------------------------------------
    // INITIALIZE GAMEPLAY
    //--------------------------------------------------

    public void InitializeGameplay()
    {
        coins = FindObjectsByType<Coin>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        Debug.Log(
            "GameManager found " +
            coins.Length +
            " pieces."
        );

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.InitializeScore();
        }
        else
        {
            Debug.LogError(
                "ScoreManager.Instance is NULL!"
            );
        }

        CurrentState = GameState.Positioning;
    }


    //--------------------------------------------------
    // RECORD A COIN POCKETED THIS SHOT
    //--------------------------------------------------
    //
    // Called by PocketScript whenever a normal coin is pocketed.
    // If this shot turns out to be a foul, these coins are
    // returned to the board as "due" coins.
    //--------------------------------------------------

    public void RecordCoinPocketedThisShot(GameObject coinObject)
    {
        if (coinObject == null)
            return;

        if (!coinsPocketedThisShot.Contains(coinObject))
            coinsPocketedThisShot.Add(coinObject);
    }


    //--------------------------------------------------
    // UPDATE
    //--------------------------------------------------

    void Update()
    {
        if (CurrentState != GameState.Waiting)
            return;

        if (AllCoinsStopped())
        {
            EndTurn();
        }
    }


    //--------------------------------------------------
    // CHECK ALL COINS STOPPED
    //--------------------------------------------------

    bool AllCoinsStopped()
    {
        if (striker == null)
            return false;

        Rigidbody2D strikerRb =
            striker.GetComponent<Rigidbody2D>();

        if (strikerRb == null)
            return false;

        if (strikerRb.linearVelocity.magnitude > 0.05f)
            return false;

        if (coins == null)
            return false;

        foreach (Coin coin in coins)
        {
            if (coin == null)
                continue;

            if (!coin.gameObject.activeInHierarchy)
                continue;

            if (coin.IsMoving())
                return false;
        }

        return true;
    }


    //--------------------------------------------------
    // CHECK REAL BOARD STATE
    //--------------------------------------------------

    bool ArePiecesStillOnBoard()
    {
        Coin[] activeCoins = FindObjectsByType<Coin>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        int activeCount = 0;

        foreach (Coin coin in activeCoins)
        {
            if (coin == null)
                continue;

            if (!coin.gameObject.activeInHierarchy)
                continue;

            activeCount++;
        }

        Debug.Log(
            "Actual active pieces on board: " +
            activeCount
        );

        return activeCount > 0;
    }


    //--------------------------------------------------
    // END TURN (wrapper — guarantees the per-shot coin
    // record list is always cleared afterward, regardless
    // of which branch/return path EndTurnInternal takes)
    //--------------------------------------------------

    void EndTurn()
    {
        EndTurnInternal();

        coinsPocketedThisShot.Clear();
    }


    //--------------------------------------------------
    // END TURN INTERNAL
    //--------------------------------------------------

    void EndTurnInternal()
    {
        Debug.Log("Turn Finished");


        //--------------------------------------------------
        // STRIKER FOUL — overrides everything else that
        // happened this stroke. Checked first, always.
        //--------------------------------------------------

        if (strikerFoul)
        {
            Debug.Log("FOUL — Striker Pocketed This Stroke");

            strikerFoul = false;

            bool wasLastQueenInProgress =
                lastQueenPendingExtraTurn || lastQueenExtraShot;

            if (wasLastQueenInProgress)
            {
                lastQueenPendingExtraTurn = false;
                lastQueenExtraShot = false;
                queenReturnedOnce = false;
            }

            bool queenNeedsReturn =
                waitingForQueenCover ||
                queenPocketed ||
                wasLastQueenInProgress;

            if (queenNeedsReturn)
            {
                ReturnQueen();
            }

            waitingForQueenCover = false;
            queenCovered = false;
            queenPocketed = false;
            queenCoverExtraShotGranted = false;

            //--------------------------------------------------
            // RETURN DUE COINS
            //
            // Any normal coins pocketed during this fouled
            // stroke go back onto the board, and their points
            // are reverted.
            //--------------------------------------------------

            if (coinsPocketedThisShot.Count > 0)
            {
                Debug.Log(
                    "FOUL — Returning " +
                    coinsPocketedThisShot.Count +
                    " Due Coin(s) To Board"
                );

                foreach (GameObject coinObj in coinsPocketedThisShot)
                {
                    ReturnDueCoin(coinObj);
                }

                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.RevertPoints(
                        coinsPocketedThisShot.Count
                    );
                }
            }

            coinPocketed = false;

            ResetStriker();

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.NextPlayer();
            }

            if (ArePiecesStillOnBoard())
            {
                CurrentState = GameState.Positioning;
                return;
            }

            if (ScoreManager.Instance != null &&
                ScoreManager.Instance.IsGameOver())
            {
                CurrentState = GameState.GameOver;
                ShowWinner();
                return;
            }

            CurrentState = GameState.Positioning;
            return;
        }


        //--------------------------------------------------
        // LAST QUEEN — transition into the bonus re-pocket shot.
        //--------------------------------------------------

        if (lastQueenPendingExtraTurn)
        {
            lastQueenPendingExtraTurn = false;

            StartLastQueenExtraTurn();

            return;
        }


        //--------------------------------------------------
        // LAST QUEEN BONUS SHOT — resolve win or miss
        //--------------------------------------------------

        if (lastQueenExtraShot)
        {
            if (queenPocketed)
            {
                Debug.Log(
                    "Last Queen Pocketed Again — Game Over"
                );

                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.QueenScored();
                }

                lastQueenExtraShot = false;
                queenPocketed = false;

                CurrentState = GameState.GameOver;

                ShowWinner();

                return;
            }


            Debug.Log(
                "Last Queen Bonus Shot Missed"
            );

            lastQueenExtraShot = false;
            queenReturnedOnce = false;

            ResetStriker();

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.NextPlayer();
            }

            CurrentState = GameState.Positioning;

            return;
        }


        //--------------------------------------------------
        // NORMAL QUEEN — COVER RESOLUTION
        //--------------------------------------------------

        if (waitingForQueenCover)
        {
            if (queenCovered)
            {
                Debug.Log("Queen Covered Successfully");

                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.QueenScored();
                }

                waitingForQueenCover = false;
                queenCovered = false;
                queenPocketed = false;
                queenCoverExtraShotGranted = false;

                ResetStriker();

                if (coinPocketed)
                {
                    Debug.Log("Queen Covered + Coin Pocketed — Player Continues");
                }
                else
                {
                    Debug.LogWarning(
                        "Queen marked covered but no coin was recorded."
                    );
                }

                coinPocketed = false;

                if (ArePiecesStillOnBoard())
                {
                    CurrentState = GameState.Positioning;
                    return;
                }

                if (ScoreManager.Instance != null &&
                    ScoreManager.Instance.IsGameOver())
                {
                    CurrentState = GameState.GameOver;
                    ShowWinner();
                    return;
                }

                CurrentState = GameState.Positioning;
                return;
            }


            if (!queenCoverExtraShotGranted)
            {
                Debug.Log("Queen Pocketed — Bonus Shot To Cover");

                queenCoverExtraShotGranted = true;

                coinPocketed = false;

                ResetStriker();
                CurrentState = GameState.Positioning;

                return;
            }


            Debug.Log(
                "Queen Not Covered — Returning To Centre"
            );

            ReturnQueen();

            waitingForQueenCover = false;
            queenCovered = false;
            queenPocketed = false;
            queenCoverExtraShotGranted = false;
            coinPocketed = false;

            ResetStriker();

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.NextPlayer();
            }

            CurrentState = GameState.Positioning;

            return;
        }

        //--------------------------------------------------
        // NORMAL TURN
        //--------------------------------------------------

        ResetStriker();


        if (coinPocketed)
        {
            Debug.Log("Player Continues");
        }
        else
        {
            Debug.Log("Next Player");

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.NextPlayer();
            }
        }


        coinPocketed = false;


        //--------------------------------------------------
        // GAME OVER CHECK
        //--------------------------------------------------

        if (ArePiecesStillOnBoard())
        {
            Debug.Log(
                "Pieces are still on board — Game continues."
            );

            CurrentState = GameState.Positioning;

            return;
        }


        Debug.Log(
            "No pieces remain on board."
        );

        if (ScoreManager.Instance != null &&
            ScoreManager.Instance.IsGameOver())
        {
            CurrentState = GameState.GameOver;

            ShowWinner();

            return;
        }


        CurrentState = GameState.Positioning;
    }


    //--------------------------------------------------
    // RESET STRIKER
    //--------------------------------------------------

    void ResetStriker()
    {
        if (striker == null || strikerStart == null)
            return;

        Rigidbody2D rb =
            striker.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        striker.position = strikerStart.position;

        striker.gameObject.SetActive(true);

        strikerPlaced = false;
    }


    //--------------------------------------------------
    // RETURN QUEEN (failed cover / foul)
    //--------------------------------------------------

    void ReturnQueen()
    {
        if (queen == null || queenStartPosition == null)
            return;

        CoinPocketAnimation anim =
            queen.GetComponent<CoinPocketAnimation>();

        if (anim != null)
            anim.CancelPocketAnimation();

        Rigidbody2D rb =
            queen.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        queen.position =
            queenStartPosition.position;

        queen.gameObject.SetActive(true);

        Debug.Log(
            "Queen Returned To Centre"
        );
    }


    //--------------------------------------------------
    // RETURN A DUE COIN (foul only)
    //--------------------------------------------------

    void ReturnDueCoin(GameObject coinObj)
    {
        if (coinObj == null)
            return;

        CoinPocketAnimation anim =
            coinObj.GetComponent<CoinPocketAnimation>();

        if (anim != null)
            anim.CancelPocketAnimation();

        Rigidbody2D rb =
            coinObj.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Transform returnPoint =
            dueCoinReturnPoint != null
                ? dueCoinReturnPoint
                : queenStartPosition;

        Vector3 basePos =
            returnPoint != null
                ? returnPoint.position
                : Vector3.zero;

        Vector2 randomOffset =
            Random.insideUnitCircle * dueCoinScatterRadius;

        coinObj.transform.position =
            basePos + new Vector3(randomOffset.x, randomOffset.y, 0f);

        coinObj.gameObject.SetActive(true);

        Debug.Log(
            "Due Coin Returned To Board: " + coinObj.name
        );
    }


    //--------------------------------------------------
    // LAST QUEEN BONUS SHOT
    //--------------------------------------------------

    public void StartLastQueenExtraTurn()
    {
        Debug.Log(
            "Last Queen Bonus Shot Starting"
        );

        if (queen != null && queenStartPosition != null)
        {
            CoinPocketAnimation anim =
                queen.GetComponent<CoinPocketAnimation>();

            if (anim != null)
                anim.CancelPocketAnimation();

            Rigidbody2D queenRb =
                queen.GetComponent<Rigidbody2D>();

            if (queenRb != null)
            {
                queenRb.linearVelocity = Vector2.zero;
                queenRb.angularVelocity = 0f;
            }

            queen.position = queenStartPosition.position;
            queen.gameObject.SetActive(true);
        }

        lastQueenExtraShot = true;

        queenPocketed = false;

        ResetStriker();

        CurrentState = GameState.Positioning;
    }


    //--------------------------------------------------
    // SHOW WINNER
    //--------------------------------------------------

    void ShowWinner()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWin();
        }

        string winner;

        if (ScoreManager.Instance.player1Score >
            ScoreManager.Instance.player2Score)
        {
            winner = "Player 1 Wins!";
        }
        else if (ScoreManager.Instance.player2Score >
                 ScoreManager.Instance.player1Score)
        {
            winner = "Player 2 Wins!";
        }
        else
        {
            winner = "Draw!";
        }

        Debug.Log(winner);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(winner);
        }
    }
}