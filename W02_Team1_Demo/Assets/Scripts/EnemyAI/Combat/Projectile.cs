using UnityEngine;

/// <summary>
/// 아주 단순한 투사체: 직선 이동 → 충돌 시 파괴.
/// 실제 데미지 처리는 충돌 대상의 체력 컴포넌트에 맞춰 확장하면 됨.
/// </summary>
public class Projectile : MonoBehaviour
{
    public float speed = 10f;      // 속도
    public float life = 3f;        // 자동 파괴 시간
    public int damage = 1;         // 피해량(확장용)
    public LayerMask hitMask;      // 맞을 수 있는 레이어(예: Player)

    Vector2 dir;

    private Rigidbody2D rb;

    /// <summary>생성 직후 호출: 진행 방향 세팅</summary>
    public void Init(Vector2 direction)
    {
        dir = direction.normalized;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.Log("리즈드바디 넣으세요");
        }
        Destroy(gameObject, life); // 일정 시간 뒤 자동 삭제
    }

    void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);

        // gameobject 날라가는 direction방향으로 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // 충돌한 객체의 태그가 "Player"가 아니면 함수를 즉시 종료
        if (!col.CompareTag("Player"))
        {
            return;
        }
        Destroy(gameObject);
    }
}