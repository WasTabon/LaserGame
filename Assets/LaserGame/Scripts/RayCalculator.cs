using System.Collections.Generic;
using UnityEngine;

public struct RaySegment
{
    public Vector2 fromLocal;
    public Vector2 toLocal;
}

public struct RayResult
{
    public List<RaySegment> segments;
    public HashSet<Vector2Int> visitedCells;
}

public static class RayCalculator
{
    private const int MaxBounces = 64;

    public static RayResult Calculate(GridSystem grid, LaserEmitter emitter, List<MirrorElement> mirrors)
    {
        var result = new RayResult
        {
            segments = new List<RaySegment>(),
            visitedCells = new HashSet<Vector2Int>()
        };
        if (grid == null || emitter == null) return result;

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
        result.visitedCells.Add(curCell);

        int safety = MaxBounces;
        while (safety-- > 0)
        {
            Vector2Int next = curCell + dir;

            if (!grid.IsInBounds(next))
            {
                Vector2 toLocal = grid.GetCellLocalPos(curCell) + new Vector2(dir.x, dir.y) * grid.CellSize * 0.5f;
                result.segments.Add(new RaySegment { fromLocal = segmentStart, toLocal = toLocal });
                break;
            }

            result.visitedCells.Add(next);

            if (mirrorMap.TryGetValue(next, out var mirror))
            {
                Vector2 mirrorCenter = grid.GetCellLocalPos(next);
                result.segments.Add(new RaySegment { fromLocal = segmentStart, toLocal = mirrorCenter });

                dir = MirrorReflection.Reflect(dir, mirror.rotationStep);
                curCell = next;
                segmentStart = mirrorCenter;
                continue;
            }

            curCell = next;
        }

        return result;
    }
}
