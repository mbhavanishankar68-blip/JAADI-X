using System;

namespace JaadiX.Core
{
    public static class GameEvents
    {
        // Match Events
        public static Action OnMatchStarted;
        public static Action OnMatchEnded;

        // Turn Events
        public static Action OnTurnStarted;
        public static Action OnTurnEnded;

        // Coin Events
        public static Action OnCoinPocketed;
        public static Action OnQueenPocketed;

        // UI Events
        public static Action OnScoreUpdated;

        // Scene Events
        public static Action<string> OnSceneLoaded;
    }
}