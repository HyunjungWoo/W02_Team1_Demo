using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToStage2 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            InGameManager.Instance.TouchCheckPoint(new Vector3(0,0,0));
            ChangeScene();
        }
    }
    public void ChangeScene()
    {
        SceneManager.LoadScene(3);
    }
}
