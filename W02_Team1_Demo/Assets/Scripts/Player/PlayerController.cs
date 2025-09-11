using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal.Internal;

public class PlayerController : MonoBehaviour
{
    [Header("던지기 설정")]
    public GameObject kunaiPrefab; // 던질 수 있는 칼날 프리팹
    public float thorwForce = 30f; // 던지는 힘
    public LineRenderer aimLine; // 조준선을 그리기 위한 LineRenderer

    // 내부 변수
    private ThrowableKunai currentKunai; // 현재 던져진 칼날
    private Camera mainCamera; // 메인 카메라 참조
    private bool isAiming = false; // 조준 중인지 여부

    void Start()
    {
        mainCamera = Camera.main; // 메인 카메라 참조 초기화
        if (aimLine != null)
        {
            aimLine.enabled = false; // 처음에는 조준선을 비활성화
        }
        else
        {
            Debug.LogError("Aim Line Renderer is not assigned.");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 왼쪽 버튼을 누르는 순간
        {
            isAiming = true; // 조준 시작
            aimLine.enabled = true; // 조준선 활성화
        }

        if (isAiming)
        {
            UpdateAimLine(); // 조준선 업데이트
        }

        if (Input.GetMouseButtonUp(0)) // 왼쪽 버튼을 떼는 순간
        {
            if (isAiming)
            {
                ThrowKunai(); // 칼날 던지기
                isAiming = false; // 조준 종료
                aimLine.enabled = false; // 조준선 비활성화
            }
        }

        if (Input.GetMouseButton(1))
        { // 마우스 우클릭을 누르면
            if (currentKunai != null && currentKunai.IsStuck())
            {
               WarpToKunai(); // 칼날 위치로 순간이동
            }
        }


    }
    
    private void UpdateAimLine()
    {
        Vector2 playerPosition = transform.position; // 플레이어 위치

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition); // 마우스 위치

        Vector2 aimDirection = (mousePosition - playerPosition).normalized; // 조준 방향 계산

        aimLine.SetPosition(0, playerPosition); // 조준선 시작점 설정
        // 조준선 길이 설정
        aimLine.SetPosition(1, playerPosition + aimDirection * 5f); // 조준선 끝점 설정 (길이 5)
    }

    private void ThrowKunai()
    {
        Vector2 playerPosition = transform.position;
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
       
        // 방향 벡터 계산
        Vector2 thorwDirection = (mousePosition - playerPosition).normalized;

        // 회전 로직
        float angleRad = Mathf.Atan2(thorwDirection.y, thorwDirection.x);

        // 라디안을 우리가 사용하는 각도로 변환
        float angleDeg = angleRad * Mathf.Rad2Deg;

        Quaternion rotation = Quaternion.Euler(0, 0, angleDeg);

        // 기존 쿠나이가 있다면 파괴 ( 새로운 쿠나이를 던지기 전에 이전 쿠나이 정리)
        if (currentKunai != null)
        {
            Destroy(currentKunai.gameObject);
        }

        // 쿠나이 생성 및 던지기
        GameObject kunaiInstance = Instantiate(kunaiPrefab, playerPosition, rotation);
        currentKunai = kunaiInstance.GetComponent<ThrowableKunai>();

        // 생성된 쿠나이에 힘을 가해 날려보냄
        // ForceMode2D.Impulse를 사용하여 순간적인 힘을 가함
        kunaiInstance.GetComponent<Rigidbody2D>().AddForce(thorwDirection * thorwForce, ForceMode2D.Impulse);
    }
    private void WarpToKunai()
    {
        transform.position = currentKunai.transform.position;
        Destroy(currentKunai.gameObject);
        currentKunai = null;
    }

}
