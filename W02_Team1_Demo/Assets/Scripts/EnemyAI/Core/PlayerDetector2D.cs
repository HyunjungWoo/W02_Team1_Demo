// PlayerDetector2D.cs (업그레이드 버전)
using UnityEngine;

/// <summary>
/// 플레이어 탐지: 반경 내 + (옵션)시야 확보 체크.
/// "보인다/안보인다"만 판단하고, 추격/공격 결정은 Brain이 함.
/// </summary>
public class PlayerDetector2D : MonoBehaviour
{
    [Header("탐지 설정")]
    public float radius = 8f;                 // 탐지 반경
    public bool requireLineOfSight = true;    // 시야 확보(Line of Sight) 체크 여부
    public LayerMask visionBlockMask;         // 시야를 가리는 장애물 레이어 (예: Ground, Platform)
    public string playerTag = "Player";

    /// <summary>
    /// 지정한 타겟이 탐지 범위와 시야 내에 있는지 확인합니다.
    /// </summary>
    /// <param name="self">탐지를 수행하는 자기 자신</param>
    /// <param name="target">탐지할 대상</param>
    /// <returns>탐지되었으면 true, 아니면 false</returns>
    public bool CanSeeTarget(Transform self, Transform target)
    {
        if (target == null)
        {
            return false;
        }

        // 1. 반경 체크
        float distance = Vector2.Distance(self.position, target.position);
        if (distance > radius)
        {
            return false;
        }

        // 2. 시야 확보(Line of Sight) 체크
        if (requireLineOfSight)
        {
            // Raycast를 쏴서 self와 target 사이에 시야를 가리는 장애물이 있는지 확인
            RaycastHit2D hit = Physics2D.Raycast(self.position, (target.position - self.position).normalized, distance, visionBlockMask);

            // 무언가에 맞았는데 그게 플레이어가 아니라면, 시야가 가려진 것임
            if (hit.collider != null && !hit.collider.CompareTag(playerTag))
            {
                return false;
            }
        }

        // 모든 조건을 통과했으면 '보인다'고 판단
        return true;
    }

    // 유니티 에디터에서 탐지 범위를 시각적으로 보여주는 기능
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 0, 0.25f); // 반투명 노란색
        Gizmos.DrawSphere(transform.position, radius);
    }
}