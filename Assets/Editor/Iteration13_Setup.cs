using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Iteration13_Setup
{
    private const string DatabasePath = "Assets/LaserGame/Levels/LevelsDatabase.asset";

    [MenuItem("LaserGame/Iteration 13/Update Levels To Very Hard")]
    public static void UpdateLevelsToVeryHard()
    {
        var db = AssetDatabase.LoadAssetAtPath<LevelsDatabaseSO>(DatabasePath);
        if (db == null || db.uniqueConfigs == null || db.uniqueConfigs.Length < 5)
        {
            Debug.LogWarning("[Iteration 13] LevelsDatabase not found or incomplete. Run Iteration 08 setup first.");
            return;
        }

        if (db.uniqueConfigs[1] != null) db.uniqueConfigs[1].definition = BuildLevel2VeryHard();
        if (db.uniqueConfigs[2] != null) db.uniqueConfigs[2].definition = BuildLevel3VeryHard();
        if (db.uniqueConfigs[3] != null) db.uniqueConfigs[3].definition = BuildLevel4VeryHard();
        if (db.uniqueConfigs[4] != null) db.uniqueConfigs[4].definition = BuildLevel5VeryHard();

        for (int i = 0; i < db.uniqueConfigs.Length; i++)
        {
            if (db.uniqueConfigs[i] != null) EditorUtility.SetDirty(db.uniqueConfigs[i]);
        }
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log("[Iteration 13] Levels 2-5 updated to VERY HARD versions. Level 1 untouched.");
    }

    private static LevelDefinition BuildLevel2VeryHard()
    {
        return new LevelDefinition
        {
            cols = 7, rows = 7,
            emitterCell = new Vector2Int(0, 3),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(1, 3), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(1, 5), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(3, 5), initialRotationStep = 0 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(1, 4),
                new Vector2Int(3, 1)
            },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(2, 5)
            },
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 3
        };
    }

    private static LevelDefinition BuildLevel3VeryHard()
    {
        return new LevelDefinition
        {
            cols = 7, rows = 7,
            emitterCell = new Vector2Int(0, 3),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 3), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(2, 5), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(5, 5), initialRotationStep = 0 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(5, 0)
            },
            walls = new List<Vector2Int>
            {
                new Vector2Int(4, 3),
                new Vector2Int(3, 1)
            },
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 3),
                new Vector2Int(5, 3)
            },
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 3
        };
    }

    private static LevelDefinition BuildLevel4VeryHard()
    {
        return new LevelDefinition
        {
            cols = 7, rows = 7,
            emitterCell = new Vector2Int(0, 3),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 3), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(2, 5), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(5, 5), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(5, 3), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(4, 5),
                new Vector2Int(5, 4)
            },
            walls = new List<Vector2Int>
            {
                new Vector2Int(4, 3)
            },
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 3),
                new Vector2Int(3, 3)
            },
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 4
        };
    }

    private static LevelDefinition BuildLevel5VeryHard()
    {
        return new LevelDefinition
        {
            cols = 7, rows = 7,
            emitterCell = new Vector2Int(0, 3),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 2), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(5, 2), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(5, 3),
                new Vector2Int(3, 2),
                new Vector2Int(5, 4)
            },
            walls = new List<Vector2Int>
            {
                new Vector2Int(0, 5),
                new Vector2Int(6, 1)
            },
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 3),
                new Vector2Int(4, 2)
            },
            splitters = new List<SplitterPlacement>
            {
                new SplitterPlacement { cell = new Vector2Int(2, 3), rotationStep = 1 }
            },
            maxMovesForThreeStars = 2
        };
    }
}
