using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private int stage;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            InGameManager.Instance.TouchCheckPoint(transform.position);
        }
    }
}
