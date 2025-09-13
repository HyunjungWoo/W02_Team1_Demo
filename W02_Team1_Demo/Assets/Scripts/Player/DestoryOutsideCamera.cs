using UnityEngine;

public class DestroyOutsideCamera : MonoBehaviour
{
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        Vector3 viewPos = mainCam.WorldToViewportPoint(transform.position);

        // 화면 밖이면 제거
        if (viewPos.x < 0 || viewPos.x > 1 ||
            viewPos.y < 0 || viewPos.y > 1 ||
            viewPos.z < 0) // 카메라 뒤쪽
        {
            if (!gameObject.GetComponent<ThrowableKunai>().IsStuck())
                Destroy(gameObject);
        }
    }
}
