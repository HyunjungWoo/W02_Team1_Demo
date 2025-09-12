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
    [SerializeField] private ScriptableStats _stats;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private FrameInput _frameInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;
    private float _time;
    #endregion

    #region 대시 관련 변수
    private bool    _isDashing;
    private float   _dashTimeLeft;
    private float   _dashCooldownTimer;

    #endregion

    #region 벽타기 관련 변수
    private bool _onWall;
    private bool _isWallSliding;
    private int  _wallDirection;
    #endregion

    #region 점프하기 변수 
    // Jumping
    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;
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
    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    #endregion

    private void Awake()
    {
        // Tarodev의 Awake() 내용: 필수 컴포넌트 초기화
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

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
        _time += Time.deltaTime;
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
        _frameInput = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
            JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
            DashDown = Input.GetButton("Fire3") || Input.GetKeyDown(KeyCode.X),
            Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
        };

        if (_stats.SnapInput)
        {
            _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
            _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
        }

        if (_frameInput.JumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
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
                _dashCooldownTimer = 0;
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
        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(Vector2.up * selfForce, ForceMode2D.Impulse);

        currentKunai = null;
    }

    #endregion

    #region Tarodev 이동 로직 

    // Collisions
    private float _frameLeftGrounded = float.MinValue;
    private bool _grounded;

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // 땅&천장 충돌 검사 
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

        if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        }
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

    private void HandleJump()
    {
        if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.linearVelocity.y > 0) _endedJumpEarly = true;
        if (!_jumpToConsume && !HasBufferedJump) return;
        if (_grounded || CanUseCoyote) ExecuteJump();
        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _frameVelocity.y = _stats.JumpPower;
        Jumped?.Invoke();
    }

    private void HandleDashCooldown()
    {
        if (_dashCooldownTimer > 0)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void HandleDash()
    {
        // 대쉬 시작 조건 확인
        if (_frameInput.DashDown && _dashCooldownTimer <= 0 && !_isDashing)
        {
            Vector2 dashDirection = _frameInput.Move;
            if (dashDirection == Vector2.zero)
            {
                dashDirection = new Vector2(transform.localScale.x, 0); // 기본적으로 캐릭터가 바라보는 방향으로 대시

            }

            _isDashing = true;
            _dashTimeLeft = _stats.DashDuration;
            _frameVelocity = dashDirection.normalized * _stats.DashPower; // 대시 속도 설정
            _dashCooldownTimer = _stats.DashCooldown; // 쿨타임 초기화
        }

        if(_isDashing)
        {
            _dashTimeLeft -= Time.fixedDeltaTime;

            if (_dashTimeLeft <= 0)
            {
                _isDashing = false;
                _frameVelocity = Vector2.zero;

            }
        }
    }
    // Horizontal
    private void HandleDirection()
    {
        if (_frameInput.Move.x != 0)
        {
            // Mathf.Sign() 함수는 입력값이 양수면 1, 음수면 -1을 반환합니다.
            // 이를 이용하여 캐릭터의 localScale.x 값을 1 또는 -1로 만들어 방향을 뒤집습니다.
            transform.localScale = new Vector3(Mathf.Sign(_frameInput.Move.x), 1, 1);
        }
        // -----------------------------------------

        // 기존 이동 로직 (수정 없음)
        if (_frameInput.Move.x == 0)
        {
            var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    // Gravity
    private void HandleGravity()
    {
        if(_isDashing) return;

        if (_grounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = _stats.GroundingForce;
        }
        else
        {
            var inAirGravity = _stats.FallAcceleration;
            if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;

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