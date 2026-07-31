using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;

    public int rows = 5;
    public float spacing = 0.4f;

    void Start()
    {
        SpawnCoins();
    }

    void SpawnCoins()
    {
        Vector2 center = Vector2.zero;

        for (int y = -2; y <= 2; y++)
        {
            for (int x = -2; x <= 2; x++)
            {
                Vector2 position = center + new Vector2(x * spacing, y * spacing);

                Instantiate(coinPrefab, position, Quaternion.identity);
            }
        }
    }
}