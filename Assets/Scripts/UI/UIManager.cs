using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Score UI")]
    public TMP_Text player1ScoreText;
    public TMP_Text player2ScoreText;
    public TMP_Text currentTurnText;
    public TMP_Text remainingCoinsText;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text winnerText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Hide Game Over panel at game start
        gameOverPanel.SetActive(false);
    }

    public void UpdateScores(int p1, int p2)
    {
        player1ScoreText.text = "Player 1 : " + p1;
        player2ScoreText.text = "Player 2 : " + p2;
    }

    public void UpdateTurn(bool playerOneTurn)
    {
        currentTurnText.text = playerOneTurn ?
            "Turn : Player 1" :
            "Turn : Player 2";
    }

    public void UpdateRemainingCoins(int coins)
    {
        remainingCoinsText.text = "Coins Left : " + coins;
    }

    public void ShowGameOver(string winner)
    {
        winnerText.text = winner;
        gameOverPanel.SetActive(true);
    }

    public void HideGameOver()
    {
        gameOverPanel.SetActive(false);
    }
}