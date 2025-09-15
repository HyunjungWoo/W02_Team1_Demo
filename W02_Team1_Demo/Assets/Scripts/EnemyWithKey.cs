using UnityEngine;

public class EnemyWithKey : MonoBehaviour
{
    // 이 적이 몇 번째 자식인지 저장할 변수
    private int myIndex;

    void Start()
    {
        // 시작할 때 자신의 인덱스 번호를 가져옴 (0부터 시작)
        myIndex = transform.GetSiblingIndex();
    }

    // 외부에서 호출할 Die 함수 (예: 체력이 0이 되었을 때)
    public void Die()
    {
        // KeyEnemyManager에게 내가 죽었다고 알림
        Debug.Log($"[EnemyWithKey] 적 오브젝트가 파괴됨. 인덱스: {myIndex}");
        KeyEnemyManager.Instance.RecordEnemyDeath(myIndex);
    }

    private void OnDestroy() // 이 오브젝트가 파괴될 때 호출되는 함수
    {
        Die();
    }

}
