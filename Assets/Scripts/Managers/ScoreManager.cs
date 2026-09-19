    using UnityEngine;
    using JaadiX.Core;
    using JaadiX.Coins;

    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance;

        public int player1Score = 0;
        public int player2Score = 0;

        public int currentPlayer = 1;

        // Total remaining pieces
        private int totalCoins;

        // Remaining normal coins
        private int normalCoinsLeft;

        private bool initialized = false;

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

        // Called by GameManager AFTER Gameplay is loaded
        public void InitializeScore()
        {
            Coin[] coins = FindObjectsByType<Coin>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

            totalCoins = coins.Length;
            normalCoinsLeft = 0;

            foreach (Coin coin in coins)
            {
                if (!coin.CompareTag("Queen"))
                {
                    normalCoinsLeft++;
                }
            }

            initialized = true;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScores(
                    player1Score,
                    player2Score
                );

                UIManager.Instance.UpdateTurn(
                    currentPlayer == 1
                );

                UIManager.Instance.UpdateRemainingCoins(
                    totalCoins
                );
            }

            Debug.Log("==============================");
            Debug.Log("SCORE MANAGER INITIALIZED");
            Debug.Log("Total Pieces : " + totalCoins);
            Debug.Log("Normal Coins : " + normalCoinsLeft);
            Debug.Log("==============================");
        }

        // Normal coin pocketed
        public void AddPoint()
        {
            if (!initialized)
            {
                Debug.LogWarning(
                    "ScoreManager is not initialized!"
                );

                return;
            }

            if (currentPlayer == 1)
            {
                player1Score++;
            }
            else
            {
                player2Score++;
            }

            if (totalCoins > 0)
            {
                totalCoins--;
            }

            if (normalCoinsLeft > 0)
            {
                normalCoinsLeft--;
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScores(
                    player1Score,
                    player2Score
                );

                UIManager.Instance.UpdateRemainingCoins(
                    totalCoins
                );
            }

            Debug.Log(
                "Normal Coins Left : " +
                normalCoinsLeft
            );

            Debug.Log(
                "Total Pieces Left : " +
                totalCoins
            );
        }

        // Queen is finally removed from the game
        public void QueenScored()
        {
            if (!initialized)
            {
                Debug.LogWarning(
                    "ScoreManager is not initialized!"
                );

                return;
            }

            if (totalCoins > 0)
            {
                totalCoins--;
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateRemainingCoins(
                    totalCoins
                );
            }

            Debug.Log("Queen Removed");

            Debug.Log(
                "Total Pieces Left : " +
                totalCoins
            );
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
            currentPlayer =
                (currentPlayer == 1) ? 2 : 1;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateTurn(
                    currentPlayer == 1
                );
            }

            Debug.Log(
                "Current Player : " +
                currentPlayer
            );
        }
        public void RevertPoints(int count)
        {
            if (!initialized)
            {
                Debug.LogWarning(
                    "ScoreManager is not initialized!"
                );

                return;
            }

            if (count <= 0)
                return;

            if (currentPlayer == 1)
            {
                player1Score =
                    Mathf.Max(0, player1Score - count);
            }
            else
            {
                player2Score =
                    Mathf.Max(0, player2Score - count);
            }

            totalCoins += count;
            normalCoinsLeft += count;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScores(
                    player1Score,
                    player2Score
                );

                UIManager.Instance.UpdateRemainingCoins(
                    totalCoins
                );
            }

            Debug.Log(
                "Reverted " +
                count +
                " point(s) due to foul. " +
                "Coins Left : " +
                totalCoins
            );
        }
        public bool IsGameOver()
        {
            return initialized && totalCoins <= 0;
        }
    }