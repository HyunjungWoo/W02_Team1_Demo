using UnityEngine;
using System.Collections;

/// <summary>
/// 근접 공격: 히트박스(Trigger 콜라이더)를 잠깐 켰다가 끄는 방식.
/// 데미지 처리(충돌 시 피해)는 히트박스 쪽에서 별도로 구현 가능.
/// </summary>
public class AttackMelee : MonoBehaviour
{
    [Header("히트박스(Trigger 콜라이더)")]
    public Collider2D hitbox;

    [Header("타이밍")]
    public float windup = 0.1f;   // 예열(텔레그래프)
    public float active = 0.2f;   // 히트박스가 켜져 있는 시간
    public float cooldown = 0.5f; // 쿨타임
    public float range = 1.2f;    // 공격 가능 거리

    [Tooltip("공격 애니메이션을 재생할 Animator (비워두면 자동 탐색)")]
    public Animator animator;
    [Tooltip("공격 시작 시 SetTrigger로 발사할 파라미터 이름")]
    public string attackTriggerName = "AttackOn";
    [Tooltip("공격 종료 시 초기화할(선택) 트리거 이름. 보통은 비워둬도 됨")]
    public string attackResetTriggerName = "";

    public bool IsAttacking { get; private set; }
    bool onCooldown;

    /// <summary>지금 공격 시작 가능한가?</summary>
    public bool IsReady => !IsAttacking && !onCooldown;

    /// <summary>타깃이 사거리 안인가?</summary>
    public bool InRange(Transform self, Transform target)
        => target && Vector2.Distance(self.position, target.position) <= range;

    /// <summary>외부(Brain)에서 호출: 공격 개시</summary>
    public void StartAttack(MonoBehaviour runner)
    {
        if (!IsReady) return;
        runner.StartCoroutine(AttackCo());
    }

    void Awake()
    {
        // Animator 자동 탐색(비워두면)
        if (!animator)
        {
            // 부모에 EnemyContext가 있으면 그 애니메이터를 우선 사용
            var ctx = GetComponentInParent<EnemyContext>();
            if (ctx && ctx.animator) animator = ctx.animator;
            else animator = GetComponentInChildren<Animator>();
        }

        // 히트박스는 기본적으로 꺼두기(안전)
        if (hitbox) hitbox.enabled = false;
    }

        IEnumerator AttackCo()
    {
        IsAttacking = true;
        onCooldown = true;

        if (hitbox) hitbox.enabled = false;   // 예열 동안은 꺼둠
        yield return new WaitForSeconds(windup);

        if (hitbox) hitbox.enabled = true;    // 유효 시간에만 켜기
        yield return new WaitForSeconds(active);

        if (hitbox) hitbox.enabled = false;   // 다시 끄기

        IsAttacking = false;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }
}