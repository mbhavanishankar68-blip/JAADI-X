using UnityEngine;

namespace JaadiX.Coins
{
    [CreateAssetMenu(fileName = "CoinData", menuName = "Jaadi X/Coins/Coin Data")]
    public class CoinData : ScriptableObject
    {
        [Header("Identity")]
        public CoinType CoinType;

        [Header("Score")]
        public int Score;

        [Header("Appearance")]
        public Sprite Sprite;

        [Header("Physics")]
        public float Mass = 1f;

        public float Drag = 0.5f;

        public float AngularDrag = 0.5f;
    }
}