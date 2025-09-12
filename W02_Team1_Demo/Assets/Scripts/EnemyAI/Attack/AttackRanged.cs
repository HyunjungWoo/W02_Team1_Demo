using UnityEngine;

/// <summary>
/// 원거리 발사: firePoint에서 Projectile 프리팹을 생성 후 날림.
/// </summary>
public class AttackRanged : MonoBehaviour
{
    [Header("발사 설정")]
    public Transform firePoint;            // 총구 위치
    public Projectile projectilePrefab;    // 투사체 프리팹
    public float cooldown = 0.6f;          // 발사 간격
    public float range = 8f;               // 발사 가능 거리

    float nextFireTime;

    /// <summary>지금 발사 가능한가?</summary>
    public bool IsReady => Time.time >= nextFireTime;

    /// <summary>타깃이 사거리 안인가?</summary> 
    public bool InRange(Transform self, Transform target)
        => target && Vector2.Distance(self.position, target.position) <= range;

    /// <summary>외부(Brain)에서 호출: 발사</summary>
    public void Fire(Transform self, Transform target)
    {
        if (!IsReady || !firePoint || !projectilePrefab || !target) return;

        var dir = (target.position - firePoint.position).normalized;
        var p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        p.Init((Vector2)dir);

        nextFireTime = Time.time + cooldown;
    }
}