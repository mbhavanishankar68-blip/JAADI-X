using System;
using UnityEngine;

public static class GameEvents
{
    // Coin Events
    public static Action<Coin> OnCoinPocketed;
    public static Action<Coin> OnCoinSpawned;

    // Queen Events
    public static Action OnQueenPocketed;
    public static Action OnQueenReturned;

    // Turn Events
    public static Action<int> OnTurnChanged;

    // Match Events
    public static Action OnMatchStarted;
    public static Action OnMatchEnded;

    // Striker Events
    public static Action OnShotStarted;
    public static Action OnShotFinished;

    // Physics Events
    public static Action OnAllCoinsStopped;
}