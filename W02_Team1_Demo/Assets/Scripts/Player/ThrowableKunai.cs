using System.Collections;
using UnityEngine;

public class ThrowableKunai : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isStuck = false;
    public GameObject borderObject;
    public bool isInvincible = false; // 무적 상태 여부

    private Vector2 hitNormal = Vector2.zero; //  벽에 꽂힌 방향 저장

    // 감속을 위한 변수 추가
    [Header("쿠나이 감속")]
    [SerializeField] private float minSpeed = 30.0f;
    [SerializeField] float decreaseTime = 1.0f;

    // 쿠나이가 박혔을때 애니메이션 추가
    [Header("쿠나이 애니메이션")]
    [SerializeField] Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();        
        StartCoroutine(BecomeVulnerableAfterDelay(0.02f));
    }

    private void Start()
    {
        // 생성 시 감속 코드
        StartCoroutine(DecreaseSpeed());
    }

    private IEnumerator BecomeVulnerableAfterDelay(float delay)
    {
        isInvincible = true;
        yield return new WaitForSeconds(delay); // 0.1초 대기
        isInvincible = false;
    }

    private IEnumerator DecreaseSpeed()
    {
        float startSpeed = rb.linearVelocity.magnitude;
        
        if (startSpeed <= minSpeed) yield break;

        float elapsed = 0f;
        while (elapsed < decreaseTime && !isStuck)
        {
            float t = elapsed / decreaseTime;
            float targetSpeed = Mathf.Lerp(startSpeed, minSpeed, t);

            if (rb.linearVelocity.sqrMagnitude > 0.0001f)
            {
                Vector2 dir = rb.linearVelocity.normalized;
                rb.linearVelocity = dir * targetSpeed;
            }

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 마지막 보정
        if (!isStuck && rb.linearVelocity.magnitude > minSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * minSpeed;
        }
    }



    void Update()
    {
        // ⭐ 속도가 0.01 이상일 때만 회전
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
    }


    public void OnHit(RaycastHit2D hit, Vector2 throwDirection)
    {
        if (isStuck) return;
        isStuck = true;

        Debug.Log("123");
        // STUCK ANIMATION
        animator.SetTrigger("isStuck");

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        // 1. 살짝 안쪽으로 박히게 위치 보정
        float insetDistance = 0.05f; // 벽 안으로 들어갈 거리
        Vector2 finalPos = hit.point - hit.normal * insetDistance;
        transform.position = finalPos;

        // 2. 던진 방향으로 회전
        float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 3. Wall / Enemy / RotationPlatform 처리
        if (hit.collider.CompareTag("Enemy"))
        {
            StickToEnemy(hit.collider.transform);
        }
        else if (hit.collider.CompareTag("RotationPlatform"))
        {
            RotationPlatform platform = hit.collider.GetComponent<RotationPlatform>();
            if (platform != null)
            {
                Vector3 localPos = platform.transform.InverseTransformPoint(transform.position);
                Vector2 localNormal = platform.transform.InverseTransformDirection(hit.normal);
                platform.SetKunaiTransform(this, localPos, localNormal);
            }
        }
        

        else if (hit.collider.CompareTag("MovingPlatform"))
        {
            MovingPlatform platform = hit.collider.GetComponent<MovingPlatform>();
            if (platform != null)
            {
                Vector2 worldPos = hit.point - hit.normal * 0.05f;
                platform.SetKunaiTransform(this, worldPos, hit.normal);
            }
        }

        else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            hitNormal = hit.normal;
            GetComponent<TrailRenderer>().enabled = false;
        }
    }


    void StickToEnemy(Transform enemy)
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
