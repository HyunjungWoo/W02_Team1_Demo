using UnityEngine;
using System.Collections;
public class TeleportRawLineEffect : MonoBehaviour
{
    private Renderer objectRenderer;
    private Material objectMaterial;
    private float MaxYScale = 0.1f;
    private float MaxXScale = 1f;
    private float MinYScale = 0.2f;
    private float phase1Duration = 0.2f;
    private float phase2Duration = 0.3f;

    private float initialYScale;
    private float initialXScale; // X축 초기 크기를 저장할 변수

    void Awake()
    {
        // 스크립트가 붙은 오브젝트와 자식 오브젝트에서 Renderer 컴포넌트를 찾습니다.
        objectRenderer = GetComponentInChildren<Renderer>();
        if (objectRenderer != null)
        {
            objectMaterial = objectRenderer.material;
        }
    }

    void Start()
    {
        // 현재 Y축 크기를 저장합니다.
        initialYScale = transform.localScale.y;
        initialXScale = transform.localScale.x; // 시작 시 X축 크기 저장
        // 애니메이션 코루틴을 시작합니다.
        StartCoroutine(AnimateAndFadeOut());
    }

    IEnumerator AnimateAndFadeOut()
    {
        // 1단계와 2단계에 모두 사용될 타이머
        float totalTimer = 0f;
        float totalDuration = phase1Duration + phase2Duration;
        // --- 1단계: 0.2초 동안 Y축 크기 1만큼 늘리기 ---
        float startScaleY_1 = transform.localScale.y;
        float endScaleY_1 = startScaleY_1 + MaxYScale;
        float endScaleX = transform.localScale.x + MaxXScale;

        while (totalTimer < phase1Duration)
        {
            totalTimer += Time.deltaTime;
            float progress = totalTimer / phase1Duration;

            float currentScaleY = Mathf.Lerp(startScaleY_1, endScaleY_1, progress);
            float currentScaleX = Mathf.Lerp(initialXScale, endScaleX, progress); // X축 크기 계산
            transform.localScale = new Vector3(currentScaleX, currentScaleY, transform.localScale.z);

            yield return null;
        }

        // 1단계 애니메이션 완료 후 정확한 크기로 설정
        transform.localScale = new Vector3(endScaleX, endScaleY_1, transform.localScale.z);

        float startScaleY_2 = transform.localScale.y;
        float endScaleY_2 = startScaleY_2 - MinYScale;

        if (objectMaterial != null)
        {
            Color startColor = objectMaterial.color;
            startColor.a = 1f; // 시작 알파 값은 1
            objectMaterial.color = startColor;

            while (totalTimer < totalDuration)
            {
                totalTimer += Time.deltaTime;
                float progress2 = (totalTimer - phase1Duration) / phase2Duration;
                float overallProgress = totalTimer / totalDuration; // 전체 진행률

                // Y축 크기 줄이기
                float currentScaleY = Mathf.Lerp(startScaleY_2, endScaleY_2, progress2);

                // 최종 크기 적용
                transform.localScale = new Vector3(transform.localScale.x, currentScaleY, transform.localScale.z);

                // 페이드 아웃
                Color currentColor = objectMaterial.color;
                currentColor.a = Mathf.Lerp(1f, 0f, progress2);
                objectMaterial.color = currentColor;

                yield return null;
            }

            // 2단계 애니메이션 완료 후 최종 상태 설정
            transform.localScale = new Vector3(transform.localScale.x, endScaleY_2, transform.localScale.z);
            objectMaterial.color = new Color(objectMaterial.color.r, objectMaterial.color.g, objectMaterial.color.b, 0f);
        }

        // 애니메이션이 끝나면 오브젝트를 파괴합니다.
        Destroy(gameObject);
    }
}
