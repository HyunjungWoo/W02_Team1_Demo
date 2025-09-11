using UnityEngine;



public class movementLimiter : MonoBehaviour
{
    public static movementLimiter instance;

    [SerializeField] bool _initialCharacterCanMove = true;
    public bool characterCanMove;

    private void OnEnable()
    {
        instance = this;
    }

    private void Start()
    {
        characterCanMove = _initialCharacterCanMove;
    }
}
