using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviour
{

    [Header("게임 상태 관련")]
    public static bool IsPlaying = true;
    public static bool IsDead = false;
    public static bool IsCleared = false;

    [Header("Optional UI")]
    public GameObject pauseUI;
    public GameObject deathUI;
    public GameObject clearUI;
    public Animator blackPanelAnimator; // 인스펙터에 연결할 Animator 변수
    public static event System.Action<bool> OnPauseChanged;
    public static InGameManager Instance;
    private Vector3 lastCheckpointPosition;
    private int stage;
    [SerializeField] private GameObject player;

   
    /// 준홍 추가
    void Start()
    {
        AndTime();
        UpdateUIs();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsDead && !IsCleared)
        {
            IsPlaying = !IsPlaying;
            AndTime();
            OnPauseChanged?.Invoke(IsPlaying); // 상태 변경 시 이벤트 호출
        }

        UpdateUIs();
    }

    void AndTime()
    {
        Time.timeScale = IsPlaying ? 1f : 0f;
    }


    void UpdateUIs()
    {
        if (pauseUI) pauseUI.SetActive(!IsPlaying && !IsDead && !IsCleared);
        if (deathUI) deathUI.SetActive(IsDead);
        if (clearUI) clearUI.SetActive(IsCleared);
    }
    // 준홍 추가 여기까지
    private void Awake()
    {
      
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            // 이미 다른 인스턴스가 존재하면 현재 오브젝트를 파괴
            Destroy(this.gameObject);
        }

        // 나중에 꼭 바꿀것.
        stage = 1;
        lastCheckpointPosition = new Vector3( -0, 3, 0);
    }

    public void TouchCheckPoint(Vector3 checkPointPos)
    {
        lastCheckpointPosition = checkPointPos;
        Debug.Log("새로운 체크포인트 저장됨: " + lastCheckpointPosition);
    }
    void OnEnable()
    {
        // 씬 로드가 완료되면 OnSceneLoaded 함수를 호출하도록 등록합니다.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        // 스크립트가 비활성화되면 이벤트 등록을 해제합니다.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로딩이 완료된 후, 새로운 씬에 생성된 플레이어를 찾습니다.
        player = GameObject.FindGameObjectWithTag("Player");
        IsDead = false;
    }
    public void PlayerDied()
    {
        if (IsDead) return;
        IsDead = true;
        StartCoroutine(RestartAfterAnimation());
    }

    private IEnumerator RestartAfterAnimation()
    {
        Wipe.Instance.animator.Rebind();
        Wipe.Instance.animator.Update(0f);
        Wipe.Instance.StartCloseWipe(player.transform.position);
        float animationDuration = 1f; // 예시: 1.5초
        yield return new WaitForSeconds(animationDuration);
        // 3. 씬 재시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void RespawnAtCheckpoint()
    {
        // Check if the player object still exists before trying to use it.
        if (player != null)
        {
            // Player object still exists, proceed with respawn logic.
            // 예를 들어:
            player.transform.position = lastCheckpointPosition;
        }
        else
        {
            // Player object is destroyed.
            // Re-instantiate the player or handle the null case.
            Debug.Log("Player object is null. Cannot respawn.");
            // 필요하다면, 여기서 플레이어를 다시 생성하는 코드를 추가하세요.
            // Instantiate(playerPrefab, checkpointPosition, Quaternion.identity);
        }
    }
}
