using UnityEngine;

/// <summary>
/// 적이 공통으로 쓰는 참조를 보관하는 컨텍스트.
/// - rb, animator, target(비우면 Player 태그 자동 탐색)
/// </summary>
[DisallowMultipleComponent]
public class EnemyContext : MonoBehaviour
{
    [Header("타깃(비워두면 Player 태그 자동)")]
    public Transform target;

    [Header("공용 참조")]
    public Rigidbody2D rb;
    public Animator animator;

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform; // Player 태그 자동 연결
        }
    }
}