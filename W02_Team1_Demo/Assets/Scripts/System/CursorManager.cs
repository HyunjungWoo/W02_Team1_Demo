using System.Linq;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
   public static CursorManager Instance { get; private set; }

    [Header("조준 보정 설정")]
    [SerializeField] private float aimAssistRadius = 0.5f;
    [SerializeField] private Texture2D lockOnCursorTexture;
    [SerializeField] private LayerMask enemyLayer;

    [Header("커서 핫스팟 (중심점)")]
    [SerializeField] private Vector2 hotSpotOffset = Vector2.zero;
    
    // 🎯 외부에서 현재 조준된 적을 확인할 수 있도록 public 프로퍼티로 선언
    public Transform LockedOnEnemy { get; private set; }

    private Camera mainCamera;
    private bool isAimAssistActive = false;

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않게 하려면 주석 해제
        }
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

        // 조준 대상 유무에 따라 커서 모양 변경
        if (LockedOnEnemy != null)
        {
            SetCursor(lockOnCursorTexture);
        }
        else
        {
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
