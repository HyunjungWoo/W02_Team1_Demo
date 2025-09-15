using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // 1. 자기 자신을 담을 static 변수 선언 (싱글톤의 핵심)
    public static LevelManager Instance { get; private set; }
    // 인스펙터에 열쇠 프리팹과 생성 위치를 할당
    [SerializeField] private GameObject Key;

    // 죽은 몬스터 수를 추적
    private int deadEnemyCount = 0;

    void Awake()
    {
        // 이미 다른 인스턴스가 있는지 확인
        if (Instance != null && Instance != this)
        {
            // 이미 존재한다면 이 오브젝트는 파괴
            Destroy(gameObject);
        }
        else
        {
            // 이 인스턴스를 static 변수에 할당
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    // 몬스터가 죽었을 때 호출
    public void OnEnemyDied()
    {
        
        Debug.Log("쥬금");
        deadEnemyCount++;
        Debug.Log("죽은 몬스터 수: " + deadEnemyCount);

        // 죽은 몬스터 수가 4명과 같으면 열쇠 생성
        if (deadEnemyCount >= 4)
        {
            SpawnKey();
        }
    }

    private void SpawnKey()
    {
        Debug.Log("키 소환!!!!!!!!!!!!");

        Key.SetActive(true);
    }
}