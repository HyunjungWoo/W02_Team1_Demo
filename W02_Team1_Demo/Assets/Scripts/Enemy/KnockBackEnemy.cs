using UnityEngine;

public class KnockBackEnemy : MonoBehaviour
{
    [SerializeField] private GameObject weapon;
    float knockbackForce = 5f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            KnockbackEnemyInBox(collision);
        }
    }
    public void KnockbackEnemyInBox(Collider2D enemyCollider)
    {
        Rigidbody enemyRigidbody = enemyCollider.GetComponent<Rigidbody>();
        if (enemyRigidbody != null)
        {
            // 4. 튕겨낼 방향을 계산합니다. (무기 위치에서 적 위치로의 방향 + 위쪽 방향)
            // 이렇게 하면 대각선 45도 위로 튕겨나가는 효과를 줄 수 있습니다.
            Vector3 direction = (enemyCollider.transform.position - weapon.transform.position).normalized;
            Vector3 knockbackDirection = (direction + Vector3.up).normalized;

            // 5. 적에게 순간적인 힘(Impulse)을 가해 튕겨냅니다. 🚀
            enemyRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }
    }
}
