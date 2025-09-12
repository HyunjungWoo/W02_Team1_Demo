using UnityEngine;

/// <summary>
/// 간단한 공중 이동: 목표 지점으로 직선 이동.
/// (A* 없이도 공중몹은 맵 제약이 적기 때문에 충분)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Movement2DFly : MonoBehaviour
{
    public float moveSpeed = 3f;   // 속도
    public float arriveDist = 0.1f;// 이 거리 이하면 도착

    Rigidbody2D rb;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    /// <summary>이 위치로 이동</summary>
    public void MoveTo(Vector2 target)
    {
        Vector2 delta = target - rb.position;
        if (delta.magnitude <= arriveDist) { Stop(); return; }
        rb.linearVelocity = delta.normalized * moveSpeed;
    }

    /// <summary>멈춤(자연 감속)</summary>
    public void Stop()
    {
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.2f);
    }
}