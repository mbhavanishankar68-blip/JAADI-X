using UnityEngine;

public class InnerPocketTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("INNER TRIGGER ENTERED BY: " + other.name);
    }
}