using UnityEngine;

// HealthSystem 부품이 반드시 필요하다고 명시
[RequireComponent(typeof(HealthSystem))]
public class PlayerHP : MonoBehaviour
{
    private HealthSystem health;

    void Awake()
    {
        // 내 게임오브젝트에 붙어있는 HealthSystem 부품을 찾아옴
        health = GetComponent<HealthSystem>();
    }

    void Start()
    {
        // HealthSystem의 OnDeath 이벤트(신호)가 발생하면, 나의 HandleDeath 함수를 실행해달라고 '구독' 신청
        health.OnDeath += HandleDeath;
    }

    // HealthSystem으로부터 사망 신호를 받았을 때 실행될 함수
    private void HandleDeath()
    {
        Debug.Log("Player script received death signal. Calling GameManager.");
        // GameManager에 게임오버 상태 변경을 요청
        GameManager.SetDead();

        // 여기에 플레이어 오브젝트를 파괴하거나, 비활성화하는 코드를 추가할 수 있음
        // gameObject.SetActive(false);
    }

    // 스크립트가 파괴될 때 구독을 취소하여 메모리 누수 방지 (좋은 습관)
    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
        }
    }
}