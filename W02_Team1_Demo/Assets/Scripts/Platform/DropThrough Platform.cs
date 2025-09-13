using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlatformEffector2D), typeof(CompositeCollider2D))]
public class DropThroughPlatform : Platform
{
    private PlatformEffector2D effector;
    [SerializeField] private float disableDuration = 0.3f; // 얼마 동안 아래로 통과 가능하게 할지
    private bool isDropping = false;

    private void Awake()
    {
        effector = GetComponent<PlatformEffector2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDropping) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                StartCoroutine(DropThrough());
            }
        }
    }

    private IEnumerator DropThrough()
    {
        isDropping = true;

        // Effector를 180도 회전시켜서 아래쪽 통과 가능하게
        effector.rotationalOffset = 180f;

        yield return new WaitForSeconds(disableDuration);

        // 다시 위쪽에서만 충돌되도록 복구
        effector.rotationalOffset = 0f;

        isDropping = false;
    }
}
