using UnityEngine;

public class EnemyHomeRun : MonoBehaviour
{
    private float knockbackForce = 30f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeadEnemy"))
        {
            KnockbackEnemy(collision);
        }
    }

    public void KnockbackEnemy(Collider2D enemyCollider)
    {
        Rigidbody2D enemyRigidbody = enemyCollider.GetComponent<Rigidbody2D>();
        if (enemyRigidbody != null)
        {
            Vector2 direction = (enemyCollider.transform.position - transform.position).normalized;
            direction.y += 0.1f;
            Vector2 knockbackDirection = (direction + Vector2.up).normalized;
            enemyRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }
}
