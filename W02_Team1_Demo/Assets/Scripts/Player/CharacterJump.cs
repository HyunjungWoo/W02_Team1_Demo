using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CharacterJump : MonoBehaviour
{
    private Rigidbody2D     rb;
    private CharacterGround ground;
    private Vector2         velocity;

    [Header("Jumping Stats")]
    [SerializeField, Range(2f, 5.5f)]    public float   jumpHeight = 7.3f;
    [SerializeField, Range(0.2f, 1.25f)] public float   timeToJumpApex = 0.5f; // 점프 정점에 도달하는 시간
    [SerializeField, Range(0f, 5f)]      public float   upwardMovementMultiplier = 1f;// 점프 상승 시 중력 조절
    [SerializeField, Range(1f, 10f)]     public float   downwardMovementMultiplier = 6.17f; // 낙하 시 중력 조절
    [SerializeField, Range(0, 1)]        public int     maxAirJumps = 0; // 이중 점프 횟수

    [Header("Options")]
    public bool variablejumpHeight; // 점프 높이 가변 옵션
    [SerializeField, Range(1f, 10f)]  public float jumpCutOff;
    [SerializeField]                  public float speedLimit = 100f; // 낙하 속도 제한
    [SerializeField, Range(0f, 0.3f)] public float coyoteTime = 0.15f;
    [SerializeField, Range(0f, 0.3f)] public float jumpBuffer = 0.15f;

    [Header("Calculations")]
    private float jumpSpeed;
    private float defaultGravityScale;
    private float gravMultiplier;

    [Header("Current State")]
    private bool    canJumpAgain = false;
    private bool    desiredJump;
    private float   jumpBufferCounter;
    private float   coyoteTimeCounter = 0;
    private bool    pressingJump;
    private bool    onGround;
    private bool    currentlyJumping;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ground = GetComponent<CharacterGround>();
        defaultGravityScale = 1f; // 기본 중력 스케일 설정
    }

    private void Update()
    {
        SetPhysics();
        onGround = ground.GetOnGround();

        if (jumpBuffer > 0 && desiredJump)
        {
            jumpBufferCounter += Time.deltaTime;
            if (jumpBufferCounter > jumpBuffer)
            {
                desiredJump = false;
                jumpBufferCounter = 0;
            }
        }

        if (!currentlyJumping && !onGround)
        {
            coyoteTimeCounter += Time.deltaTime;
        }
        else
        {
            coyoteTimeCounter = 0;
        }
    }

    private void FixedUpdate()
    {
        velocity = rb.linearVelocity;

        if (desiredJump)
        {
            DoAJump();
            rb.linearVelocity = velocity;
            return;
        }

        CalculateGravity();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            desiredJump = true;
            pressingJump = true;
        }

        if (context.canceled)
        {
            pressingJump = false;
        }
    }

    private void SetPhysics()
    {
        Vector2 newGravity = new Vector2(0, (-2 * jumpHeight) / (timeToJumpApex * timeToJumpApex));
        rb.gravityScale = (newGravity.y / Physics2D.gravity.y) * gravMultiplier;
    }

    private void CalculateGravity()
    {
        if (rb.linearVelocity.y > 0.01f)
        {
            if (onGround)
            {
                gravMultiplier = defaultGravityScale;
            }
            else
            {
                if (variablejumpHeight)
                {
                    gravMultiplier = pressingJump && currentlyJumping ? upwardMovementMultiplier : jumpCutOff;
                }
                else
                {
                    gravMultiplier = upwardMovementMultiplier;
                }
            }
        }
        else if (rb.linearVelocity.y < -0.01f)
        {
            gravMultiplier = onGround ? defaultGravityScale : downwardMovementMultiplier;
        }
        else
        {
            if (onGround) currentlyJumping = false;
            gravMultiplier = defaultGravityScale;
        }
        rb.linearVelocity = new Vector2(velocity.x, Mathf.Clamp(velocity.y, -speedLimit, 100));
    }

    private void DoAJump()
    {
        if (onGround || (coyoteTimeCounter > 0.03f && coyoteTimeCounter < coyoteTime) || canJumpAgain)
        {
            desiredJump = false;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;

            canJumpAgain = (maxAirJumps > 0 && canJumpAgain == false);

            jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * rb.gravityScale * jumpHeight);

            if (velocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
            else if (velocity.y < 0f)
            {
                jumpSpeed += Mathf.Abs(rb.linearVelocity.y);
            }

            velocity.y += jumpSpeed;
            currentlyJumping = true;
        }

        if (jumpBuffer == 0)
        {
            desiredJump = false;
        }
    }

}
