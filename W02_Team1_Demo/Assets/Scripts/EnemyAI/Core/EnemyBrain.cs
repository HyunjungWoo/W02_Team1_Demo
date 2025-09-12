using UnityEngine;

/// <summary>
/// 적 AI의 상태를 결정(FSM)하고 모듈을 호출하는 브레인.
/// 상태: Idle / Patrol / Chase / Attack / Wait
/// - 이동: A* 지상(Movement2DGroundAstar) 우선 → (옵션) 공중
/// - 공격: 근접/돌진/원거리 중 가능한 것 우선
/// - Animator Bool: isMove / isAttack / isWait 세팅 (공격 트리거는 AttackMelee에서 발사)
/// </summary>
[RequireComponent(typeof(EnemyContext))]
public class EnemyBrain : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack, Wait }

    [Header("센서/경로")]
    public PlayerDetector2D detector;    // 플레이어 감지(반경+시야)
    public PatrolPath patrolPath;        // 순찰 경로(없으면 Idle)

    [Header("이동 모듈")]
    public Movement2DGroundAstar moveAStar; // ★ A* 지상 이동(필수)
    public Movement2DFly moveFly;           // (옵션) 공중 이동 모듈

    [Header("공격 모듈(붙인 것만 사용)")]
    public AttackMelee melee;   // 근접 — 내부에서 AttackOn 트리거 쏨
    public AttackDash dash;    // 돌진
    public AttackRanged ranged;  // 원거리

    [Header("추격 히스테리시스")]
    public float chaseStart = 6f; // 이 이하면 추격 시작
    public float chaseStop = 9f; // 이 이상이면 추격 해제(시작보다 크게)

    [Header("공격 후 대기")]
    public float waitAfterAttack = 0.30f;

    // 애니메이터 상태 플래그(선택: 컨트롤러에 동일 bool 있으면 연결해서 사용)
    public bool isMove, isAttack, isWait;

    // 내부
    private EnemyContext ctx;
    public State state = State.Idle;
    private int patrolIndex = 0;
    private float waitUntil = 0f;

    void Awake()
    {
        ctx = GetComponent<EnemyContext>();
        if (!detector) detector = GetComponent<PlayerDetector2D>(); // 있으면 자동 참조
    }

    void Update()
    {
        // 1) 상태 결정
        UpdateStateMachine();

        // 2) 상태 실행
        switch (state)
        {
            case State.Idle: DoIdle(); break;
            case State.Patrol: DoPatrol(); break;
            case State.Chase: DoChase(); break;
            case State.Attack: DoAttack(); break;
            case State.Wait: DoWait(); break;
        }

        // 3) 애니메이터 bool 동기화(있을 때만)
        if (ctx.animator)
        {
            ctx.animator.SetBool("isMove", isMove);
            ctx.animator.SetBool("isAttack", isAttack);
            ctx.animator.SetBool("isWait", isWait);
        }

        // 4) 좌우 바라보기
        UpdateFacing();
    }

    // ---------- FSM: 상태 결정 ----------
    void UpdateStateMachine()
    {
        var t = ctx.target;
        float dist = t ? Vector2.Distance(ctx.rb.position, t.position) : Mathf.Infinity;

        // Wait은 예정 시간까지 유지
        if (state == State.Wait && Time.time < waitUntil) return;

        // 공격 모듈이 이미 실행 중이면 공격 상태 유지
        bool attacking = (melee && melee.IsAttacking) || (dash && dash.IsDashing);
        if (attacking) { state = State.Attack; return; }

        // 공격 기회가 있으면 우선 공격으로 전환
        if (t)
        {
            if (melee && melee.IsReady && melee.InRange(transform, t)) { state = State.Attack; return; }
            if (dash && dash.IsReady && dash.InRange(transform, t)) { state = State.Attack; return; }
            if (ranged && ranged.IsReady && ranged.InRange(transform, t)) { state = State.Attack; return; }
        }

        // 추격 판단(센서 + 히스테리시스)
        bool see = detector ? detector.TryDetect(transform, out _, out _) : (dist <= chaseStart);
        bool keepChasing = (state == State.Chase) && (dist < chaseStop);

        if (see || keepChasing) { state = State.Chase; return; }

        // 순찰 경로가 있으면 Patrol, 없으면 Idle
        state = (patrolPath && patrolPath.waypoints != null && patrolPath.waypoints.Length > 0)
              ? State.Patrol : State.Idle;
    }

    // ---------- 상태 수행 ----------
    void DoIdle()
    {
        isMove = false; isAttack = false; isWait = false;
        StopAllMovement();
    }

    void DoPatrol()
    {
        isMove = true; isAttack = false; isWait = false;

        if (!patrolPath || patrolPath.waypoints == null || patrolPath.waypoints.Length == 0)
        {
            StopAllMovement(); return;
        }

        var wp = patrolPath.waypoints[patrolIndex];

        
        if (!wp) { StopAllMovement(); return; }

        if (moveAStar)
        {
            // 현재 웨이포인트로 길찾아 이동
            moveAStar.MoveTo(wp.position);

            // 도착 판정 → 잠깐 대기 후 다음 웨이포인트
            if (Vector2.Distance(transform.position, wp.position) <= patrolPath.arriveDist)
            {
                StartWait(patrolPath.waitAtPoint);
                patrolIndex = (patrolIndex + 1) % patrolPath.waypoints.Length;
            }
        }
        else if (moveFly)
        {
            moveFly.MoveTo(wp.position);
            if (Vector2.Distance(transform.position, wp.position) <= patrolPath.arriveDist)
            {
                StartWait(patrolPath.waitAtPoint);
                patrolIndex = (patrolIndex + 1) % patrolPath.waypoints.Length;
            }
        }
        else
        {
            // 이동 모듈 없으면 정지
            StopAllMovement();
        }
    }

    void DoChase()
    {
        isMove = true; isAttack = false; isWait = false;
        var t = ctx.target; if (!t) { StopAllMovement(); return; }

        if (moveAStar) moveAStar.MoveTo(t.position); // 타깃으로 추격
        else if (moveFly) moveFly.MoveTo(t.position);   // (공중 몹일 때)
        else StopAllMovement();
    }

    void DoAttack()
    {
        isMove = false; isAttack = true; isWait = false;

        var t = ctx.target; if (!t) { state = State.Idle; return; }

        // 이동 간섭 방지
        StopAllMovement();

        // 이미 진행 중인 공격이면 그대로 유지
        if ((melee && melee.IsAttacking) || (dash && dash.IsDashing)) return;

        // 가능한 공격부터 시도(우선순위는 취향대로)
        if (melee && melee.IsReady && melee.InRange(transform, t)) melee.StartAttack(this);
        else if (dash && dash.IsReady && dash.InRange(transform, t)) dash.StartDash(transform, t, this);
        else if (ranged && ranged.IsReady && ranged.InRange(transform, t)) ranged.Fire(transform, t);
        else
        {
            // 공격 불가면 이동 상태로 복귀
            state = (patrolPath && patrolPath.waypoints != null && patrolPath.waypoints.Length > 0)
                  ? State.Patrol : State.Chase;
            return;
        }

        // 공격 후 잠깐 대기(후딜)
        StartWait(waitAfterAttack);
    }

    void DoWait()
    {
        isMove = false; isAttack = false; isWait = true;
        StopAllMovement();
    }

    // ---------- 유틸 ----------
    void StartWait(float sec)
    {
        waitUntil = Time.time + Mathf.Max(0f, sec);
        state = State.Wait;
    }

    void StopAllMovement()
    {
        if (moveAStar) moveAStar.Stop();
        if (moveFly) moveFly.Stop();
    }

    void UpdateFacing()
    {
        var spr = GetComponentInChildren<SpriteRenderer>();
        if (!spr) return;

        if (ctx.target)
        {
            bool faceLeft = ctx.target.position.x < transform.position.x;
            spr.flipX = faceLeft;
        }
        else
        {
            if (ctx.rb.linearVelocity.x > 0.01f) spr.flipX = false;
            if (ctx.rb.linearVelocity.x < -0.01f) spr.flipX = true;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (chaseStop < chaseStart) chaseStop = chaseStart + 0.1f;
    }
#endif
}