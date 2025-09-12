using UnityEngine;

public class NonStuckPlatform : Platform
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Kunai"))
        {
            Destroy(collision.gameObject);
        }
    }
}
