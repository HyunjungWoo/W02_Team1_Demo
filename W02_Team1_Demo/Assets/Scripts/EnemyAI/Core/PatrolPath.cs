using UnityEngine;

/// <summary>
/// 순찰용 웨이포인트 목록.
/// Brain이 이 배열을 따라 이동하고 각 지점에서 잠깐 대기.
/// </summary>
public class PatrolPath : MonoBehaviour
{
    public Transform[] waypoints;   // 순서대로 이동할 지점들
    public float arriveDist = 0.2f; // 이 거리 이하면 도착으로 간주
    public float waitAtPoint = 0.5f;// 각 지점에서 쉬는 시간
}
