using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToStage2 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ChangeScene();
        }
    }
    public void ChangeScene()
    {
        SceneManager.LoadScene(3);
    }
}
