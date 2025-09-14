using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // 인스펙터에 열쇠 프리팹과 생성 위치를 할당
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Transform keySpawnPosition;

    // 인스펙터에 4마리의 특정 몬스터들을 직접 할당
    [SerializeField] private Enemy[] specificEnemies;

    // 죽은 몬스터 수를 추적
    private int deadEnemyCount = 0;

    void Awake()
    {
        // 몬스터들이 LevelManager를 참조하도록 연결
        foreach (Enemy enemy in specificEnemies)
        {
            if (enemy != null)
            {
                // Enemy 스크립트의 levelManager 변수에 자기 자신을 할당
                // 이 부분을 Enemy 스크립트에 public 변수로 만들거나 SetManager() 함수를 만들어서 연결하면 됩니다.
            }
        }
    }

    // 몬스터가 죽었을 때 호출
    public void OnEnemyDied()
    {
        deadEnemyCount++;
        Debug.Log("죽은 몬스터 수: " + deadEnemyCount);

        // 죽은 몬스터 수가 4명과 같으면 열쇠 생성
        if (deadEnemyCount >= specificEnemies.Length)
        {
            SpawnKey();
        }
    }

    private void SpawnKey()
    {
        if (keyPrefab != null && keySpawnPosition != null)
        {
            Instantiate(keyPrefab, keySpawnPosition.position, Quaternion.identity);
            Debug.Log("열쇠 생성!");
        }
        else
        {
            Debug.LogError("열쇠 프리팹 또는 위치가 할당되지 않았습니다.");
        }
    }
}