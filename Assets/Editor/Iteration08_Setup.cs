using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Iteration08_Setup
{
    private const string GamePath = "Assets/LaserGame/Scenes/Game.unity";
    private const string LevelsFolder = "Assets/LaserGame/Levels";
    private const string DatabasePath = "Assets/LaserGame/Levels/LevelsDatabase.asset";

    [MenuItem("LaserGame/Iteration 08/Create Levels Database + 5 Configs")]
    public static void CreateLevelsAssets()
    {
        EnsureFolders();

        var configs = new LevelConfigSO[5];
        configs[0] = CreateOrUpdateConfig("Level_01_Mirror", BuildLevel1());
        configs[1] = CreateOrUpdateConfig("Level_02_WallDetour", BuildLevel2());
        configs[2] = CreateOrUpdateConfig("Level_03_EnergyStar", BuildLevel3());
        configs[3] = CreateOrUpdateConfig("Level_04_MultiBattery", BuildLevel4());
        configs[4] = CreateOrUpdateConfig("Level_05_Splitter", BuildLevel5());

        var db = AssetDatabase.LoadAssetAtPath<LevelsDatabaseSO>(DatabasePath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<LevelsDatabaseSO>();
            AssetDatabase.CreateAsset(db, DatabasePath);
        }
        db.uniqueConfigs = configs;
        db.totalLevels = 30;
        db.levelToConfigMapping = new int[30];
        for (int i = 0; i < 30; i++) db.levelToConfigMapping[i] = i % 5;

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Iteration 08] Created/updated 5 level configs + LevelsDatabase. Cyclic mapping 1-30 -> configs 0-4.");
    }

    [MenuItem("LaserGame/Iteration 08/Assign Database To Game Scene")]
    public static void AssignDatabaseToGameScene()
    {
        if (!File.Exists(GamePath)) { Debug.LogWarning("Game scene not found."); return; }
        var db = AssetDatabase.LoadAssetAtPath<LevelsDatabaseSO>(DatabasePath);
        if (db == null) { Debug.LogWarning("LevelsDatabase not found. Run Create Levels Database first."); return; }

        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null) { Debug.LogWarning("GameController not found."); return; }
        ctrl.levelsDatabase = db;
        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 08] LevelsDatabase assigned to GameController in Game scene.");
    }

    [MenuItem("LaserGame/Iteration 08/Run Both (Create + Assign)")]
    public static void RunBoth()
    {
        CreateLevelsAssets();
        AssignDatabaseToGameScene();
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/LaserGame")) AssetDatabase.CreateFolder("Assets", "LaserGame");
        if (!AssetDatabase.IsValidFolder("Assets/LaserGame/Levels")) AssetDatabase.CreateFolder("Assets/LaserGame", "Levels");
    }

    private static LevelConfigSO CreateOrUpdateConfig(string name, LevelDefinition def)
    {
        string path = LevelsFolder + "/" + name + ".asset";
        var cfg = AssetDatabase.LoadAssetAtPath<LevelConfigSO>(path);
        if (cfg == null)
        {
            cfg = ScriptableObject.CreateInstance<LevelConfigSO>();
            AssetDatabase.CreateAsset(cfg, path);
        }
        cfg.definition = def;
        EditorUtility.SetDirty(cfg);
        return cfg;
    }

    private static LevelDefinition BuildLevel1()
    {
        return new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 2), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int> { new Vector2Int(2, 4) },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int>(),
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 1
        };
    }

    private static LevelDefinition BuildLevel2()
    {
        return new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(1, 2), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(1, 4), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int> { new Vector2Int(4, 4) },
            walls = new List<Vector2Int> { new Vector2Int(2, 2) },
            energyStars = new List<Vector2Int>(),
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 2
        };
    }

    private static LevelDefinition BuildLevel3()
    {
        return new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(3, 2), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int> { new Vector2Int(3, 4) },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int> { new Vector2Int(1, 2) },
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 1
        };
    }

    private static LevelDefinition BuildLevel4()
    {
        return new LevelDefinition
        {
            cols = 7, rows = 7,
            emitterCell = new Vector2Int(0, 3),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(4, 3), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(2, 3),
                new Vector2Int(4, 5)
            },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int>(),
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 1
        };
    }

    private static LevelDefinition BuildLevel5()
    {
        return new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(3, 2), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(3, 4),
                new Vector2Int(1, 3)
            },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int>(),
            splitters = new List<SplitterPlacement>
            {
                new SplitterPlacement { cell = new Vector2Int(3, 3), rotationStep = 1 }
            },
            maxMovesForThreeStars = 1
        };
    }
}
