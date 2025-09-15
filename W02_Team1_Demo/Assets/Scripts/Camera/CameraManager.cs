using UnityEngine;

using Unity.Cinemachine;


public class CameraManager : MonoBehaviour 
{
    [Header("Cameras[0] = PlayerCam, 나머지는 ZoneCams")]
    public CinemachineCamera[] cams; // [0]: Player, [1..N]: Zones
    [Tooltip("기본 우선순위(비활성 카메라)")]
    public int basePriority = 10;
    [Tooltip("활성 카메라 우선순위")]
    public int activePriority = 100;

    int activeIndex = 0; // 현재 활성(보는) 카메라 인덱스

    void Awake()
    {
        // 초기화: PlayerCam을 활성으로
        for (int i = 0; i < cams.Length; i++)
            cams[i].Priority = (i == 0) ? activePriority : basePriority;
        activeIndex = 0;
    }

    public void Activate(int index)
    {
        if (index < 0 || index >= cams.Length) return;
        for (int i = 0; i < cams.Length; i++)
            cams[i].Priority = basePriority;
        cams[index].Priority = activePriority;
        activeIndex = index;
    }

    public void ActivatePlayer() => Activate(0);

    public void ActivateNextZone()
    {
        // Player(0) 다음부터 순차 증가
        int next = Mathf.Clamp(activeIndex + 1, 1, cams.Length - 1);
        Activate(next);
    }
}

