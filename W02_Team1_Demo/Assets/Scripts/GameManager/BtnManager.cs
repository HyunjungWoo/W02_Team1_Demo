using UnityEngine;
using UnityEngine.SceneManagement;

public class BtnManager : MonoBehaviour
{
    /// <summary>
    /// 이름에 맞는 씬을 로드합니다. 메인 메뉴의 스테이지 선택 버튼 등에서 사용합니다.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Time.timeScale = 1f; // 혹시 게임이 멈춰있을 수 있으니 시간 흐르게 설정
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("씬 이름이 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// 메인 메뉴 씬을 로드합니다. 일시정지 메뉴의 '타이틀로' 버튼 등에서 사용합니다.
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // 씬 이름을 "MainMenu"로 고정
    }

    /// <summary>
    /// 현재 씬을 다시 시작합니다.
    /// </summary>
    public void RestartGame()
    {
        // 게임 상태 리셋
        GameManager.IsDead = false;
        GameManager.IsCleared = false;
        GameManager.IsPlaying = true;
        Time.timeScale = 1f;

        // 현재 활성화된 씬을 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 게임을 종료합니다.
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("게임을 종료합니다."); // 에디터에서는 로그만 보임
        Application.Quit();
    }
}
