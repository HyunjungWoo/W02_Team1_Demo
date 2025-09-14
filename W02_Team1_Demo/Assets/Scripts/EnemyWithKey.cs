using UnityEngine;

public class EnemyWithKey : MonoBehaviour
{
    // 이 몬스터를 관리하는 LevelManager에 대한 참조
    [SerializeField] private LevelManager levelManager;

    // 몬스터가 죽을 때 호출되는 함수 (예시)
    public void Die()
    {
        if (levelManager != null)
        {
            // LevelManager에게 자신이 죽었음을 알립니다.
            levelManager.OnEnemyDied();
        }
        Destroy(gameObject);
    }
}
