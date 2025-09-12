using UnityEngine;

/// <summary>
/// 플레이어 탐지: 반경 내 + (옵션)시야막힘 체크.
/// "보인다/안보인다"만 판단하고, 추격/공격 결정은 Brain이 함.
/// </summary>
public class PlayerDetector2D : MonoBehaviour
{
    [Header("탐지 설정")]
    public float radius = 8f;                  // 탐지 반경
    public bool requireLineOfSight = true;     // 시야막힘 체크 여부
    public LayerMask visionMask;               // Player + Ground 포함
    public string playerTag = "Player";

    /// <summary>플레이어를 발견하면 true, target/dist를 반환.</summary>
    public bool TryDetect(Transform self, out Transform target, out float dist)
    {
        target = null; dist = Mathf.Infinity;

        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (!player) return false;

        var pt = player.transform;
        dist = Vector2.Distance(self.position, pt.position);
        if (dist > radius) return false;

        if (requireLineOfSight)
        {
            // 살짝 위쪽에서 레이 → 장애물에 가렸는지 확인
            Vector2 origin = (Vector2)self.position + Vector2.up * 0.3f;
            Vector2 to = (Vector2)pt.position + Vector2.up * 0.5f;
            var hit = Physics2D.Raycast(origin, (to - origin).normalized, dist, visionMask);

            // 첫 히트가 Player여야 "보인다"
            if (!hit.collider || !hit.collider.CompareTag(playerTag)) return false;
        }

        target = pt;
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}