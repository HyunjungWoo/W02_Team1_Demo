using UnityEngine;
using UnityEngine.UI;

public class KunaiDirectionIndicator : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static KunaiDirectionIndicator Instance { get; private set; }

    [SerializeField] private Image indicatorImage;
    // 여기 새로운 변수를 추가합니다. 인스펙터에서 여백을 조정할 수 있습니다.
    [SerializeField] private float edgeMargin = 0.05f;

    private Transform targetKunai;
    private Camera mainCam;
    private RectTransform rectTransform;

    void Awake()
    {
        // 싱글톤 인스턴스 설정 (기존과 동일)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        mainCam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        if (indicatorImage != null)
        {
            indicatorImage.enabled = false;
        }
    }

    void Update()
    {
        // ... (Update 함수 시작 부분은 기존과 동일) ...
        if (targetKunai == null)
        {
            if (indicatorImage.enabled)
            {
                indicatorImage.enabled = false;
            }
            return;
        }

        Vector3 screenPos = mainCam.WorldToViewportPoint(targetKunai.position);

        bool isOffScreen = screenPos.x < 0 || screenPos.x > 1 || screenPos.y < 0 || screenPos.y > 1 || screenPos.z < 0;

        if (isOffScreen)
        {
            if (!indicatorImage.enabled)
            {
                indicatorImage.enabled = true;
            }

            Vector3 center = new Vector3(0.5f, 0.5f, 0f);
            Vector3 direction = screenPos - center;
            direction.z = 0;
            direction.Normalize();

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rectTransform.rotation = Quaternion.Euler(0, 0, angle);

            // 여백을 적용하여 UI 위치를 계산
            // (0.5f - edgeMargin)을 곱해 화면 안쪽으로 이동시킵니다.
            Vector3 clampedPos = center + direction * (0.5f - edgeMargin);
            rectTransform.anchorMin = clampedPos;
            rectTransform.anchorMax = clampedPos;
        }
        else
        {
            if (indicatorImage.enabled)
            {
                indicatorImage.enabled = false;
            }
        }
    }

    // 외부에서 호출하여 추적할 쿠나이를 설정
    public void SetTarget(Transform newTarget)
    {
        targetKunai = newTarget;
    }
}