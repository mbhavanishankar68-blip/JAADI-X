using UnityEngine;
using JaadiX.Coins;

namespace JaadiX.Coins
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Coin : MonoBehaviour
    {
        [Header("Coin Data")]
        [SerializeField] private CoinData coinData;

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;

        public CoinType CoinType => coinData.CoinType;
        public int Score => coinData.Score;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();

            ApplyCoinData();
        }

        private void ApplyCoinData()
        {
            if (coinData == null)
            {
                Debug.LogWarning($"{name}: CoinData is not assigned.");
                return;
            }

            spriteRenderer.sprite = coinData.Sprite;

            rb.mass = coinData.Mass;
            rb.linearDamping = coinData.Drag;
            rb.angularDamping = coinData.AngularDrag;
        }

        public void SetCoinData(CoinData data)
        {
            coinData = data;
            ApplyCoinData();
        }
        public bool IsMoving()
        {
            if (rb == null)
                return false;

            return rb.linearVelocity.magnitude > 0.05f;
        }
    }
}