using UnityEngine;

public static class MirrorReflection
{
    public static Vector2Int Reflect(Vector2Int dir, int rotationStep)
    {
        if (rotationStep == 0)
        {
            return new Vector2Int(dir.y, dir.x);
        }
        return new Vector2Int(-dir.y, -dir.x);
    }
}
