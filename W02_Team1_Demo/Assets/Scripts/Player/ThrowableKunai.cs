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

    void OnCollisionEnter2D(Collision2D collision)  
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
    }

    public bool IsStuck()
    {
        return isStuck;
    }
}
