using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakablePlatform : Platform
{
    [SerializeField] float destroyDelay = 1.0f;  // 사라지기 전 대기 시간
    [SerializeField] float reappearDelay = -1f;  // 원하면 일정 시간 뒤에 다시 나타나도록 (음수면 영구 삭제)

    private bool isBreaking = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isBreaking && collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(DestroyPlatform());
        }
    }

    IEnumerator DestroyPlatform()
    {
        isBreaking = true;

        // 1초 정도 대기 (플레이어가 밟은 후)
        yield return new WaitForSeconds(destroyDelay);

        // 플랫폼 비활성화
        gameObject.GetComponent<TilemapCollider2D>().enabled = false;
        gameObject.GetComponent<TilemapRenderer>().enabled = false;

        // reappearDelay가 0보다 크면 다시 나타남
        if (reappearDelay > 0f)
        {
            yield return new WaitForSeconds(reappearDelay);

            gameObject.GetComponent<TilemapCollider2D>().enabled = true;
            gameObject.GetComponent<TilemapRenderer>().enabled = true;
            isBreaking = false; // 다시 밟을 수 있음
        }
    }
}
