using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelDefinition
{
    public int cols = 5;
    public int rows = 5;
    public Vector2Int emitterCell = new Vector2Int(0, 2);
    public Vector2Int emitterDir = new Vector2Int(1, 0);
    public List<MirrorPlacement> mirrors = new List<MirrorPlacement>();
    public List<Vector2Int> batteries = new List<Vector2Int>();
    public List<Vector2Int> energyStars = new List<Vector2Int>();
    public List<Vector2Int> walls = new List<Vector2Int>();
    public int maxMovesForThreeStars = 3;
}

[Serializable]
public struct MirrorPlacement
{
    public Vector2Int cell;
    public int initialRotationStep;
}
