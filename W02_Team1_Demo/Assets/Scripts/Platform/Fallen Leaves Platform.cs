using UnityEngine;

public class FallenLeavesPlatform : Platform
{ 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Kunai"))
            Destroy(gameObject);
    }
}
