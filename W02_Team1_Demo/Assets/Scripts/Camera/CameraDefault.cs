using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraDefault : MonoBehaviour
{
    public CameraManager manager;
    [Header("Optional")]
    public float enterDelay = 0f;
    public bool oneShot = false;

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

        if (enterDelay > 0f) Invoke(nameof(DoReturn), enterDelay);
        else DoReturn();

        if (oneShot) triggered = true;
    }

    void DoReturn()
    {
        if (manager != null) manager.ActivatePlayer(); // 또는 manager.Activate(0);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.25f); // 파란색
        var col = GetComponent<Collider2D>();
        if (col) Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
#endif
}
