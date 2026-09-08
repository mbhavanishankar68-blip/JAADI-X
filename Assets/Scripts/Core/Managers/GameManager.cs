using UnityEngine;
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

    // True once the one-time bonus "cover" shot has already
    // been granted for the CURRENT queen-cover cycle
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
    // END TURN
    //--------------------------------------------------

    void EndTurn()
    {
        Debug.Log("Turn Finished");


        //--------------------------------------------------
        // LAST QUEEN — transition into the bonus re-pocket shot.
        // Deferred here so it only happens once everything —
        // including the striker — has actually stopped moving.
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
        //
        // Official rule: the queen must be covered either in
        // the SAME stroke, or the IMMEDIATELY FOLLOWING stroke.
        // If both fail, it is respotted and the turn passes.
        //--------------------------------------------------

        if (waitingForQueenCover)
        {
            //--------------------------------------------------
            // COVERED (same shot, either order)
            //--------------------------------------------------
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


            //--------------------------------------------------
            // NOT covered yet — grant the ONE bonus shot
            //--------------------------------------------------
            if (!queenCoverExtraShotGranted)
            {
                Debug.Log("Queen Pocketed — Bonus Shot To Cover");

                queenCoverExtraShotGranted = true;

                // This next shot's own outcome hasn't happened yet
                coinPocketed = false;

                ResetStriker();
                CurrentState = GameState.Positioning;

                return;
            }


            //--------------------------------------------------
            // Bonus shot already used and STILL not covered
            //--------------------------------------------------
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

        strikerPlaced = false;
    }


    //--------------------------------------------------
    // RETURN QUEEN (failed cover)
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
    // LAST QUEEN BONUS SHOT
    //--------------------------------------------------
    //
    // This is the ONLY place the last-queen's position is
    // ever touched. It runs from EndTurn() -> only after
    // AllCoinsStopped() has confirmed the striker and every
    // coin are fully at rest.
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