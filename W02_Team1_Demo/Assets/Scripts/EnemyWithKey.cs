using UnityEngine;

public class EnemyWithKey : MonoBehaviour
{
    private void OnDestroy()
    {

        // 싱글톤 LevelManager가 씬에 존재할 경우에만 안전하게 호출
        if (LevelManager.Instance != null)
        {
            // LevelManager에 있는 OnEnemyDied 함수를 호출
            LevelManager.Instance.OnEnemyDied();
        }
    }
}
