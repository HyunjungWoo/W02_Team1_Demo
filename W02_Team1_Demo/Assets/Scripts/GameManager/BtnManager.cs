using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements.Experimental;

public class BtnManager : MonoBehaviour
{
    [Header("재시작할 씬 이름")]
    public string sceneName = "SampleScene";

    public void RestartGame()
    {
        // 상태 리셋
        GameManager.IsDead = false;
        GameManager.IsCleared = false;
        GameManager.IsPlaying = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);

        var gs = FindObjectOfType<GameManager>();
        if (gs != null)
        {
            if (gs.pauseUI) gs.pauseUI.SetActive(false);
            if (gs.deathUI) gs.deathUI.SetActive(false);
            if (gs.clearUI) gs.clearUI.SetActive(false);
        }

        //씬 로드
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogError("씬 이름이 설정되지 않았습니다.");
    }

}
