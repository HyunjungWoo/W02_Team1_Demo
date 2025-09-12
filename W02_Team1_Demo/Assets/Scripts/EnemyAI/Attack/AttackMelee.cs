using UnityEngine;
using System.Collections;

public class AttackMelee : MonoBehaviour
{
    [Header("히트박스(Trigger 콜라이더)")]
    public Collider2D hitbox;
    public float hitboxOffset = 0.5f;

    [Header("타이밍")]
    public float windup = 0.1f;
    public float active = 0.2f;
    public float cooldown = 0.5f;
    public float range = 1.2f;

    [Tooltip("몬스터의 기본 Animator. (비워두면 자동 탐색)")]
    public Animator animator; // 몬스터 본체의 Animator
    [Tooltip("공격 애니메이션이 붙어있는 스프라이트 렌더러 (비워두면 자동 탐색)")]
    public SpriteRenderer attackSpriteRenderer; // [추가] 공격 애니메이션 스프라이트 렌더러
    [Tooltip("공격 시작 시 SetTrigger로 발사할 파라미터 이름")]
    public string attackTriggerName = "AttackOn";
    [Tooltip("공격 종료 시 초기화할(선택) 트리거 이름. 보통은 비워둬도 됨")]
    public string attackResetTriggerName = "";

    public bool IsAttacking { get; private set; }
    bool onCooldown;

    // 몬스터 본체의 SpriteRenderer 참조 (방향을 가져오기 위함)
    private SpriteRenderer mainSpriteRenderer; // [추가] 몬스터 본체 스프라이트 렌더러

    public bool IsReady => !IsAttacking && !onCooldown;
    public bool InRange(Transform self, Transform target)
        => target && Vector2.Distance(self.position, target.position) <= range;

    public void StartAttack(MonoBehaviour runner)
    {
        if (!IsReady) return;
        runner.StartCoroutine(AttackCo());
    }

    void Awake()
    {
        // 몬스터 본체의 Animator 탐색
        if (!animator)
        {
            var ctx = GetComponentInParent<EnemyContext>();
            if (ctx && ctx.animator) animator = ctx.animator;
            else animator = GetComponentInChildren<Animator>();
        }

        // 몬스터 본체의 SpriteRenderer 탐색 (방향 전환 정보를 가져오기 위함)
        mainSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // [추가] 공격 애니메이션 스프라이트 렌더러 탐색
        if (!attackSpriteRenderer)
        {
            // AttackMelee 스크립트가 붙은 오브젝트의 자식 중 SpriteRenderer를 찾음
            // (혹은 AttackMelee 스크립트가 붙은 오브젝트 자체에 있으면 그걸 사용)
            attackSpriteRenderer = GetComponent<SpriteRenderer>();
            if (attackSpriteRenderer == null)
            {
                // 자식 오브젝트에서 탐색 (예: "AttackVisual" 같은 이름의 자식)
                // 특정 이름을 가진 자식을 찾거나, 그냥 첫 번째 자식 SpriteRenderer를 찾을 수 있습니다.
                // 여기서는 첫 번째 자식 SpriteRenderer를 찾습니다. 필요시 수정해주세요.
                attackSpriteRenderer = transform.GetComponentInChildren<SpriteRenderer>(); 
            }
        }
        
        // 히트박스는 기본적으로 꺼두기(안전)
        if (hitbox) hitbox.enabled = false;
        else Debug.LogWarning($"{gameObject.name}의 AttackMelee에 히트박스가 할당되지 않았습니다!");
    }

    IEnumerator AttackCo()
    {
        IsAttacking = true;
        onCooldown = true;

        if (animator && !string.IsNullOrEmpty(attackTriggerName))
        {
            animator.SetTrigger(attackTriggerName);
        }

        if (hitbox) hitbox.enabled = false;
        if (attackSpriteRenderer) attackSpriteRenderer.enabled = false; // [추가] 예열 동안 공격 스프라이트 끄기

        yield return new WaitForSeconds(windup);

        // [수정] 공격 스프라이트 및 히트박스 방향 설정
        UpdateAttackVisualsFacingAndPosition(); 
        
        if (hitbox) hitbox.enabled = true;
        if (attackSpriteRenderer) attackSpriteRenderer.enabled = true; // [추가] 공격 스프라이트 켜기

        yield return new WaitForSeconds(active);

        if (hitbox) hitbox.enabled = false;
        if (attackSpriteRenderer) attackSpriteRenderer.enabled = false; // [추가] 공격 스프라이트 끄기

        IsAttacking = false;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    /// <summary>
    /// 몬스터의 현재 방향에 맞춰 공격 히트박스와 스프라이트의 위치/방향을 조절합니다.
    /// </summary>
    void UpdateAttackVisualsFacingAndPosition()
    {
        if (mainSpriteRenderer == null) return; // 본체 스프라이트가 없으면 방향 정보를 알 수 없음

        // 몬스터 본체 스프라이트의 FlipX 상태를 가져옴
        bool isFlipped = mainSpriteRenderer.flipX;
        float directionMultiplier = isFlipped ? -1f : 1f;

        // 1. 공격 애니메이션 스프라이트 반전 및 위치 조정
        if (attackSpriteRenderer != null)
        {
            // 스프라이트 자체의 flipX를 true/false로 설정
            attackSpriteRenderer.flipX = isFlipped;

            // ★★★ 가장 중요한 부분 ★★★
            // 공격 스프라이트가 붙어있는 '게임 오브젝트'의 로컬 위치를 변경합니다.
            // X 위치를 |원래 X 위치| * 방향으로 설정하여 항상 몸 바깥쪽을 향하게 합니다.
            float newX = Mathf.Abs(attackSpriteRenderer.transform.localPosition.x) * directionMultiplier;
            attackSpriteRenderer.transform.localPosition = new Vector3(newX, attackSpriteRenderer.transform.localPosition.y, attackSpriteRenderer.transform.localPosition.z);
        }

        // 2. 히트박스 위치 조절
        if (hitbox != null)
        {
            // 콜라이더의 offset을 방향에 맞게 조절
            hitbox.offset = new Vector2(hitboxOffset * directionMultiplier, hitbox.offset.y);
        }
    }
}