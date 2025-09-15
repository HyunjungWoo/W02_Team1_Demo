using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraZoneTrigger : MonoBehaviour
{
    public CameraManager manager;   // manager.Activate(int), ActivatePlayer() 제공
    public int cameraIndex = 1;

    [Header("Optional")]
    public float enterDelay = 0f;   // 진입 후 약간 늦게 전환하고 싶을 때
    public bool oneShot = false;    // 한 번만 동작하고 비활성화

    bool triggered;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered && oneShot) return;
        if (!other.CompareTag("Player")) return;

        if (enterDelay > 0f) Invoke(nameof(DoActivate), enterDelay);
        else DoActivate();

        if (oneShot) triggered = true;
    }

    void DoActivate()
    {
        if (manager != null) manager.Activate(cameraIndex);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.25f); // 녹색
        var col = GetComponent<Collider2D>();
        if (col) Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
#endif
}
