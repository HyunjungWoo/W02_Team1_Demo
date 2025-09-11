using UnityEngine;

public class Item : MonoBehaviour
{
    float degreesPerSecond = 180.0f;

    

    protected virtual void Rotate()
    {
        transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.World);
    }

    // Update is called once per frame
    void Update()
    {
        Rotate();        
    }
}
