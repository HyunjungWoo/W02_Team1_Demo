using UnityEngine;

public class HitboxDamager : MonoBehaviour
{
    public int damage = 10; // 이 공격의 데미지

    void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 상대에게 HealthSystem 컴포넌트가 있는지 확인
        HealthSystem targetHealth = other.GetComponent<HealthSystem>();

        // HealthSystem을 가지고 있다면 (플레이어든, 다른 적이든)
        if (targetHealth != null)
        {
            // TakeDamage 함수를 호출하여 데미지를 줌
            targetHealth.TakeDamage(damage);

            // [선택 사항] 한 번의 공격에 한 번만 데미지를 주기 위해
            // 충돌 직후 콜라이더를 비활성화 할 수 있습니다.
            // 이 기능이 필요하다면 아래 줄의 주석을 해제하세요.
            // gameObject.GetComponent<Collider2D>().enabled = false;
        }
    }
}
