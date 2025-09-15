using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 꼭 추가해야 합니다.

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance { get; private set; }

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

    // 매 프레임마다 입력을 확인하기 위해 Update 함수를 사용합니다.
    void Update()
    {
        // 숫자 키 1을 눌렀을 때
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene(1);
        }
        // 숫자 키 2를 눌렀을 때
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene(2);
        }
        // 숫자 키 3을 눌렀을 때
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SceneManager.LoadScene(3);
        }
        // 숫자 키 4를 눌렀을 때
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SceneManager.LoadScene(4);
        }
    }
}