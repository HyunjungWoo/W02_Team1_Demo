using System.Linq;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
   public static CursorManager Instance { get; private set; }

    [Header("조준 보정 설정")]
    [SerializeField] private float aimAssistRadius = 0.5f;
    [SerializeField] private Texture2D lockOnCursorTexture;
    [SerializeField] private LayerMask enemyLayer;

    [Header("조준 방해물 설정")]
    [SerializeField] private Transform playerTransform; 
    [SerializeField] private LayerMask wallLayer;       
    [SerializeField] private Texture2D obstructedCursorTexture;  

    [Header("커서 핫스팟 (중심점)")]
    [SerializeField] private Vector2 hotSpotOffset = Vector2.zero;
    
    // 🎯 외부에서 현재 조준된 적을 확인할 수 있도록 public 프로퍼티로 선언
    public Transform LockedOnEnemy { get; private set; }

    private Camera mainCamera;
    private bool isAimAssistActive = false;

    private void Awake()
    {
       if(Instance == null) Instance = this;
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // 조준 보정이 활성화된 상태일 때만 매 프레임 실행
        if (isAimAssistActive)
        {
            UpdateAimAssistTarget();
        }
    }

    // 외부(PlayerController)에서 조준 보정을 켜고 끌 수 있도록 public 함수로 만듦
    public void SetAimAssistActive(bool isActive)
    {
        isAimAssistActive = isActive;
        if (!isActive)
        {
            ResetToDefaultCursor();
        }
    }

    private void UpdateAimAssistTarget()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // 마우스 주변의 모든 적 콜라이더를 가져옴
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(mousePosition, aimAssistRadius, enemyLayer);

        // LINQ를 사용해 가장 가까운 적을 찾음 (없으면 null)
        LockedOnEnemy = enemiesInRange
            .OrderBy(enemy => Vector2.Distance(mousePosition, enemy.transform.position))
            .FirstOrDefault()?.transform;

        // 조준 대상이 있다면, 플레이어와의 사이에 벽이 있는지 확인
        if (LockedOnEnemy != null)
        {
            // 1. 플레이어가 할당되었는지 안전하게 확인
            if (playerTransform == null)
            {
                Debug.LogError("Player Transform이 CursorManager에 할당되지 않았습니다! Raycast를 실행할 수 없습니다.");
                ResetToDefaultCursor(); // 오류 발생 시 기본 커서로 되돌림
                return;
            }

            // 2. Raycast에 필요한 정보 계산
            Vector2 startPoint = playerTransform.position;
            Vector2 endPoint = LockedOnEnemy.position;
            Vector2 direction = (endPoint - startPoint).normalized;
            float distance = Vector2.Distance(startPoint, endPoint);

            // 3. Raycast 실행 (wallLayer만 감지하도록)
            RaycastHit2D hit = Physics2D.Raycast(startPoint, direction, distance, wallLayer);

            // 4. Raycast 결과에 따라 커서 변경
            if (hit.collider != null)
            {
                // 벽에 막혔을 경우: 방해물 커서로 변경
                SetCursor(obstructedCursorTexture);
            }
            else
            {
                // 시야가 확보된 경우: 조준 커서로 변경
                SetCursor(lockOnCursorTexture);
            }
        }
        else
        {
            // 조준 대상이 없을 경우
            ResetToDefaultCursor();
        }
    }

    private void SetCursor(Texture2D cursorTexture)
    {
        if (cursorTexture == null)
        {
            ResetToDefaultCursor();
            return;
        }
        Vector2 hotSpot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2) + hotSpotOffset;
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }
    
    private void ResetToDefaultCursor()
    {
        LockedOnEnemy = null;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
