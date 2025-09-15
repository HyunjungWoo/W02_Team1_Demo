using UnityEngine;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [Header("HP 설정")]
    [SerializeField] private int maxHp = 100;
    public int MaxHp => maxHp; // [수정] 외부에서 maxHp를 읽을 수 있도록 프로퍼티 추가
    public int CurrentHp { get; private set; }

    [Header("무적 설정")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    private bool isInvincible = false;

    // [추가] HP가 변경될 때 호출될 신호 (현재 HP, 최대 HP)
    public event System.Action<int, int> OnHealthChanged;
    public event System.Action OnDeath;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        CurrentHp = maxHp;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // [추가] 게임 시작 시 UI가 초기 HP를 표시할 수 있도록 신호 한 번 보내기
        OnHealthChanged?.Invoke(CurrentHp, maxHp);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || CurrentHp <= 0) return;

        CurrentHp -= damage;

        // [수정] HP가 변경되었음을 구독자들에게 알림
        OnHealthChanged?.Invoke(CurrentHp, maxHp);

        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        if (spriteRenderer != null)
        {
            float endTime = Time.time + invincibilityDuration;
            while (Time.time < endTime)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(0.1f);
            }
            spriteRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(invincibilityDuration);
        }

        isInvincible = false;
    }

    void OnEnable()
    {
        if (InGameManager.Instance != null)
        {
            OnDeath += InGameManager.Instance.PlayerDied;
        }
    }

    // Player가 비활성화되거나 파괴될 때 호출됩니다.
    void OnDisable()
    {
        // 구독했던 PlayerDied 함수를 해지합니다.
        // 이걸 하지 않으면 오류가 발생할 수 있습니다! (매우 중요)
        if (InGameManager.Instance != null)
        {
            OnDeath -= InGameManager.Instance.PlayerDied;
        }
    }
}