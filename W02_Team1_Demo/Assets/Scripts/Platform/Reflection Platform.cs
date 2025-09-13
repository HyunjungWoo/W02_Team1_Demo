using UnityEngine;

public enum ReflectionSurface
{
    Floor,
    Ceiling,
    LeftWall,
    RightWall
}

public class ReflectionPlatform : Platform
{
    [SerializeField] private ReflectionSurface surfaceType = ReflectionSurface.Floor;

    public Vector2 GetSurfaceNormal()
    {
        switch (surfaceType)
        {
            case ReflectionSurface.Floor: return Vector2.up;
            case ReflectionSurface.Ceiling: return Vector2.down;
            case ReflectionSurface.LeftWall: return Vector2.right;
            case ReflectionSurface.RightWall: return Vector2.left;
        }
        return Vector2.up;
    }
}
