using UnityEngine;
using System.Collections;

/// <summary>
/// 돌진 공격: 잠깐 멈췄다가(Windup) 좌우 임펄스로 돌진.
/// 벽 충돌 시 스턴 같은 추가 연출은 OnCollisionEnter2D에서 확장 가능.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class AttackDash : MonoBehaviour
{
    [Header("타이밍/파워")]
    public float windup = 0.12f;   // 예열
    public float impulse = 8f;     // 좌우 임펄스 크기
    public float active = 0.18f;   // 돌진 유지 시간(물리 관성)
    public float cooldown = 0.6f;  // 쿨타임
    public float range = 3.0f;     // 돌진 개시 거리

    Rigidbody2D rb;
    public bool IsDashing { get; private set; }
    bool onCooldown;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    public bool IsReady => !IsDashing && !onCooldown;

    public bool InRange(Transform self, Transform target)
        => target && Vector2.Distance(self.position, target.position) <= range;

    /// <summary>외부(Brain)에서 호출: 돌진 개시</summary>
    public void StartDash(Transform self, Transform target, MonoBehaviour runner)
    {
        if (!IsReady || !target) return;
        runner.StartCoroutine(DashCo(self, target));
    }

    IEnumerator DashCo(Transform self, Transform target)
    {
        IsDashing = true; onCooldown = true;

        // 예열: 수평 속도 0으로 멈춤
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        yield return new WaitForSeconds(windup);

        // 좌우 방향 결정 후 임펄스 가하기
        int dir = (target.position.x >= self.position.x) ? 1 : -1;
        rb.AddForce(new Vector2(dir * impulse, 0f), ForceMode2D.Impulse);

        yield return new WaitForSeconds(active);

        IsDashing = false;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }
}
