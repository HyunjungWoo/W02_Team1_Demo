using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    // 오브젝트를 삭제할 시간을 초 단위로 설정합니다.
    public float delayInSeconds = 2.0f;

    void Start()
    {
        // 2초 뒤에 이 게임 오브젝트를 삭제하는 함수를 호출합니다.
        Destroy(gameObject, delayInSeconds);
    }
}