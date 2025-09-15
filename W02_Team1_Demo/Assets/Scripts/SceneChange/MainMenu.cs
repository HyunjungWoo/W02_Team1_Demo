using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void ChangeScene()
    {
        SceneManager.LoadScene(1);
    }
}
