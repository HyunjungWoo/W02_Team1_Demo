using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance;
    private Vector3 lastCheckpointPosition;
    private int stage;
    [SerializeField] private PlayerController player;
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
    }

    public void TouchCheckPoint(int stage, Vector3 checkPointPos)
    {
        lastCheckpointPosition = checkPointPos;
        Debug.Log("새로운 체크포인트 저장됨: " + lastCheckpointPosition);
    }

    public void PlayerDied()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void RespawnAtCheckpoint(GameObject playerObject)
    {
        playerObject.transform.position = lastCheckpointPosition;

        // 추가적으로 플레이어의 상태 초기화 (예: 체력, 속도 등)
        // PlayerController playerScript = playerObject.GetComponent<PlayerController>();
        // if (playerScript != null)
        // {
        //     playerScript.ResetState();
        // }
    }
}
