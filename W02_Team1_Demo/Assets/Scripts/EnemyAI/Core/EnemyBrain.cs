// EnemyBrain.cs (moveFly, UpdateFacing 모두 복원된 최종 버전)
using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyBrain : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack, Wait }

    [Header("센서/경로")]
    public PlayerDetector2D detector;
    public PatrolPath patrolPath;

    [Header("이동 모듈")]
    public AstarMovement moveAStar; // ★ A* 지상 이동 (새 스크립트)
    public Movement2DFly moveFly;   // [복원] 공중 이동 모듈

    [Header("공격 모듈(붙인 것만 사용)")]
    public AttackMelee melee;
    public AttackDash dash;
    public AttackRanged ranged;

    [Header("추격 설정")]
    public float chaseStart = 8f;
    public float chaseStop = 12f;

    [Header("공격 후 대기")]
    public float waitAfterAttack = 0.5f;

    // 애니메이터 상태 플래그
    private bool isMove, isAttack, isWait;

    // 내부 시스템 변수
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
            case State.Wait: DoWait(); break;
        }

        //if (ctx.animator)
        //{
        //    ctx.animator.SetBool("isMove", isMove);
        //    ctx.animator.SetBool("isAttack", isAttack);
        //    ctx.animator.SetBool("isWait", isWait);
        //}

        // [복원] EnemyBrain이 직접 방향 전환을 제어
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

        if (CanAttack(ctx.target, distanceToTarget))
        {
            currentState = State.Attack;
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

    void DoIdle()
    {
        isMove = false; isAttack = false; isWait = false;
        StopAllMovement();
    }

    void DoPatrol()
    {
        if (!HasPatrolPath()) { currentState = State.Idle; return; }

        isMove = true; isAttack = false; isWait = false;
        Transform wp = patrolPath.waypoints[patrolIndex];
        if (!wp) return;

        // [복원] 이동 모듈 우선순위 적용
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

        // [복원] 이동 모듈 우선순위 적용
        if (moveAStar) moveAStar.MoveTo(ctx.target.position);
        else if (moveFly) moveFly.MoveTo(ctx.target.position);
    }

    void DoAttack()
    {
        if (ctx.target == null) { currentState = State.Idle; return; }
        isMove = false; isAttack = true; isWait = false;
        StopAllMovement();

        if ((melee && melee.IsAttacking) || (dash && dash.IsDashing)) return;

        if (melee && melee.IsReady && melee.InRange(transform, ctx.target)) melee.StartAttack(this);
        else if (dash && dash.IsReady && dash.InRange(transform, ctx.target)) dash.StartDash(transform, ctx.target, this);
        else if (ranged && ranged.IsReady && ranged.InRange(transform, ctx.target)) ranged.Fire(transform, ctx.target);

        StartWait(waitAfterAttack);
    }

    void DoWait()
    {
        isMove = false; isAttack = false; isWait = true;
        StopAllMovement();
    }

    void StartWait(float sec)
    {
        waitUntil = Time.time + Mathf.Max(0f, sec);
        currentState = State.Wait;
    }

    void StopAllMovement()
    {
        // [복원] 모든 이동 모듈 정지
        if (moveAStar) moveAStar.Stop();
        if (moveFly) moveFly.Stop();
    }

    // [복원] 기존의 UpdateFacing 함수
    void UpdateFacing()
    {
        var spr = GetComponentInChildren<SpriteRenderer>();
        if (!spr) return;

        // 타겟이 있거나, 이동 중이거나, 공격 중일 때 방향을 결정
        if (ctx.target != null && (currentState == State.Chase || currentState == State.Attack || currentState == State.Wait))
        {
            // 타겟이 왼쪽에 있으면 왼쪽을 바라보도록 설정
            bool faceLeft = ctx.target.position.x < transform.position.x;
            spr.flipX = faceLeft;
        }
        else // 타겟이 없거나, Idle/Patrol 상태일 때 (이동 속도 기반)
        {
            // 이동 속도가 오른쪽(+)이면 오른쪽, 왼쪽(-)이면 왼쪽을 바라보도록 설정
            if (ctx.rb.linearVelocity.x > 0.01f) spr.flipX = false;
            else if (ctx.rb.linearVelocity.x < -0.01f) spr.flipX = true;
            // 멈춰있을 때는 이전 방향 유지
        }
        // 이 코드가 스프라이트의 FlipX를 제어하므로, 몬스터 스프라이트 자체는 기본적으로 오른쪽을 바라보도록 만들어야 합니다.
    }
}