using UnityEngine;

public class ThrowableKunai : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isStuck = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isStuck && collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {

            isStuck = true;

            // rb.useFullKinematicContacts = true; // 충돌 감지를 유지하면서 물리적 움직임을 멈춥니다
            rb.linearVelocity = Vector2.zero; // 속도를 0으로 설정하여 움직임을 멈춥니다
            //rb.angularVelocity = 0f; // 회전 속도를 0으로 설정하여 회전을 멈춥니다
            // y position 고정
            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

            GetComponent<TrailRenderer>().enabled = false; // 트레일 렌더러 비활성화


        }
        // 아직 꽂히지 않았고, 부딪힌 대상이 "Enemy" 태그를 가지고 있다면
        // collision.gameObject 대신 collision.transform을 사용합니다.
        if (!isStuck && collision.gameObject.CompareTag("Enemy"))
        {
            StickToEnemy(collision.transform);
        }
    }
   
    private void StickToEnemy(Transform enemy)
    {
        isStuck = true;

        // --- 물리 효과 제거 ---
        // Rigidbody의 모든 물리 활동을 정지시킵니다.
        // Destroy(rb) 대신 시뮬레이션을 끄는 것이 더 안전할 수 있습니다.
        rb.bodyType = RigidbodyType2D.Kinematic; // 물리 엔진의 영향을 받지 않게 됨
        rb.linearVelocity = Vector2.zero; // 혹시 모를 속도 제거

        // Collider도 비활성화
        GetComponent<Collider2D>().enabled = false;

        // --- 적에게 꽂히기 ---
        transform.parent = enemy;

        //적 스크립트에 꽂혔다고 알려주기
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
}
