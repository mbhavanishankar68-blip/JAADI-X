using UnityEngine;

public class Coin : MonoBehaviour
{
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public bool IsMoving()
    {
        return rb.linearVelocity.magnitude > 0.05f;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Striker"))
        {
            AudioManager.Instance.PlayStrikerHit();
        }
        else if (collision.gameObject.CompareTag("Coin") ||
                 collision.gameObject.CompareTag("Queen"))
        {
            AudioManager.Instance.PlayCoinHit();
        }
    }
}
