using System.Collections;
using UnityEngine;

public class ThrowableKunai : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isStuck = false;
    public GameObject borderObject;
    public bool isInvincible = false; // 무적 상태 여부

    private Vector2 hitNormal = Vector2.zero; //  벽에 꽂힌 방향 저장

    // ⭐ 감속을 위한 변수 추가
    [Header("쿠나이 감속")]
    [SerializeField] private float dragFactor = 0.98f; // 매 프레임마다 속도를 98%로 줄임
    [SerializeField] private float minSpeed = 0.5f; // 이 속도 이하가 되면 멈춤

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(BecomeVulnerableAfterDelay(0.02f));
    }
    private IEnumerator BecomeVulnerableAfterDelay(float delay)
    {
        isInvincible = true;
        yield return new WaitForSeconds(delay); // 0.1초 대기
        isInvincible = false;
    }
    void Update()
    {

        // ⭐ 감속 로직 추가
        if (rb.linearVelocity.sqrMagnitude > minSpeed * minSpeed)
        {
            // 현재 속도에 감속 계수를 곱하여 속도를 줄입니다.
            rb.linearVelocity *= dragFactor;
        }
        else
        {
            // 최소 속도 이하가 되면 완전히 멈춥니다.
            rb.linearVelocity = Vector2.zero;
        }
        // ⭐ 속도가 0.01 이상일 때만 회전
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        //// --- 스프라이트 방향 회전 ---
        //if (!isStuck && rb.linearVelocity.sqrMagnitude > 0.01f)
        //{
        //    float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        //    transform.rotation = Quaternion.Euler(0, 0, angle);
        //}
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isInvincible) return;
        if (borderObject != null)
        {
            borderObject.SetActive(true);
        }

        // 벽에 꽂힌 경우
        if (!isStuck && collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            isStuck = true;

            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            GetComponent<TrailRenderer>().enabled = false;

            //  벽 Normal 추출 (Raycast 방식)
            // 쿠나이의 진행 방향 기준으로 짧게 Ray 쏴서 Normal 얻기
            Vector2 dir = rb.linearVelocity.normalized;
            if (dir == Vector2.zero) dir = transform.right; // 혹시 멈췄을 경우 대비

            RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) - dir * 0.1f, dir, 0.2f, LayerMask.GetMask("Wall"));
            if (hit.collider != null)
            {
                hitNormal = hit.normal;
            }
        }

        //  적에 꽂힌 경우
        if (!isStuck && collision.gameObject.CompareTag("Enemy"))
        {
            StickToEnemy(collision.transform);
        }

        if (!isStuck && collision.CompareTag("RotationPlatform"))
        {
            isStuck = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;

            RotationPlatform platform = collision.GetComponent<RotationPlatform>();

            if (platform != null)
            { // 현재 위치와 Normal을 local로 변환해서 전달
                Vector3 localPos = platform.transform.InverseTransformPoint(transform.position);
                Vector2 localNormal = platform.transform.InverseTransformDirection(Vector2.up); // 여기서 RotationPlatform 쪽 함수 호출
                platform.SetKunaiTransform(this, localPos, localNormal);
            }
        }

    }

    private void StickToEnemy(Transform enemy)
    {
        isStuck = true;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        transform.parent = enemy;

        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.SetStuckKunai(this);
        }
    }

    public bool IsStuck()
    {
        return isStuck;
    }

    public Vector2 GetHitNormal() // 플레이어가 가져다 쓰기 위한 함수
    {
        return hitNormal;
    }

    public void SetHitNormal(Vector2 normal_)
    {
        hitNormal = normal_;
    }
}
