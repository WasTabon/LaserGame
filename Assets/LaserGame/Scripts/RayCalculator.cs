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

    public static List<RaySegment> Calculate(GridSystem grid, LaserEmitter emitter, List<MirrorElement> mirrors)
    {
        var segments = new List<RaySegment>();
        if (grid == null || emitter == null) return segments;

        var mirrorMap = new Dictionary<Vector2Int, MirrorElement>();
        if (mirrors != null)
        {
            for (int i = 0; i < mirrors.Count; i++)
            {
                var m = mirrors[i];
                if (m != null) mirrorMap[m.cell] = m;
            }
        }

        Vector2Int curCell = emitter.cell;
        Vector2Int dir = emitter.direction;
        Vector2 segmentStart = emitter.GetExitLocalPos(grid);

        int safety = MaxBounces;
        while (safety-- > 0)
        {
            Vector2Int next = curCell + dir;

            if (!grid.IsInBounds(next))
            {
                Vector2 toLocal = grid.GetCellLocalPos(curCell) + new Vector2(dir.x, dir.y) * grid.CellSize * 0.5f;
                segments.Add(new RaySegment { fromLocal = segmentStart, toLocal = toLocal });
                break;
            }

            if (mirrorMap.TryGetValue(next, out var mirror))
            {
                Vector2 mirrorCenter = grid.GetCellLocalPos(next);
                segments.Add(new RaySegment { fromLocal = segmentStart, toLocal = mirrorCenter });

                dir = MirrorReflection.Reflect(dir, mirror.rotationStep);
                curCell = next;
                segmentStart = mirrorCenter;
                continue;
            }

            curCell = next;
        }

        return segments;
    }
}
