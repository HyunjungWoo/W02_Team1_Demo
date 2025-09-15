using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToStage3 : MonoBehaviour
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
        SceneManager.LoadScene(4);  // Stage3의 넘버는 4
    }
}
