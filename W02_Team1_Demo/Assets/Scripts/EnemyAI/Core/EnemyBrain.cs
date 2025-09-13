// EnemyBrain.cs (AstarMovement 스위치 제어 기능 추가 버전)
using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyBrain : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, PrepareAttack, Attack, Wait }

    [Header("센서/경로")]
    public PlayerDetector2D detector;
    public PatrolPath patrolPath;

    [Header("이동 모듈")]
    public AstarMovement moveAStar;
    public Movement2DFly moveFly;

    [Header("공격 모듈(붙인 것만 사용)")]
    public AttackMelee melee;
    public AttackDash dash;
    public AttackRanged ranged;

    [Header("추격 설정")]
    public float chaseStart = 8f;
    public float chaseStop = 12f;

    [Header("공격 준비")]
    public float attackPrepareDuration = 1f; // 공격 준비 시간 (1초)
    private bool isPreparingAttack = false;
    private float prepareAttackEndTime;
    public float waitAfterAttack = 0.5f; // 공격 후 쿨타임

    private bool isMove, isAttack, isWait;
    private EnemyContext ctx;
    private State currentState = State.Idle;
    private int patrolIndex = 0;
    private float waitUntil = 0f;

    void Awake()
    {
        ctx = GetComponent<EnemyContext>();
        if (!detector) detector = GetComponent<PlayerDetector2D>();
        if (!moveAStar) moveAStar = GetComponent<AstarMovement>();
    }

    void Update()
    {
        UpdateStateMachine();
        switch (currentState)
        {
            case State.Idle: DoIdle(); break;
            case State.Patrol: DoPatrol(); break;
            case State.Chase: DoChase(); break;
            case State.Attack: DoAttack(); break;
            case State.PrepareAttack: DoPrepareAttack(); break; // [추가]
            case State.Wait: DoWait(); break;
        }
        UpdateFacing();
    }

    void UpdateStateMachine()
    {
        if (ctx.target == null)
        {
            currentState = HasPatrolPath() ? State.Patrol : State.Idle;
            return;
        }
        float distanceToTarget = Vector2.Distance(transform.position, ctx.target.position);
        if (currentState == State.Wait && Time.time < waitUntil) return;
        bool isAttackingNow = (melee && melee.IsAttacking) || (dash && dash.IsDashing);
        if (isAttackingNow)
        {
            currentState = State.Attack;
            return;
        }
        // 공격 준비 상태 처리
        if (isPreparingAttack)
        {
            // 준비 시간이 끝났으면 사거리와 상관없이 바로 공격
            if (Time.time >= prepareAttackEndTime)
            {
                isPreparingAttack = false;
                if (melee) melee.HideTelegraph();
                currentState = State.Attack;
                return;
            }

            // 아직 준비 중이면 다른 행동 말고 상태 유지
            currentState = State.PrepareAttack;
            return;
        }

        // 공격 가능 범위에 들어왔고, 아직 준비 중이 아니라면 -> 공격 준비 시작
        if (CanAttack(ctx.target, distanceToTarget) && !isPreparingAttack)
        {
            isPreparingAttack = true;
            prepareAttackEndTime = Time.time + attackPrepareDuration;
            if (melee) melee.ShowTelegraph(); // 근접 공격 모듈의 예고 표시
                                              // 다른 공격(돌진, 원거리)의 예고 표시 로직도 필요하다면 여기에 추가

            currentState = State.PrepareAttack;
            return;
        }

        bool isCurrentlyChasing = currentState == State.Chase || currentState == State.Attack;
        if (detector.CanSeeTarget(transform, ctx.target) || (isCurrentlyChasing && distanceToTarget < chaseStop))
        {
            currentState = State.Chase;
            return;
        }
        currentState = HasPatrolPath() ? State.Patrol : State.Idle;
    }

    // [추가] 공격 준비 상태의 행동
    void DoPrepareAttack()
    {
        isMove = false; isAttack = false; isWait = false;
        if (moveAStar) moveAStar.Active = false;
        StopAllMovement(); // 준비하는 동안 제자리에 멈춤
    }


    private bool CanAttack(Transform target, float distance)
    {
        if (melee && melee.IsReady && melee.InRange(transform, target)) return true;
        if (dash && dash.IsReady && dash.InRange(transform, target)) return true;
        if (ranged && ranged.IsReady && ranged.InRange(transform, target)) return true;
        return false;
    }

    private bool HasPatrolPath()
    {
        return patrolPath && patrolPath.waypoints != null && patrolPath.waypoints.Length > 0;
    }

    // --- 상태 실행 함수 수정 ---

    void DoIdle()
    {
        isMove = false; isAttack = false; isWait = false;
        if (moveAStar) moveAStar.Active = true; // [수정] 이동 가능하도록 스위치 켜기
        StopAllMovement();
    }

    void DoPatrol()
    {
        if (!HasPatrolPath()) { currentState = State.Idle; return; }
        isMove = true; isAttack = false; isWait = false;
        if (moveAStar) moveAStar.Active = true; // [수정] 이동 가능하도록 스위치 켜기

        Transform wp = patrolPath.waypoints[patrolIndex];
        if (!wp) return;

        if (moveAStar) moveAStar.MoveTo(wp.position);
        else if (moveFly) moveFly.MoveTo(wp.position);
        else return;

        if (Vector2.Distance(transform.position, wp.position) <= patrolPath.arriveDist)
        {
            StartWait(patrolPath.waitAtPoint);
            patrolIndex = (patrolIndex + 1) % patrolPath.waypoints.Length;
        }
    }

    void DoChase()
    {
        if (ctx.target == null) { currentState = State.Idle; return; }
        isMove = true; isAttack = false; isWait = false;
        if (moveAStar) moveAStar.Active = true; // [수정] 이동 가능하도록 스위치 켜기

        if (moveAStar) moveAStar.MoveTo(ctx.target.position);
        else if (moveFly) moveFly.MoveTo(ctx.target.position);
    }

    void DoAttack()
    {
        if (ctx.target == null) { currentState = State.Idle; return; }
        isMove = false; isAttack = true; isWait = false;
        if (moveAStar) moveAStar.Active = false;
        StopAllMovement();

        if ((melee && melee.IsAttacking) || (dash && dash.IsDashing)) return;

        // [수정] InRange() 검사를 제거하여, 일단 공격 상태가 되면 무조건 공격하도록 변경
        if (melee && melee.IsReady) melee.StartAttack(this);
        else if (dash && dash.IsReady) dash.StartDash(transform, ctx.target, this);
        else if (ranged && ranged.IsReady) ranged.Fire(transform, ctx.target);

        StartWait(waitAfterAttack);
    }

    void DoWait()
    {
        isMove = false; isAttack = false; isWait = true;
        if (moveAStar) moveAStar.Active = false; // [수정] 대기 중에도 물리 제어를 끔
        StopAllMovement();
    }

    void StartWait(float sec)
    {
        waitUntil = Time.time + Mathf.Max(0f, sec);
        currentState = State.Wait;
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

        // [수정] 공격 준비 및 공격 중에는 방향 전환을 아예 실행하지 않음
        if (currentState == State.PrepareAttack || currentState == State.Attack)
        {
            return; // 아무것도 하지 않고 함수 종료
        }

        // 'Chase' 상태일 때만 타겟을 바라봄
        if (ctx.target != null && currentState == State.Chase)
        {
            bool faceLeft = ctx.target.position.x < transform.position.x;
            spr.flipX = faceLeft;
        }
        // 그 외의 상태(Idle, Patrol, Wait)에서는 이동 속도에 따라 방향 전환
        else
        {
            if (ctx.rb.linearVelocity.x > 0.01f) spr.flipX = false;
            else if (ctx.rb.linearVelocity.x < -0.01f) spr.flipX = true;
        }
    }
}