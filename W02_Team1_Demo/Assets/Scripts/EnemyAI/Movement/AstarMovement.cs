// AstarMovement.cs (스위치 기능 추가 버전)
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
public class AstarMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public float accel = 30f;
    public float nextWaypointDistance = 0.8f;
    public float pathUpdateInterval = 0.4f;

    // [추가] 이 스크립트의 작동 여부를 제어하는 스위치
    public bool Active { get; set; } = true;

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
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        // [추가] 스위치가 꺼져있으면 아무것도 하지 않음
        if (!Active) return;

        if (path == null || currentWaypoint >= path.vectorPath.Count)
        {
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, 0.2f), rb.linearVelocity.y);
            return;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
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
