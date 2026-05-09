using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Iteration14_Hotfix_Cfg09
{
    private const string DatabasePath = "Assets/LaserGame/Levels/LevelsDatabase.asset";

    [MenuItem("LaserGame/Iteration 14 Hotfix/Fix Cfg09 Monster")]
    public static void FixCfg09()
    {
        var db = AssetDatabase.LoadAssetAtPath<LevelsDatabaseSO>(DatabasePath);
        if (db == null || db.uniqueConfigs == null || db.uniqueConfigs.Length < 10)
        {
            Debug.LogWarning("[Hotfix] Database not ready. Run Iteration 14 Run All first.");
            return;
        }
        if (db.uniqueConfigs[9] == null)
        {
            Debug.LogWarning("[Hotfix] Slot 9 empty.");
            return;
        }

        db.uniqueConfigs[9].definition = BuildFixedCfg09();
        EditorUtility.SetDirty(db.uniqueConfigs[9]);
        AssetDatabase.SaveAssets();
        Debug.Log("[Hotfix] Cfg09 (Monster) fixed. Solvable in 5 rotations: m1-m5 to opposite step, m6 stays.");
    }

    private static LevelDefinition BuildFixedCfg09()
    {
        return new LevelDefinition
        {
            cols = 15, rows = 15,
            emitterCell = new Vector2Int(0, 7),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 7), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(2, 11), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(5, 11), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(5, 4), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(9, 4), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(9, 11), initialRotationStep = 0 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(2, 9),
                new Vector2Int(5, 8),
                new Vector2Int(9, 7),
                new Vector2Int(13, 5)
            },
            walls = new List<Vector2Int>
            {
                new Vector2Int(4, 7),
                new Vector2Int(12, 7),
                new Vector2Int(8, 3),
                new Vector2Int(8, 9)
            },
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 7),
                new Vector2Int(4, 11),
                new Vector2Int(8, 4)
            },
            splitters = new List<SplitterPlacement>
            {
                new SplitterPlacement { cell = new Vector2Int(13, 11), rotationStep = 1 }
            },
            maxMovesForThreeStars = 5
        };
    }
}
