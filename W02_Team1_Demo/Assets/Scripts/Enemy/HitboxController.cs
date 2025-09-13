using UnityEngine;
using System.Collections;

public class HitboxController : MonoBehaviour
{
    public bool isActive = false;
    public static HitboxController Instance { get; private set; }
    [SerializeField] private GameObject player;
    private float knockbackForce = 10f;

    [Header("슬로우 모션 효과")]
    private float slowdownFactor = 0.3f; // 얼마나 느려지게 할지 (0.05 = 5%)
    private float slowdownLength = 4f;   // 슬로우 모션 지속 시간 (초)

    [Header("카메라 줌 효과")]
    [SerializeField] private Camera mainCamera; // 메인 카메라를 인스펙터에서 연결
    private float zoomInSize = 10f; // 줌 했을 때 카메라 크기 (작을수록 확대)
    private float originalCameraSize; // 원래 카메라 크기를 저장할 변수

    // ⭐ 추가된 코드: 원래 카메라 위치를 저장할 변수
    private Vector3 originalCameraPosition;

    // 폭발 프리펩
    public GameObject explosionPrefab;

    private void Awake()
    {
        //// 인스턴스가 이미 존재하면 새로 생성된 오브젝트를 파괴합니다.
        //if (Instance != null && Instance != this)
        //{
        //    Debug.Log("asdasd");
        //    Destroy(this.gameObject);
        //    return;
        //}

        //// 이 스크립트의 인스턴스를 Instance 변수에 할당합니다.
        Instance = this;
    }
    void Start()
    {
        // 게임 시작 시, 원래 카메라 크기와 위치를 저장해 둡니다.
        if (mainCamera != null)
        {
            originalCameraSize = mainCamera.orthographicSize;
            // ⭐ 추가된 코드: 카메라의 현재 위치를 저장
            originalCameraPosition = mainCamera.transform.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;
        if (collision.CompareTag("Enemy"))
        {
            // 슬로우 모션 및 카메라 줌 효과를 시작합니다.
            StartSlowMotionEffect();

            // 적을 튕겨냅니다.
            KnockbackEnemy(collision);
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
    }

    public void KnockbackEnemy(Collider2D enemyCollider)
    {
        Rigidbody2D enemyRigidbody = enemyCollider.GetComponent<Rigidbody2D>();
        if (enemyRigidbody != null)
        {
            Vector2 direction = (enemyCollider.transform.position - player.transform.position).normalized;
            direction.y += 0.1f;
            Vector2 knockbackDirection = (direction + Vector2.up).normalized;
            enemyRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }

    // 슬로우 모션 및 카메라 효과를 관리하는 함수
    public void StartSlowMotionEffect()
    {
        // 코루틴을 사용하여 시간의 흐름에 따라 효과를 적용하고 해제합니다.
        StartCoroutine(SlowMotionCoroutine());
    }

    private IEnumerator SlowMotionCoroutine()
    {
        // --- 효과 시작 ---
        // 1. 시간을 느리게 만듭니다.
        Time.timeScale = slowdownFactor;
        // 2. FixedUpdate의 호출 주기도 시간에 맞춰 느려지므로, 이를 보정해줍니다.
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        if (mainCamera != null)
        {
            // 3. 카메라를 확대합니다.
            mainCamera.orthographicSize = zoomInSize;

            // ⭐ 추가된 코드: 카메라의 위치를 플레이어 위치로 변경합니다.
            // Z축은 변경하지 않습니다.
            Vector3 newCameraPosition = player.transform.position;
            newCameraPosition.z = mainCamera.transform.position.z;
            mainCamera.transform.position = newCameraPosition;
        }

        // --- 효과 지속 ---
        // 지정된 시간(slowdownLength)만큼 기다립니다.
        // Time.timeScale의 영향을 받지 않는 실시간 기준으로 기다립니다.
        yield return new WaitForSecondsRealtime(slowdownLength);

        // --- 효과 종료 ---
        // 1. 시간을 원래 속도로 되돌립니다.
        Time.timeScale = 1f;
        // 2. FixedUpdate 시간도 원래대로 복구합니다.
        Time.fixedDeltaTime = 0.02f;

        if (mainCamera != null)
        {
            // 3. 카메라도 원래 크기로 되돌립니다.
            mainCamera.orthographicSize = originalCameraSize;

            // ⭐ 추가된 코드: 카메라를 원래 위치로 되돌립니다.
            mainCamera.transform.position = originalCameraPosition;
        }
    }
}