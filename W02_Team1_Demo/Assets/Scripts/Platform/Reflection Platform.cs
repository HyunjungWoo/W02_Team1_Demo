using UnityEngine;

public class ReflectionPlatform : Platform
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // "Kunai" 태그를 가진 오브젝트만 반사
        if (collision.gameObject.CompareTag("Player"))
        {
            
            Rigidbody2D rb = collision.rigidbody;
            if (rb != null)
            {
                // 충돌 지점의 법선 벡터
                Vector2 normal = collision.contacts[0].normal;

                // 현재 속도
                Vector2 incomingVelocity = rb.linearVelocity;

                // 반사된 속도 계산
                Vector2 reflectedVelocity = Vector2.Reflect(incomingVelocity, normal);

                // 속도 교체
                rb.linearVelocity = reflectedVelocity;

                Debug.DrawRay(collision.contacts[0].point, reflectedVelocity, Color.red, 1f);
            }
        }
    }
}
