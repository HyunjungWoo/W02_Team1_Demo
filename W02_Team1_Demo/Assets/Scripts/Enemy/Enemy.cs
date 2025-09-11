using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject slicedBodyPrefab; // 반으로 갈라진 시체 프리팹
    private ThrowableKunai stuckKunai; // 내 몸에 꽂힌 쿠나이

    // 쿠나이가 자신에게 꽂혔을 때 호출될 함수
    public void SetStuckKunai(ThrowableKunai kunai)
    {
        stuckKunai = kunai;
    }

    // 플레이어가 순간이동으로 도착했을 때 호출할 함수
    public void DieAndSlice()
    {
        // 1. 갈라진 시체 프리팹을 현재 내 위치에 생성합니다.
        if (slicedBodyPrefab != null)
        {
            Instantiate(slicedBodyPrefab, transform.position, transform.rotation);
        }

        // 2. 내 몸에 꽂혀있던 쿠나이가 있다면 파괴합니다. (시체와 함께 사라지지 않도록)
        if (stuckKunai != null)
        {
            Destroy(stuckKunai.gameObject);
        }

        // 3. 자기 자신(원본 적 오브젝트)을 파괴합니다.
        Destroy(gameObject);
    }
}
