using UnityEngine;

enum DirectionType
{
    Vertical, Horizontal
}

public class MovingPlatform : Platform
{
    [SerializeField] DirectionType direction;
    [SerializeField] float platformSpeed = 2f;   // 이동 속도
    [SerializeField] float platformRange = 3f;   // 이동 거리 (왕복 기준)

    private Rigidbody2D rb;
    private Vector2 startPos;
    private Vector2 lastPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = rb.position;
        lastPosition = rb.position;
    }

    private void FixedUpdate()
    {
        // --- 플랫폼 이동 제어 ---
        Vector2 newPos = GetPlatformPosition();
        rb.MovePosition(newPos);

        // --- 이동량 기록 ---
        lastPosition = rb.position;
    }

    /// <summary>
    /// 시작 위치와 방향, 속도, 범위를 바탕으로 현재 플랫폼 위치를 계산합니다.
    /// </summary>
    private Vector2 GetPlatformPosition()
    {
        Vector2 pos = startPos;

        float offset = Mathf.PingPong(Time.time * platformSpeed, platformRange * 2) - platformRange;

        if (direction == DirectionType.Horizontal)
        {
            pos.x += offset;
        }
        else if (direction == DirectionType.Vertical)
        {
            pos.y += offset;
        }

        return pos;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var movement = collision.gameObject.GetComponent<characterMovement>();
            if (movement != null)
            {
                // 플랫폼 이동량 계산
                Vector2 delta = rb.position - lastPosition;

                // 플레이어가 입력 중이 아니면 플랫폼과 함께 이동
                if (!movement.pressingKey)
                {
                    collision.rigidbody.position += delta;
                }
            }
        }
    }
}
