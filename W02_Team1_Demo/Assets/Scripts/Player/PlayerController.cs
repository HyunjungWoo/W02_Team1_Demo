using System;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))] // 연결하지 않아도 자동으로 연결
public class PlayerController : MonoBehaviour, IPlayerController
{
    #region 이동 관련 변수
    // 중요: 이 변수는 반드시 Inspector에서 할당해주어야 합니다!
    [SerializeField] private ScriptableStats stats;
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private FrameInput frameInput;
    private Vector2  frameVelocity;
    private bool  cachedQueryStartInColliders;
    private float  time;
    #endregion

    #region 대시 관련 변수
    private bool    isDashing;
    private float   dashTimeLeft;
    private float   dashCooldownTimer;

    #endregion

    #region 벽타기 관련 변수
    private bool onRightWall;
    private bool onLeftWall;
    private bool isWallSliding;
    private float wallStickTimer; // 벽에 붙어있는 시간을 계산하기 위한 타이머
    private float frameLeftWall = float.MinValue; //  
    #endregion

    #region 점프하기 변수 
    // Jumping
    private bool jumpToConsume;
    private bool bufferedJumpUsable;
    private bool endedJumpEarly;
    private bool coyoteUsable;
    private float timeJumpWasPressed;
    #endregion

    #region 쿠나이 관련 변수
    [Header("던지기 설정")]
    public GameObject kunaiPrefab;
    public float thorwForce = 30f;
    public LineRenderer aimLine;

    [Header("반동 설정")]
    [SerializeField] private float selfForce = 2f;

    private ThrowableKunai currentKunai;
    private Camera mainCamera;
    private bool isAiming = false;

    [Header("슈퍼히어로 랜딩 설정")]
    [SerializeField] private GameObject superHeroLandingCheckBox;
    #endregion

    #region 인터페이스 구현
    public Vector2 FrameInput => frameInput.Move;
    private bool CanUseWallCoyote => !grounded && time <= frameLeftWall + stats.WallCoyoteTime;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    #endregion

    #region 슬로우 모션 변수
    [Header("슬로우 모션 효과")]
    private float slowdownFactor = 0.3f; // 얼마나 느려지게 할지 (0.05 = 5%)
    private float slowdownLength = 1f;   // 슬로우 모션 지속 시간 (초)

    #endregion

    private void Awake()
    {
        // Tarodev의 Awake() 내용: 필수 컴포넌트 초기화
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

    }

private void Start()
    {
        // 직접 만드신 Start() 내용: 카메라 및 조준선 초기화
        mainCamera = Camera.main;
        if (aimLine != null)
        {
            aimLine.enabled = false;
        }
        else
        {
            Debug.LogError("Aim Line Renderer is not assigned.");
        }
        // 게임 시작 시, 연결된 오브젝트가 있는지 확인합니다.
        if (superHeroLandingCheckBox == null)
        {
            Debug.LogError("superHeroLandingCheckBox가 연결되지 않았습니다!");
        }
    }

    private void Update()
    {
        // Tarodev의 시간 추적 및 입력 수집
        time += Time.deltaTime;
        GatherInput();
        HandleKunaiActions();
        // 대쉬 쿨타임 처리
        HandleDashCooldown();
    }

    private void FixedUpdate()
    {
        // Tarodev의 물리 기반 이동 처리 (수정 없음)
        CheckCollisions();
        HandleWallSlide();
        HandleJump();
        HandleDash();
        HandleDirection();
        HandleGravity();
        ApplyMovement();
    }

   

    #region 입력 처리 (Input Handling)

    private void GatherInput()
    {
        // Tarodev의 이동 및 점프 입력 처리
        frameInput = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
            JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
            DashDown = Input.GetButton("Fire3") || Input.GetKeyDown(KeyCode.X),
            Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
        };

        if (stats.SnapInput)
        {
            frameInput.Move.x = Mathf.Abs(frameInput.Move.x) < stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(frameInput.Move.x);
            frameInput.Move.y = Mathf.Abs(frameInput.Move.y) < stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(frameInput.Move.y);
        }

        if (frameInput.JumpDown)
        {
            jumpToConsume = true;
            timeJumpWasPressed = time;
        }

    }

    private void HandleKunaiActions()
    {
        // 기존 Update()에 있던 쿠나이 관련 로직을 별도의 함수로 정리
        if (Input.GetMouseButtonDown(0))
        {
            isAiming = true;
            aimLine.enabled = true;
        }

        if (isAiming)
        {
            UpdateAimLine();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isAiming)
            {
                ThrowKunai();
                isAiming = false;
                aimLine.enabled = false;
            }
        }

        if (Input.GetMouseButton(1))
        {
            if (currentKunai != null && currentKunai.IsStuck())
            {
                // 대쉬 쿨타임 초기화
                dashCooldownTimer = 0;
                WarpToKunai();
            }
        }
    }


    #endregion

    #region 쿠나이 로직 (Kunai Logic)

    private void UpdateAimLine()
    {
        Vector2 playerPosition = transform.position;
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = (mousePosition - playerPosition).normalized;
        aimLine.SetPosition(0, playerPosition);
        aimLine.SetPosition(1, playerPosition + aimDirection * 5f);
    }

    private void ThrowKunai()
    {
        if (currentKunai != null) Destroy(currentKunai.gameObject);

        Vector2 playerPosition = transform.position;
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 throwDirection = (mousePosition - playerPosition).normalized;
        float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject kunaiInstance = Instantiate(kunaiPrefab, playerPosition, rotation);
        currentKunai = kunaiInstance.GetComponent<ThrowableKunai>();
        Vector2 aimDirection = (mousePosition - playerPosition).normalized;
        if(aimDirection.x < 0 && onLeftWall)
        {
            currentKunai.isInvincible = false;
        }
        else if (aimDirection.x > 0 && onRightWall)
        {
            currentKunai.isInvincible = false;
        }
        kunaiInstance.GetComponent<Rigidbody2D>().AddForce(throwDirection * thorwForce, ForceMode2D.Impulse);
    }

    private void WarpToKunai()
    {
        Vector3 warpPosition = currentKunai.transform.position;
       
        transform.position = warpPosition;

        Transform enemyTransform = currentKunai.transform.parent;
        if (enemyTransform != null && enemyTransform.CompareTag("Enemy"))
        {
            Debug.Log("쿠나이 적에게감");
            // 3. 적의 스크립트를 가져와서 '갈라지며 죽는' 함수를 호출합니다! 💥
            Enemy enemy = enemyTransform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.DieAndSlice();
            }
            // 이 시점에서 원본 적과 쿠나이는 파괴됩니다.
            // 4. 자신의 Rigidbody에 위쪽으로 힘을 가해 반동 효과를 줍니다.
            if (rb != null)
            {
                // 기존 속도를 0으로 초기화하여 힘이 더 깔끔하게 들어가도록 합니다.
                rb.linearVelocity = Vector2.zero;
                // 위쪽으로 튀어 오르는 힘을 줍니다.
                rb.AddForce(Vector2.up * selfForce, ForceMode2D.Impulse);
                rb.AddForce(Vector2.right * selfForce, ForceMode2D.Impulse);
            }
            StartSlowMotionEffect();

        }
        else
        {
            HitboxController.Instance.isActive = true;
            Invoke("DeactivateObject", 0.1f);   // 적 날리는 박스 해제.

            // 벽 보정
            warpPosition = CheckWallInner(warpPosition);
            transform.position = warpPosition;

        }
        // 반동 효과
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(Vector2.up * selfForce, ForceMode2D.Impulse);
        }


        Destroy(currentKunai.gameObject); // 워프 후 쿠나이는 파괴
        currentKunai = null;
    }

    public void StartSlowMotionEffect()
    {
        // 코루틴을 사용하여 시간의 흐름에 따라 효과를 적용하고 해제합니다.
        StartCoroutine(SlowMotionCoroutine());
    }

    private IEnumerator SlowMotionCoroutine()
    {
        // --- 효과 시작 ---
        // 1. 시간을 느리게 만듭니다.
        Time.timeScale = slowdownFactor;
        // 2. FixedUpdate의 호출 주기도 시간에 맞춰 느려지므로, 이를 보정해줍니다.
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        yield return new WaitForSecondsRealtime(slowdownLength);

        // --- 효과 종료 ---
        // 1. 시간을 원래 속도로 되돌립니다.
        Time.timeScale = 1f;
        // 2. FixedUpdate 시간도 원래대로 복구합니다.
        Time.fixedDeltaTime = 0.02f;

    }
    // 적 밀어내는 박스 사라지게 하는 함수. 날라가고 0.1~0.3초뒤 끌것.
    private void DeactivateObject()
    {
        HitboxController.Instance.isActive = false;

    }

    /// <summary>
    /// 워프하려는 위치가 벽 안쪽이라면, 쿠나이가 꽂힌 벽의 Normal 기준으로
    /// 안전한 위치로 보정해서 돌려준다.
    /// </summary>
    private Vector3 CheckWallInner(Vector3 targetPos)
    {
        if (currentKunai != null)
        {
            Vector2 normal = currentKunai.GetHitNormal();

            if (normal != Vector2.zero)
            {
                // 플레이어 콜라이더 크기만큼 바깥쪽으로 밀어냄
                float offset = col.size.magnitude * 0.5f; // 캡슐 반경 정도
                return targetPos + (Vector3)(normal * offset);
            }
        }

        // Normal 정보가 없으면 기존 방식 fallback
        Vector2 dir = (targetPos - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, targetPos);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance, LayerMask.GetMask("Wall"));

        if (hit.collider != null)
        {
            return hit.point - dir * 0.1f;
        }

        return targetPos;
    }


    #endregion

    #region Tarodev 이동 로직 

    // Collisions
    private float frameLeftGrounded = float.MinValue;
    private bool grounded;

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;
        float facingDirection = transform.localScale.x; // 캐릭터가 바라보는 방향 (1 또는 -1)

        // 땅&천장 충돌 검사 
        bool groundHit = Physics2D.CapsuleCast(col.bounds.center, col.size, col.direction, 0, Vector2.down, stats.GrounderDistance, ~stats.PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(col.bounds.center, col.size, col.direction, 0, Vector2.up, stats.GrounderDistance, ~stats.PlayerLayer);

        if (ceilingHit) frameVelocity.y = Mathf.Min(0, frameVelocity.y);

        if (!grounded && groundHit)
        {
            grounded = true;
            coyoteUsable = true;
            bufferedJumpUsable = true;
            endedJumpEarly = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(frameVelocity.y));
        }
        else if (grounded && !groundHit)
        {
            grounded = false;
            frameLeftGrounded = time;
            GroundedChanged?.Invoke(false, 0);
        }

        // 벽 충돌 검사
        onRightWall = Physics2D.CapsuleCast(col.bounds.center, col.size, col.direction, 0, Vector2.right, 0.1f, stats.WallLayer);
        // 왼쪽 벽 확인
        onLeftWall = Physics2D.CapsuleCast(col.bounds.center, col.size, col.direction, 0, Vector2.left, 0.1f, stats.WallLayer);
        Physics2D.queriesStartInColliders = cachedQueryStartInColliders;
    }

    private bool HasBufferedJump => bufferedJumpUsable && time < timeJumpWasPressed + stats.JumpBuffer;
    private bool CanUseCoyote => coyoteUsable && !grounded && time < frameLeftGrounded + stats.CoyoteTime;

    private void HandleJump()
    {
        if(isDashing) return; // 대쉬 중에는 점프 불가

        if(jumpToConsume && (isWallSliding || CanUseWallCoyote))
        {
            ExecuteWallJump();
            return;
        }

        if (!endedJumpEarly && !grounded && !frameInput.JumpHeld && rb.linearVelocity.y > 0) endedJumpEarly = true;
        if (!jumpToConsume && !HasBufferedJump) return;
        if (grounded || CanUseCoyote) ExecuteJump();
        jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        endedJumpEarly = false;
        timeJumpWasPressed = 0;
        bufferedJumpUsable = false;
        coyoteUsable = false;
        frameVelocity.y = stats.JumpPower;
        Jumped?.Invoke();
    }

    private void ExecuteWallJump()
    {
        float wallDirection = onRightWall ? 1 : -1;

        // 벽 점프 직후 상태 초기화
        isWallSliding = false;
        jumpToConsume = false;
        endedJumpEarly = false;
        frameLeftWall = float.MinValue;

        // 힘 계산: 이제 wallDirection이 실제 벽의 위치이므로, 
        // -wallDirection은 항상 벽의 반대 방향이 됨
        Vector2 force = new Vector2(stats.WallJumpPower.x * -wallDirection, stats.WallJumpPower.y);
        frameVelocity = force;

        // 벽 점프 후에는 반대 방향을 보도록 캐릭터를 뒤집어 줌
        transform.localScale = new Vector3(-wallDirection, 1, 1);

    }

    private void HandleDashCooldown()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void HandleDash()
    {
        // 대쉬 시작 조건 확인
        if (frameInput.DashDown && dashCooldownTimer <= 0 && !isDashing)
        {
            Vector2 dashDirection;

            if (frameInput.Move.x != 0) // 좌우 입력이 있을 때 
            {
                dashDirection = new Vector2(Mathf.Sign(frameInput.Move.x), 0);  // sign으로 -1,1로 방향 고정
            }
            else
            {
                dashDirection = new Vector2(transform.localScale.x, 0); // 입력 없으면 바라보는 방향으로 대시
            }

            isDashing = true;
            dashTimeLeft = stats.DashDuration;
            frameVelocity = dashDirection.normalized * stats.DashPower; // 대시 속도 설정
            dashCooldownTimer = stats.DashCooldown; // 쿨타임 초기화
        }

        if(isDashing)
        {
            frameVelocity *= stats.DashDrag; // 대시 감속 적용
            dashTimeLeft -= Time.fixedDeltaTime;

            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                frameVelocity = Vector2.zero;

            }
        }
    }

    private void HandleWallSlide()
    { 
        var wasWallSliding = isWallSliding; // 변경 전 상태를 기록

        if ((onRightWall || onLeftWall) && !grounded && frameVelocity.y < 0)
        {
            bool isPushingWall = (onRightWall && frameInput.Move.x > 0) || (onLeftWall && frameInput.Move.x < 0);

            if (isPushingWall)
            {
                isWallSliding = true;
                wallStickTimer = stats.WallStickDuration;
            }
            else
            {
                if (wallStickTimer > 0)
                {
                    wallStickTimer -= Time.fixedDeltaTime;
                }
                else
                {
                    isWallSliding = false;
                }
            }
        }
        else
        {
            isWallSliding = false;
        }

        if (wasWallSliding && !isWallSliding) // 벽 슬라이딩 상태가 true에서 false로 바뀌는 '순간' 시간을 기록
        {
            frameLeftWall = time;
        }

        if (isWallSliding) // 미끄러지는 효과 적용 
        {
            frameVelocity.y = -stats.WallSlideSpeed;
        }
    }

    // Horizontal
    private void HandleDirection()
    {
        if (frameInput.Move.x != 0)
        {
            // Mathf.Sign() 함수는 입력값이 양수면 1, 음수면 -1을 반환합니다.
            // 이를 이용하여 캐릭터의 localScale.x 값을 1 또는 -1로 만들어 방향을 뒤집습니다.
            transform.localScale = new Vector3(Mathf.Sign(frameInput.Move.x), 1, 1);
        }
        // -----------------------------------------

        // 기존 이동 로직 (수정 없음)
        if (frameInput.Move.x == 0)
        {
            var deceleration = grounded ? stats.GroundDeceleration : stats.AirDeceleration;
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, frameInput.Move.x * stats.MaxSpeed, stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    // Gravity
    private void HandleGravity()
    {
        if(isDashing || isWallSliding) return;

        if (grounded && frameVelocity.y <= 0f)
        {
            frameVelocity.y = stats.GroundingForce;
        }
        else
        {
            var inAirGravity = stats.FallAcceleration;
            if (endedJumpEarly && frameVelocity.y > 0) inAirGravity *= stats.JumpEndEarlyGravityModifier;
            frameVelocity.y = Mathf.MoveTowards(frameVelocity.y, -stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    private void ApplyMovement() => rb.linearVelocity = frameVelocity;

    #endregion
}

// 인터페이스와 구조체는 클래스 밖, 네임스페이스 안에 둡니다.
public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public bool DashDown;
    public Vector2 Move;
}

public interface IPlayerController
{
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public Vector2 FrameInput { get; }
}