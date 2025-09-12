using UnityEngine;
using Pathfinding; // A* Pathfinding Project

/// <summary>
/// A* Pathfinding(Seeker) 기반 2D 지상 이동 모듈.
/// Brain이 MoveTo()로 목적지를 던져주면, 내부에서 경로를 따라 이동합니다.
/// - AddForce 방식보다 확실히 움직이도록, 기본은 X축 속도 지정(velocity) 모드.
/// - 필요하면 AddForce 모드로 바꿀 수 있습니다.
/// </summary>
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class Movement2DGroundAstar : MonoBehaviour
{
    [Header("이동 튜닝")]
    [Tooltip("A* 경로를 따라갈 때, X축 목표 속도(velocity 모드에서 사용)")]
    public float maxSpeedX = 5f;

    [Tooltip("velocity 모드에서 목표 속도로 접근하는 감속/가속 계수(높을수록 더 즉답)")]
    public float accel = 30f;

    [Tooltip("AddForce 모드에서 힘 계수(velocity 모드 미사용 시에만 사용)")]
    public float force = 200f;

    [Tooltip("웨이포인트 전환 거리")]
    public float nextWaypointDistance = 2.5f;

    [Tooltip("경로 재요청 주기(초)")]
    public float repathInterval = 0.4f;

    [Tooltip("정지 시 X축 감속 계수(0~1)")]
    public float stopFriction = 0.2f;

    [Tooltip("속도(velocity)로 이동할지 여부. 끄면 AddForce로 이동")]
    public bool useVelocityMode = true;

    private Seeker seeker;
    private Rigidbody2D rb;

    private Pathfinding.Path path;
    private int currentWaypoint = 0;

    private bool hasDestination = false;   // 현재 목적지가 있는가
    private Vector2 desiredDestination;    // 가고 싶은 월드 위치
    private float repathTimer = 0f;        // 다음 경로 요청까지 남은 시간

    void Awake()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        
    }

    void Update()
    {
        if (!hasDestination) return;

        // 주기적으로 새 경로 요청
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f && seeker.IsDone())
        {
            Debug.Log("start destination" + desiredDestination + " " + rb.position);
            seeker.StartPath(rb.position, desiredDestination, OnPathComplete);
            repathTimer = repathInterval;
        }
    }

    void OnPathComplete(Pathfinding.Path p)
    {
        if (!p.error)
        {
            
            path = p;
            
            Debug.Log("path OK2" + path.vectorPath.Count);
            
            for (int i = 0; i < path.vectorPath.Count; i++)
            {
                Debug.Log(path.vectorPath[i] + " wp" + i);
            }
            currentWaypoint = 0;
#if UNITY_EDITOR
            // 디버그 확인용
            // Debug.Log($"{name} path OK: {path.vectorPath.Count} points");
#endif
        }
        else
        {
            path = null;
#if UNITY_EDITOR
            Debug.LogWarning($"{name} path ERROR");
#endif
        }
    }

    void FixedUpdate()
    {
        if (!hasDestination || path == null)
        {
            

            // 목적지/경로 없을 땐 자연 감속(멈춤)
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0f, stopFriction), rb.linearVelocity.y);
            return;
        }

        // 경로 끝
        if (currentWaypoint >= path.vectorPath.Count)
        {
            
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0f, stopFriction), rb.linearVelocity.y);
            return;
        }

        // 다음 웨이포인트로의 방향
        Vector2 waypoint = (Vector2)path.vectorPath[currentWaypoint];
        //Debug.Log(waypoint + " waypoint");
        //Debug.Log(path.vectorPath[currentWaypoint] + " cur wp" + currentWaypoint);
        Vector2 dir = (waypoint - rb.position);
        float dist = dir.magnitude;
        if (dist > 0.001f) dir /= dist; // normalized
        
        if (useVelocityMode)
        {
            // X축 목표 속도로 부드럽게 접근 (점프/낙하는 중력에 맡김)
            float targetVx = dir.x * maxSpeedX;
            float newVx = Mathf.MoveTowards(rb.linearVelocity.x, targetVx, accel * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newVx, rb.linearVelocity.y);
        }
        else
        {
            // AddForce 모드(원한다면 사용)
            Vector2 f = dir * force * Time.fixedDeltaTime;
            rb.AddForce(f);
        }

        // 충분히 가까우면 다음 웨이포인트
        if (dist < nextWaypointDistance) currentWaypoint++;
    }

    /// <summary>Brain이 호출: 이 좌표로 가!</summary>
    public void MoveTo(Vector2 worldPos)
    {
        
        desiredDestination = worldPos;
        hasDestination = true;

        // 빠른 반응을 위해 경로 즉시 재계산 유도
        if (repathTimer > repathInterval * 0.25f) repathTimer = 0f;
    }

    /// <summary>Brain이 호출: 멈춰!</summary>
    public void Stop()
    {
        hasDestination = false;
        path = null;
    }

    void OnDrawGizmosSelected()
    {
        if (path == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < path.vectorPath.Count - 1; i++)
            Gizmos.DrawLine(path.vectorPath[i], path.vectorPath[i + 1]);
    }
}