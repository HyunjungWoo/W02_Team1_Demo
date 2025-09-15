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

    public static event System.Action<bool> OnPauseChanged;
    public static InGameManager Instance;
    private Vector3 lastCheckpointPosition;
    private int stage;
    [SerializeField] private GameObject player;
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

    public void TouchCheckPoint(int stage, Vector3 checkPointPos)
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
    }
    public void PlayerDied()
    {
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
