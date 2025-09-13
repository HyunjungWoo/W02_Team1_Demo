using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
public class AstarMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;  // 목표 최고 속도
    public float accel = 30f;         // 가속도 추가
    public float nextWaypointDistance = 0.8f;
    public float pathUpdateInterval = 0.4f;

    private Path path;
    private int currentWaypoint = 0;
    private float pathUpdateTimer;

    private Seeker seeker;
    private Rigidbody2D rb;

    void Awake()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    public void MoveTo(Vector3 destination)
    {
        if (Time.time > pathUpdateTimer)
        {
            seeker.StartPath(rb.position, destination, OnPathComplete);
            pathUpdateTimer = Time.time + pathUpdateInterval;
        }
    }

    public void Stop()
    {
        path = null;
        // 정지 시에도 Y축 속도는 유지해야 자연스럽게 떨어짐
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        if (path == null || currentWaypoint >= path.vectorPath.Count)
        {
            // 목표가 없을 때 X축 속도만 서서히 줄임
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, 0.2f), rb.linearVelocity.y);
            return;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

        // ★★★ 가장 중요한 수정 부분 ★★★
        // X축 속도만 제어하고, Y축 속도는 현재 물리 상태(중력 등)를 그대로 유지합니다.
        float targetVx = direction.x * moveSpeed;
        float newVx = Mathf.MoveTowards(rb.linearVelocity.x, targetVx, accel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newVx, rb.linearVelocity.y);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }
}