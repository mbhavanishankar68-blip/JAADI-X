using UnityEngine;

namespace JaadiX.Gameplay
{
    public class BoardManager : MonoBehaviour
    {
        [Header("Board References")]

        [SerializeField] private Transform strikerSpawn;
        [SerializeField] private Transform queenSpawn;
        [SerializeField] private Transform coinCenter;

        private void Awake()
        {
            Debug.Log("Board Initialized.");
        }

        public Transform GetStrikerSpawn()
        {
            return strikerSpawn;
        }

        public Transform GetQueenSpawn()
        {
            return queenSpawn;
        }

        public Transform GetCoinCenter()
        {
            return coinCenter;
        }
    }
}