using UnityEngine;

public class CharacterGround : MonoBehaviour
{
    private bool onGround;

    [Header("Collider Settings")]
    [SerializeField] private float groundLength = 0.95f;
    [SerializeField] private Vector3 colliderOffset;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask groundLayer;

    private void Update()
    {
        onGround = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer) || Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = onGround ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position + colliderOffset, transform.position + colliderOffset + Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position - colliderOffset, transform.position - colliderOffset + Vector3.down * groundLength);
    }

    public bool GetOnGround()
    {
        return onGround;
    }
}