// EnemyBrain.cs (AstarMovement 스위치 제어 기능 추가 버전)
using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyBrain : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack, Wait }

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

    [Header("공격 후 대기")]
    public float waitAfterAttack = 0.5f;

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
        if (moveAStar) moveAStar.Active = false; // [수정] AstarMovement의 물리 제어를 끔
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
        if (ctx.target != null && (currentState == State.Chase || currentState == State.Attack || currentState == State.Wait))
        {
            bool faceLeft = ctx.target.position.x < transform.position.x;
            spr.flipX = faceLeft;
        }
        else
        {
            if (ctx.rb.linearVelocity.x > 0.01f) spr.flipX = false;
            else if (ctx.rb.linearVelocity.x < -0.01f) spr.flipX = true;
        }
    }
}