using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하기 위해 필수!

public class HealthBarUI : MonoBehaviour
{
    [Header("연결할 대상")]
    public HealthSystem targetHealthSystem; // HP 정보를 가져올 대상
    public Slider healthSlider;             // 제어할 슬라이더 UI

    void Start()
    {
        // 타겟이 설정되지 않았으면 아무것도 하지 않음
        if (targetHealthSystem == null || healthSlider == null)
        {
            Debug.LogError("HealthBarUI에 타겟 또는 슬라이더가 연결되지 않았습니다!");
            return;
        }

        // HealthSystem의 HP 변경 신호(OnHealthChanged)가 오면, 나의 UpdateSlider 함수를 실행해달라고 '구독'
        targetHealthSystem.OnHealthChanged += UpdateSlider;

        // 슬라이더 초기 설정
        healthSlider.maxValue = targetHealthSystem.MaxHp;
        healthSlider.value = targetHealthSystem.CurrentHp;
    }

    // HealthSystem으로부터 신호를 받았을 때 실행될 함수
    private void UpdateSlider(int currentHp, int maxHp)
    {
        healthSlider.value = currentHp;
    }

    // 이 오브젝트가 파괴될 때 구독을 취소하여 메모리 누수 방지 (좋은 습관)
    void OnDestroy()
    {
        if (targetHealthSystem != null)
        {
            targetHealthSystem.OnHealthChanged -= UpdateSlider;
        }
    }
}