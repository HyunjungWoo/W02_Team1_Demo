using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool IsPlaying = true;
public static bool IsDead = false;
public static bool IsCleared = false;

[Header("Optional UI")]
public GameObject pauseUI;
public GameObject deathUI;
public GameObject clearUI;

public static event System.Action<bool> OnPauseChanged;

// Start is called once before the first execution of Update after the MonoBehaviour is created
void Start()
{
    AndTime();
    UpdateUIs();
}

// Update is called once per frame 
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape) && !IsDead && !IsCleared)
    {
        IsPlaying = !IsPlaying;
        AndTime();
        OnPauseChanged?.Invoke(IsPlaying); // 상태 변경 시 이벤트 호출
    }

    UpdateUIs();
}

void AndTime()
{
    Time.timeScale = IsPlaying ? 1f : 0f;
}


void UpdateUIs()
{
    if (pauseUI) pauseUI.SetActive(!IsPlaying && !IsDead && !IsCleared);
    if (deathUI) deathUI.SetActive(IsDead);
    if (clearUI) clearUI.SetActive(IsCleared);
}

public static void SetDead()
{
    IsDead = true;
    IsPlaying = false;
    IsCleared = false;
    Time.timeScale = 0f;
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}

public static void SetCleared()
{
    IsCleared = true;
    IsPlaying = false;
    IsDead = false;
    Time.timeScale = 0f;
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}
}