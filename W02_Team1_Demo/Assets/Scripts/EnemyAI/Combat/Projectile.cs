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

    /// <summary>생성 직후 호출: 진행 방향 세팅</summary>
    public void Init(Vector2 direction)
    {
        dir = direction.normalized;
        Destroy(gameObject, life); // 일정 시간 뒤 자동 삭제
    }

    void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // 지정한 레이어에만 반응
        if (((1 << col.gameObject.layer) & hitMask) != 0)
        {
            // TODO: 대상의 체력 컴포넌트 찾아 damage 적용
            Destroy(gameObject);
        }
    }
}