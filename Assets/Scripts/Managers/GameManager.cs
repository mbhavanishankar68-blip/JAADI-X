using UnityEngine;

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

    // Queen has been pocketed, pending resolution this turn
    public bool queenPocketed = false;

    // True while we're in the "cover the queen" resolution window
    public bool waitingForQueenCover = false;

    // True once the queen has been successfully covered
    public bool queenCovered = false;

    // True once the one-time bonus "cover" shot has already been
    // granted for the CURRENT queen-cover cycle
    public bool queenCoverExtraShotGranted = false;

    //--------------------------------------------------
    // LAST QUEEN (DISC POOL)
    //--------------------------------------------------

    // True while the player is taking the bonus shot to re-pocket
    // the last queen after it bounced back to centre
    public bool lastQueenExtraShot = false;

    // True once the last queen has been returned to centre this cycle
    public bool queenReturnedOnce = false;

    // Set by PocketScript the instant the LAST queen is pocketed.
    // GameManager only acts on this inside EndTurn(), once physics
    // has actually settled — never directly inside a trigger callback.
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

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    //--------------------------------------------------

    void Start()
    {
        CurrentState = GameState.Positioning;

        strikerPlaced = false;

        coins = FindObjectsByType<Coin>(FindObjectsSortMode.None);

        Debug.Log("Game Started");
    }

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

    bool AllCoinsStopped()
    {
        Rigidbody2D strikerRb = striker.GetComponent<Rigidbody2D>();

        if (strikerRb.linearVelocity.magnitude > 0.05f)
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

    void EndTurn()
    {
        Debug.Log("Turn Finished");

        //--------------------------------------------------
        // LAST QUEEN — transition into the bonus re-pocket shot.
        // Deferred here so it only happens once everything has
        // actually stopped moving.
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
                Debug.Log("Last Queen Pocketed Again — Game Over");

                ScoreManager.Instance.QueenScored();

                lastQueenExtraShot = false;
                queenPocketed = false;

                CurrentState = GameState.GameOver;
                ShowWinner();

                return;
            }

            Debug.Log("Last Queen Bonus Shot Missed");

            lastQueenExtraShot = false;
            queenReturnedOnce = false;

            ResetStriker();
            ScoreManager.Instance.NextPlayer();

            CurrentState = GameState.Positioning;

            return;
        }

        //--------------------------------------------------
        // NORMAL QUEEN — cover resolution (may span 1–2 shots)
        //--------------------------------------------------
        if (waitingForQueenCover)
        {
            if (queenCovered)
            {
                Debug.Log("Queen Covered Successfully");

                ScoreManager.Instance.QueenScored();

                waitingForQueenCover = false;
                queenCovered = false;
                queenPocketed = false;
                queenCoverExtraShotGranted = false;

                // fall through — a coin WAS pocketed to earn this
                // cover, so coinPocketed correctly reflects that below.
            }
            else if (!queenCoverExtraShotGranted)
            {
                Debug.Log("Queen Pocketed — Bonus Shot To Cover");

                queenCoverExtraShotGranted = true;
                coinPocketed = false;

                ResetStriker();
                CurrentState = GameState.Positioning;

                return;
            }
            else
            {
                Debug.Log("Queen Not Covered — Returned To Centre");

                ReturnQueen();

                waitingForQueenCover = false;
                queenCovered = false;
                queenPocketed = false;
                queenCoverExtraShotGranted = false;
                coinPocketed = false;

                ResetStriker();
                ScoreManager.Instance.NextPlayer();

                CurrentState = GameState.Positioning;

                return;
            }
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
            ScoreManager.Instance.NextPlayer();
        }

        coinPocketed = false;

        //--------------------------------------------------
        // GAME OVER
        //--------------------------------------------------

        if (ScoreManager.Instance.IsGameOver())
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
        Rigidbody2D rb = striker.GetComponent<Rigidbody2D>();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

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

        // Stop any in-flight sink animation so it can't fight this reset
        CoinPocketAnimation anim = queen.GetComponent<CoinPocketAnimation>();
        if (anim != null)
            anim.CancelPocketAnimation();

        Rigidbody2D rb = queen.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        queen.position = queenStartPosition.position;
        queen.gameObject.SetActive(true);

        Debug.Log("Queen Returned To Centre");
    }

    //--------------------------------------------------
    // DISC POOL - LAST QUEEN BONUS SHOT
    //--------------------------------------------------

    public void StartLastQueenExtraTurn()
    {
        Debug.Log("Last Queen Bonus Shot Starting");

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
        AudioManager.Instance.PlayWin();

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

        UIManager.Instance.ShowGameOver(winner);
    }
}