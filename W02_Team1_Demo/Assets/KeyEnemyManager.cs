using UnityEngine;
using UnityEngine.SceneManagement;

public class KeyEnemyManager : MonoBehaviour
{
    public static KeyEnemyManager Instance;

    private GameObject keyItemObject;
    private bool[] enemyAliveStates;
    private Transform[] enemies;
    private bool allEnemiesDefeated = false;

    void Awake()
    {
        // 실행 순서 설정으로 인해 이 Awake 함수는 다른 스크립트들보다 먼저 호출됩니다.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEnemies();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void InitializeEnemies()
    {
        enemies = new Transform[transform.childCount];
        enemyAliveStates = new bool[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            enemies[i] = transform.GetChild(i);
            enemyAliveStates[i] = true;
        }
        Debug.Log($"[KeyEnemyManager] {transform.childCount}명의 적 초기화 완료.");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[KeyEnemyManager] 씬 '{scene.name}' 로드됨.");

        keyItemObject = GameObject.FindWithTag("Key");

        if (keyItemObject == null)
        {
            Debug.LogError("[KeyEnemyManager] 씬에서 'KeyItem' 태그를 가진 오브젝트를 찾을 수 없습니다!");
            return;
        }

        ApplyEnemyStates();

        // 실행 순서가 빨라졌기 때문에, 이 로직은 첫 프레임이 그려지기 전에 실행됩니다.
        if (allEnemiesDefeated)
        {
            keyItemObject.SetActive(true);
            Debug.Log("[KeyEnemyManager] 모든 적이 이미 죽었으므로 열쇠를 켭니다.");
        }
        else
        {
            keyItemObject.SetActive(false); // 완벽한 타이밍에 열쇠를 숨깁니다.
            Debug.Log("[KeyEnemyManager] 아직 남은 적이 있으므로 열쇠를 숨깁니다.");
        }
    }

    public void RecordEnemyDeath(int enemyIndex)
    {
        if (enemyIndex >= 0 && enemyIndex < enemyAliveStates.Length)
        {
            enemyAliveStates[enemyIndex] = false;
            Debug.Log($"<color=red>[KeyEnemyManager] {enemyIndex}번 적의 죽음 기록됨.</color>");

            CheckIfAllEnemiesAreDead();
        }
    }

    public void CheckIfAllEnemiesAreDead()
    {
        foreach (bool isAlive in enemyAliveStates)
        {
            if (isAlive)
            {
                Debug.Log("나 살아있슈");

                return;
            }
        }

        Debug.Log("<color=cyan>[KeyEnemyManager] 모든 적을 처치했습니다! 열쇠를 활성화합니다.</color>");
        allEnemiesDefeated = true;

        if (allEnemiesDefeated)
        {
            Debug.Log("열쇠 켜짐");
            keyItemObject.SetActive(true);
            return;
        }

    }

    void ApplyEnemyStates()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
            {
                enemies[i].gameObject.SetActive(enemyAliveStates[i]);
            }
        }
    }
}