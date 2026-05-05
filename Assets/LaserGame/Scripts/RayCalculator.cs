using System.Collections.Generic;
using UnityEngine;

public struct RaySegment
{
    public Vector2 fromLocal;
    public Vector2 toLocal;
}

public static class RayCalculator
{
    private const int MaxBounces = 64;

    public static List<RaySegment> Calculate(GridSystem grid, LaserEmitter emitter)
    {
        var segments = new List<RaySegment>();
        if (grid == null || emitter == null) return segments;

        Vector2Int cur = emitter.cell;
        Vector2Int dir = emitter.direction;
        Vector2 fromLocal = emitter.GetExitLocalPos(grid);

        int safety = MaxBounces;
        Vector2Int next = cur + dir;

        while (grid.IsInBounds(next) && safety-- > 0)
        {
            cur = next;
            next = cur + dir;
        }

        Vector2 toLocal = grid.GetCellLocalPos(cur) + new Vector2(dir.x, dir.y) * grid.CellSize * 0.5f;

        segments.Add(new RaySegment { fromLocal = fromLocal, toLocal = toLocal });
        return segments;
    }
}
