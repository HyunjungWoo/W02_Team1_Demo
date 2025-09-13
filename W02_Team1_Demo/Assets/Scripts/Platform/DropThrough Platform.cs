using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlatformEffector2D), typeof(Collider2D))]
public class DropThroughPlatform : Platform
{
    private PlatformEffector2D effector;
    [SerializeField] private float disableDuration = 0.3f; // 얼마 동안 충돌을 끌지
    private bool isDropping = false;

    private void Awake()
    {
        effector = GetComponent<PlatformEffector2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDropping) return;

        // 플레이어와 충돌 중일 때만 작동
        if (collision.gameObject.CompareTag("Player"))
        {
            // 아래키 입력 감지
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                StartCoroutine(DisableCollisionTemporarily(collision.collider));
            }
        }
    }

    private IEnumerator DisableCollisionTemporarily(Collider2D playerCollider)
    {
        isDropping = true;

        // 플레이어와 이 플랫폼의 충돌을 무시
        Physics2D.IgnoreCollision(playerCollider, GetComponent<Collider2D>(), true);

        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(disableDuration);

        // 충돌 다시 활성화
        Physics2D.IgnoreCollision(playerCollider, GetComponent<Collider2D>(), false);

        isDropping = false;
    }
}
