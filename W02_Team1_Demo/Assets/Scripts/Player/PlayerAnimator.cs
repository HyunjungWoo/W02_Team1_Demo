using UnityEngine;


public class PlayerAnimator : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sprite;

    [Header("Settings")]
    [SerializeField, Range(1f, 3f)]
    private float maxIdleSpeed = 2; // idle 애니메이션 속도

    /* 기울어질 때 애니메이션 (제거) 
    [SerializeField] private float maxTilt = 5;
    [SerializeField] private float tiltSpeed = 20; 

    */

    [Header("Particles")][SerializeField] private ParticleSystem jumpParticles; 
    [SerializeField] private ParticleSystem launchParticles;
    [SerializeField] private ParticleSystem moveParticles;
    [SerializeField] private ParticleSystem landParticles;

    /*
    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip[]  footsteps;
    */

    private IPlayerController player;
    private bool grounded;
    private ParticleSystem.MinMaxGradient  currentGradient;
    private bool isWarping = false;

    private void Awake()
    {
        //source = GetComponent<AudioSource>();
        player = GetComponentInParent<IPlayerController>();
    }

    private void OnEnable()
    {
        player.Jumped += OnJumped; // 이건 점프할 때
        player.GroundedChanged += OnGroundedChanged;  // 이건 착지할 때
        player.Warping += OnWarping;

        moveParticles.Play();
    }

    private void OnDisable()
    {
        player.Jumped -= OnJumped;
        player.GroundedChanged -= OnGroundedChanged;
        player.Warping -= OnWarping;

        moveParticles.Stop();
    }
    private void OnWarping(bool warping)
    {
        isWarping = warping;
        if (isWarping)
        {
            moveParticles.Stop();
        }
        else if (grounded)
        {
            moveParticles.Play();
        }
    }

    private void Update()
    {
        if(player == null) return;

        DetectGroundColor();

        //HandleSpriteFlip();

        //HandleIdleSpeed(); -- idle 애니메이션 속도 조절

        //HandleCharacterTilt();
    }

    private void HandleSpriteFlip()
    {
        if (player.FrameInput.x != 0) sprite.flipX = player.FrameInput.x < 0;
    }

    private void HandleIdleSpeed()
    {
        var inputStrength = Mathf.Abs(player.FrameInput.x);
        anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, maxIdleSpeed, inputStrength));
        moveParticles.transform.localScale = Vector3.MoveTowards(moveParticles.transform.localScale, Vector3.one * inputStrength, 2 * Time.deltaTime);
    }
  
    private void OnJumped()
    {
         //anim.SetTrigger(JumpKey);
         //anim.ResetTrigger(GroundedKey);

        if ( grounded) // Avoid coyote
        {
            SetColor(jumpParticles);
            SetColor(launchParticles);
             jumpParticles.Play();
        }
    }

    private void OnGroundedChanged(bool grounded_, float impact)
    {
        if(isWarping) return;

        grounded = grounded_;

        if (grounded)
        {
            DetectGroundColor();
            SetColor( landParticles);

             //anim.SetTrigger(GroundedKey);
             //source.PlayOneShot( footsteps[Random.Range(0,  footsteps.Length)]);
             moveParticles.Play();

             landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
             landParticles.Play();
        }
        else
        {
             moveParticles.Stop();
        }
    }

    private void DetectGroundColor()
    {
        var hit = Physics2D.Raycast(transform.position, Vector3.down, 2);

        if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) return;
        var color = r.color;
        currentGradient = new ParticleSystem.MinMaxGradient(color * 0.9f, color * 1.2f);
        SetColor(moveParticles);
    }

    private void SetColor(ParticleSystem ps)
    {
        var main = ps.main;
        main.startColor = currentGradient;
    }

    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
    private static readonly int JumpKey = Animator.StringToHash("Jump");
}
