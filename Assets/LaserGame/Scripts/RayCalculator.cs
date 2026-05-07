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
    private const int MaxTotalSteps = 256;

    private struct BeamState
    {
        public Vector2Int curCell;
        public Vector2Int dir;
        public Vector2 segmentStart;
        public int splittersHit;
    }

    public static RayResult Calculate(GridSystem grid, LaserEmitter emitter, List<MirrorElement> mirrors, List<Vector2Int> walls, List<SplitterElement> splitters)
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

        var wallSet = new HashSet<Vector2Int>();
        if (walls != null)
        {
            for (int i = 0; i < walls.Count; i++) wallSet.Add(walls[i]);
        }

        var splitterMap = new Dictionary<Vector2Int, SplitterElement>();
        if (splitters != null)
        {
            for (int i = 0; i < splitters.Count; i++)
            {
                var s = splitters[i];
                if (s != null) splitterMap[s.cell] = s;
            }
        }

        var stack = new Stack<BeamState>();
        stack.Push(new BeamState
        {
            curCell = emitter.cell,
            dir = emitter.direction,
            segmentStart = emitter.GetExitLocalPos(grid),
            splittersHit = 0
        });
        result.visitedCells.Add(emitter.cell);

        int totalSteps = 0;
        while (stack.Count > 0 && totalSteps < MaxTotalSteps)
        {
            var beam = stack.Pop();

            while (totalSteps < MaxTotalSteps)
            {
                totalSteps++;
                Vector2Int next = beam.curCell + beam.dir;

                if (!grid.IsInBounds(next))
                {
                    Vector2 toLocal = grid.GetCellLocalPos(beam.curCell) + new Vector2(beam.dir.x, beam.dir.y) * grid.CellSize * 0.5f;
                    result.segments.Add(new RaySegment { fromLocal = beam.segmentStart, toLocal = toLocal });
                    break;
                }

                if (wallSet.Contains(next))
                {
                    Vector2 toLocal = grid.GetCellLocalPos(beam.curCell) + new Vector2(beam.dir.x, beam.dir.y) * grid.CellSize * 0.55f;
                    result.segments.Add(new RaySegment { fromLocal = beam.segmentStart, toLocal = toLocal });
                    break;
                }

                result.visitedCells.Add(next);

                if (splitterMap.TryGetValue(next, out var splitter))
                {
                    Vector2 splitterCenter = grid.GetCellLocalPos(next);
                    result.segments.Add(new RaySegment { fromLocal = beam.segmentStart, toLocal = splitterCenter });

                    if (beam.splittersHit < 8)
                    {
                        Vector2Int perpDir = MirrorReflection.Reflect(beam.dir, splitter.rotationStep);
                        stack.Push(new BeamState
                        {
                            curCell = next,
                            dir = perpDir,
                            segmentStart = splitterCenter,
                            splittersHit = beam.splittersHit + 1
                        });
                    }

                    beam.curCell = next;
                    beam.segmentStart = splitterCenter;
                    beam.splittersHit++;
                    continue;
                }

                if (mirrorMap.TryGetValue(next, out var mirror))
                {
                    Vector2 mirrorCenter = grid.GetCellLocalPos(next);
                    result.segments.Add(new RaySegment { fromLocal = beam.segmentStart, toLocal = mirrorCenter });

                    beam.dir = MirrorReflection.Reflect(beam.dir, mirror.rotationStep);
                    beam.curCell = next;
                    beam.segmentStart = mirrorCenter;
                    continue;
                }

                beam.curCell = next;
            }
        }

        return result;
    }
}
