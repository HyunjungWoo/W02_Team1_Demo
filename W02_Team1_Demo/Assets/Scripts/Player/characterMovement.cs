using UnityEngine;
using UnityEngine.InputSystem;
public class ChracterMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private CharacterGround ground;

    [Header("Movement Stats")]
    [SerializeField, Range(0f, 20f)] public float  maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] public float maxAcceleration = 52f;
    [SerializeField, Range(0f, 100f)] public float maxDecceleration = 52f;
    [SerializeField, Range(0f, 100f)] public float maxTurnSpeed = 80f;
    [SerializeField, Range(0f, 100f)] public float maxAirAcceleration = 52f;
    [SerializeField, Range(0f, 100f)] public float maxAirDeceleration = 52f;
    [SerializeField, Range(0f, 100f)] public float maxAirTurnSpeed = 80f;

    [Header("Calculations")]
    private float   directionX;
    private Vector2 desiredVelocity;
    private Vector2 velocity;
    private float   maxSpeedChange;
    private float   acceleration;
    private float   deceleration;
    private float   turnSpeed;

    [Header("Current State")]
    private bool onGround;
    private bool pressingKey;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ground = GetComponent<CharacterGround>();
    }
    public void OnMovement(InputAction.CallbackContext context)
    {
        directionX = context.ReadValue<float>();
    }

    // Update is called once per frame
    void Update()
    {
        if (directionX != 0)
        {
            transform.localScale = new Vector3(directionX > 0 ? 1 : -1, 1, 1);
            pressingKey = true;
        }
        else
        {
            pressingKey = false;
        }
    }

    private void FixedUpdate() // 물리(Physics) 연산은 FixedUpdate에서 처리
    {
        onGround = ground.GetOnGround(); // 바닥에 닿아있는지 확인
        velocity = rb.linearVelocity; // 현재 속도

        acceleration = onGround ? maxAcceleration : maxAirAcceleration; // 바닥->가속도 , 공중 -> 공중가속도
        deceleration = onGround ? maxDecceleration : maxAirDeceleration; // 바닥->감속도 , 공중 -> 공중감속도
        turnSpeed = onGround ? maxTurnSpeed : maxAirTurnSpeed; // 바닥->회전속도 , 공중 -> 공중회전속도

        if (pressingKey) // 키를 누르고 있을 때 
        {
            if (Mathf.Sign(directionX) != Mathf.Sign(velocity.x))
            {
                maxSpeedChange = turnSpeed * Time.deltaTime;    // 방향전환
            }
            else
            {
                maxSpeedChange = acceleration * Time.deltaTime; // 가속
            }
        }
        else
        {
            maxSpeedChange = deceleration * Time.deltaTime;     // 감속
        }

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange); // 현재속도 -> 목표속도
        rb.linearVelocity = velocity; // 속도 적용
    }
}
