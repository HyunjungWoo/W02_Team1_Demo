using UnityEngine;

public enum ReflectionSurface
{
    Floor,
    Ceiling,
    LeftWall,
    RightWall
}

public class ReflectionPlatform : Platform
{
    [SerializeField] private ReflectionSurface surfaceType = ReflectionSurface.Floor;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Kunai"))
        {
            Rigidbody2D rb = collider.attachedRigidbody;
            if (rb != null)
            {
                // 플랫폼 방향에 따라 노멀 벡터 지정
                Vector2 normal = Vector2.up;
                switch (surfaceType)
                {
                    case ReflectionSurface.Floor:
                        normal = Vector2.up;
                        break;
                    case ReflectionSurface.Ceiling:
                        normal = Vector2.down;
                        break;
                    case ReflectionSurface.LeftWall:
                        normal = Vector2.right;
                        break;
                    case ReflectionSurface.RightWall:
                        normal = Vector2.left;
                        break;
                }

                // 현재 속도
                Vector2 incomingVelocity = rb.linearVelocity;

                // 반사된 속도 계산
                Vector2 reflectedVelocity = Vector2.Reflect(incomingVelocity, normal);

                // 속도 교체
                rb.linearVelocity = reflectedVelocity;

                // 디버그 표시
                Debug.DrawRay(collider.transform.position, reflectedVelocity, Color.red, 1f);
            }
        }
    }
}
