using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private bool onWall;
    private bool isWallSliding;
    private int  wallDirection;
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
    #endregion

    #region 인터페이스 구현
    public Vector2 FrameInput => frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
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
        kunaiInstance.GetComponent<Rigidbody2D>().AddForce(throwDirection * thorwForce, ForceMode2D.Impulse);
    }

    private void WarpToKunai()
    {
        Vector3 warpPosition = currentKunai.transform.position;
        Destroy(currentKunai.gameObject); // 워프 후 쿠나이는 파괴
        transform.position = warpPosition;

        // 반동 효과
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Vector2.up * selfForce, ForceMode2D.Impulse);

        currentKunai = null;
    }

    #endregion

    #region Tarodev 이동 로직 

    // Collisions
    private float frameLeftGrounded = float.MinValue;
    private bool grounded;

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

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

        Physics2D.queriesStartInColliders = cachedQueryStartInColliders;
    }

    private bool HasBufferedJump => bufferedJumpUsable && time < timeJumpWasPressed + stats.JumpBuffer;
    private bool CanUseCoyote => coyoteUsable && !grounded && time < frameLeftGrounded + stats.CoyoteTime;

    private void HandleJump()
    {
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
            Vector2 dashDirection = frameInput.Move;
            if (dashDirection == Vector2.zero)
            {
                dashDirection = new Vector2(transform.localScale.x, 0); // 기본적으로 캐릭터가 바라보는 방향으로 대시

            }

            isDashing = true;
            dashTimeLeft = stats.DashDuration;
            frameVelocity = dashDirection.normalized * stats.DashPower; // 대시 속도 설정
            dashCooldownTimer = stats.DashCooldown; // 쿨타임 초기화
        }

        if(isDashing)
        {
            dashTimeLeft -= Time.fixedDeltaTime;

            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                frameVelocity = Vector2.zero;

            }
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
        if(isDashing) return;

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