using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필요합니다.

public class SuperHeroLanding : MonoBehaviour
{
    private Renderer objectRenderer;
    private Material objectMaterial;

    void Awake()
    {
        // 핵심 변경 사항: GetComponent<Renderer>() 대신 GetComponentInChildren<Renderer>()를 사용합니다.
        // 이렇게 하면 이 오브젝트와 모든 자식 오브젝트에서 Renderer 컴포넌트를 찾습니다.
        objectRenderer = GetComponentInChildren<Renderer>();
        if (objectRenderer != null)
        {
            objectMaterial = objectRenderer.material;
        }
        else
        {
            Debug.LogError("자식 오브젝트를 포함한 어디에도 Renderer 컴포넌트가 없어 페이드 아웃을 할 수 없습니다.", this);
            enabled = false;
        }
    }
    void Start()
    {
        // 코루틴을 시작하여 크기 조절과 오브젝트 파괴를 진행합니다.
        StartCoroutine(ScaleAndDestroy());
    }

    // 크기 조절 애니메이션과 파괴를 담당하는 코루틴입니다.
    IEnumerator ScaleAndDestroy()
    {

        if (objectMaterial == null)
        {
            Debug.LogError("머티리얼이 없어 페이드 아웃을 할 수 없습니다.", this);
            Destroy(gameObject);
            yield break;
        }
        Color startColor = objectMaterial.color;
        float targetScale = 10f; // 목표 크기
        float duration = 0.2f;    // 애니메이션이 진행될 시간 (2초)
        float timer = 0f;       // 경과 시간

        startColor.a = 1f;
        objectMaterial.color = startColor;

        // 타이머가 지속 시간(duration)을 넘을 때까지 반복합니다.
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // 크기 조절: 부모의 크기가 커지면 자식 오브젝트도 함께 커집니다.
            transform.localScale = Vector3.one * Mathf.Lerp(1f, targetScale, progress);

            // 페이드 아웃: 자식 오브젝트의 머티리얼 알파 값을 조절합니다.
            Color currentColor = startColor;
            currentColor.a = Mathf.Lerp(1f, 0f, progress);
            objectMaterial.color = currentColor;

            yield return null;
        }
        transform.localScale = Vector3.one * targetScale;
        startColor.a = 0f;
        objectMaterial.color = startColor;

        Destroy(gameObject);
    }
}