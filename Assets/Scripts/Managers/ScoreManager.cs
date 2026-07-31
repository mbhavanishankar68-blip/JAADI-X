using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int player1Score = 0;
    public int player2Score = 0;

    public int currentPlayer = 1;
    // Total remaining pieces (normal coins + queen)
    private int totalCoins;

    // Remaining normal coins only
    private int normalCoinsLeft;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Coin[] coins = FindObjectsByType<Coin>(FindObjectsSortMode.None);

        totalCoins = coins.Length;
        normalCoinsLeft = 0;

        foreach (Coin coin in coins)
        {
            if (!coin.CompareTag("Queen"))
                normalCoinsLeft++;
        }

        UIManager.Instance.UpdateScores(player1Score, player2Score);
        UIManager.Instance.UpdateTurn(true);
        UIManager.Instance.UpdateRemainingCoins(totalCoins);

        Debug.Log("Total Pieces : " + totalCoins);
        Debug.Log("Normal Coins : " + normalCoinsLeft);
    }

    // Pocket a normal coin
    public void AddPoint()
    {
        if (currentPlayer == 1)
            player1Score++;
        else
            player2Score++;

        if (totalCoins > 0)
            totalCoins--;

        if (normalCoinsLeft > 0)
            normalCoinsLeft--;

        UIManager.Instance.UpdateScores(player1Score, player2Score);
        UIManager.Instance.UpdateRemainingCoins(totalCoins);

        Debug.Log("Coins Left : " + totalCoins);
    }
    // Queen is finally removed from the game
    public void QueenScored()
    {
        if (totalCoins > 0)
            totalCoins--;

        UIManager.Instance.UpdateRemainingCoins(totalCoins);

        Debug.Log("Queen Removed");
        Debug.Log("Coins Left : " + totalCoins);
    }
    public int GetNormalCoinsLeft()
    {
        return normalCoinsLeft;
    }

    public int GetTotalCoinsLeft()
    {
        return totalCoins;
    }

    public void NextPlayer()
    {
        currentPlayer = (currentPlayer == 1) ? 2 : 1;

        UIManager.Instance.UpdateTurn(currentPlayer == 1);

        Debug.Log("Current Player : " + currentPlayer);
    }

    public bool IsGameOver()
    {
        return totalCoins <= 0;
    }
}