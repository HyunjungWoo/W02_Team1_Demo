using UnityEngine;
using UnityEngine.Tilemaps;

public class KeyItem : Item
{
    [SerializeField] Tilemap LockTilemap;
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.CompareTag("Player"))
        {
            
            Destroy(gameObject);
            
            Destroy(LockTilemap.gameObject);
        }
    }

}
