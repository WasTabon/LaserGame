using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class Iteration14_Setup
{
    private const string GamePath = "Assets/LaserGame/Scenes/Game.unity";
    private const string LevelsFolder = "Assets/LaserGame/Levels";
    private const string DatabasePath = "Assets/LaserGame/Levels/LevelsDatabase.asset";
    private const int ShuffleSeed = 12345;
    private const float FixedCellSize = 110f;

    [MenuItem("LaserGame/Iteration 14/Run All (Configs + Pan + Shuffle)")]
    public static void RunAll()
    {
        CreateExtendedConfigs();
        UpdateGameScene();
        ShuffleMapping();
    }

    [MenuItem("LaserGame/Iteration 14/Create Extended Configs (10 unique)")]
    public static void CreateExtendedConfigs()
    {
        EnsureFolders();
        var db = AssetDatabase.LoadAssetAtPath<LevelsDatabaseSO>(DatabasePath);
        if (db == null)
        {
            Debug.LogWarning("[Iteration 14] Database missing. Run Iteration 08 first.");
            return;
        }

        var oldConfigs = db.uniqueConfigs;
        var newConfigs = new LevelConfigSO[10];

        for (int i = 0; i < 10; i++)
        {
            string name = "Level_" + (i + 1).ToString("D2") + "_v14";
            string path = LevelsFolder + "/" + name + ".asset";
            var cfg = AssetDatabase.LoadAssetAtPath<LevelConfigSO>(path);
            if (cfg == null)
            {
                cfg = ScriptableObject.CreateInstance<LevelConfigSO>();
                AssetDatabase.CreateAsset(cfg, path);
            }
            cfg.definition = BuildConfig(i);
            EditorUtility.SetDirty(cfg);
            newConfigs[i] = cfg;
        }

        db.uniqueConfigs = newConfigs;
        db.totalLevels = 30;
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log("[Iteration 14] 10 unique configs created/updated. Sizes from 5x5 (L1) up to 15x15 (L10).");
    }

    [MenuItem("LaserGame/Iteration 14/Shuffle Level Mapping")]
    public static void ShuffleMapping()
    {
        var db = AssetDatabase.LoadAssetAtPath<LevelsDatabaseSO>(DatabasePath);
        if (db == null || db.uniqueConfigs == null || db.uniqueConfigs.Length < 10)
        {
            Debug.LogWarning("[Iteration 14] Database needs 10 configs. Run Create Extended Configs first.");
            return;
        }

        int[] mapping = new int[30];
        mapping[0] = 0;

        UnityEngine.Random.State prevState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(ShuffleSeed);

        var pool = new List<int>(29);
        for (int i = 0; i < 29; i++) pool.Add((i % 9) + 1);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int tmp = pool[i]; pool[i] = pool[j]; pool[j] = tmp;
        }
        for (int i = 0; i < 29; i++) mapping[i + 1] = pool[i];

        UnityEngine.Random.state = prevState;

        db.levelToConfigMapping = mapping;
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        string log = "[Iteration 14] Mapping shuffled (seed " + ShuffleSeed + "): L1=" + mapping[0];
        for (int i = 1; i < 30; i++) log += ", L" + (i + 1) + "=" + mapping[i];
        Debug.Log(log);
    }

    [MenuItem("LaserGame/Iteration 14/Update Game Scene (Pan)")]
    public static void UpdateGameScene()
    {
        if (!File.Exists(GamePath)) { Debug.LogWarning("Game scene not found."); return; }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null) { Debug.LogWarning("GameController not found."); return; }

        if (ctrl.grid != null)
        {
            ctrl.grid.useFixedCellSize = true;
            ctrl.grid.fixedCellSize = FixedCellSize;
            EditorUtility.SetDirty(ctrl.grid);
        }

        if (ctrl.fieldRoot != null)
        {
            var img = ctrl.fieldRoot.GetComponent<Image>();
            if (img == null)
            {
                img = ctrl.fieldRoot.gameObject.AddComponent<Image>();
                img.color = new Color(0, 0, 0, 0.001f);
            }
            img.raycastTarget = true;

            var pan = ctrl.fieldRoot.GetComponent<FieldPanController>();
            if (pan == null) pan = ctrl.fieldRoot.gameObject.AddComponent<FieldPanController>();
            pan.fieldRoot = ctrl.fieldRoot;
            var canvas = ctrl.fieldRoot.GetComponentInParent<Canvas>();
            if (canvas != null) pan.viewport = canvas.GetComponent<RectTransform>();
            ctrl.fieldPanController = pan;
            EditorUtility.SetDirty(pan);
        }

        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 14] FieldPanController added. Cell size fixed at " + FixedCellSize + "px. Drag field to pan.");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/LaserGame")) AssetDatabase.CreateFolder("Assets", "LaserGame");
        if (!AssetDatabase.IsValidFolder("Assets/LaserGame/Levels")) AssetDatabase.CreateFolder("Assets/LaserGame", "Levels");
    }

    private static LevelDefinition BuildConfig(int index)
    {
        switch (index)
        {
            case 0: return Cfg00_Easy();
            case 1: return Cfg01_ZigzagSmall();
            case 2: return Cfg02_WallMaze();
            case 3: return Cfg03_SquareLoop();
            case 4: return Cfg04_SplitterTriple();
            case 5: return Cfg05_LongZigzag();
            case 6: return Cfg06_DoubleSplitter();
            case 7: return Cfg07_WShape();
            case 8: return Cfg08_BigMaze();
            case 9: return Cfg09_MonsterFinal();
        }
        return Cfg00_Easy();
    }

    private static LevelDefinition Cfg00_Easy()
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

    private static LevelDefinition Cfg01_ZigzagSmall()
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
                new MirrorPlacement { cell = new Vector2Int(4, 5), initialRotationStep = 0 }
            },
            batteries = new List<Vector2Int> { new Vector2Int(4, 1) },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int> { new Vector2Int(1, 4), new Vector2Int(4, 3) },
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 3
        };
    }

    private static LevelDefinition Cfg02_WallMaze()
    {
        return new LevelDefinition
        {
            cols = 8, rows = 8,
            emitterCell = new Vector2Int(0, 4),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(1, 4), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(1, 6), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(5, 6), initialRotationStep = 0 }
            },
            batteries = new List<Vector2Int> { new Vector2Int(5, 1) },
            walls = new List<Vector2Int>
            {
                new Vector2Int(3, 4),
                new Vector2Int(3, 2)
            },
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 5),
                new Vector2Int(5, 4)
            },
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 3
        };
    }

    private static LevelDefinition Cfg03_SquareLoop()
    {
        return new LevelDefinition
        {
            cols = 9, rows = 9,
            emitterCell = new Vector2Int(0, 4),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 4), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(2, 7), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(7, 7), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(7, 4), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(5, 7),
                new Vector2Int(7, 5)
            },
            walls = new List<Vector2Int>
            {
                new Vector2Int(5, 4)
            },
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 4),
                new Vector2Int(4, 4)
            },
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 4
        };
    }

    private static LevelDefinition Cfg04_SplitterTriple()
    {
        return new LevelDefinition
        {
            cols = 9, rows = 9,
            emitterCell = new Vector2Int(0, 4),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 3), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(6, 3), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(7, 4),
                new Vector2Int(4, 3),
                new Vector2Int(6, 5)
            },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 4),
                new Vector2Int(5, 3)
            },
            splitters = new List<SplitterPlacement>
            {
                new SplitterPlacement { cell = new Vector2Int(2, 4), rotationStep = 1 }
            },
            maxMovesForThreeStars = 2
        };
    }

    private static LevelDefinition Cfg05_LongZigzag()
    {
        return new LevelDefinition
        {
            cols = 10, rows = 10,
            emitterCell = new Vector2Int(0, 5),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 5), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(2, 8), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(5, 8), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(5, 2), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(8, 2), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(8, 6),
                new Vector2Int(3, 8)
            },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 5),
                new Vector2Int(5, 5),
                new Vector2Int(8, 4)
            },
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 5
        };
    }

    private static LevelDefinition Cfg06_DoubleSplitter()
    {
        return new LevelDefinition
        {
            cols = 11, rows = 11,
            emitterCell = new Vector2Int(0, 5),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(3, 8), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(8, 5), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(3, 2), initialRotationStep = 0 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(10, 5),
                new Vector2Int(3, 10),
                new Vector2Int(3, 0),
                new Vector2Int(8, 8)
            },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 5),
                new Vector2Int(5, 5)
            },
            splitters = new List<SplitterPlacement>
            {
                new SplitterPlacement { cell = new Vector2Int(3, 5), rotationStep = 0 },
                new SplitterPlacement { cell = new Vector2Int(8, 5), rotationStep = 1 }
            },
            maxMovesForThreeStars = 3
        };
    }

    private static LevelDefinition Cfg07_WShape()
    {
        return new LevelDefinition
        {
            cols = 12, rows = 12,
            emitterCell = new Vector2Int(0, 6),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 6), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(2, 9), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(5, 9), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(5, 3), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(9, 3), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(9, 9), initialRotationStep = 0 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(11, 9),
                new Vector2Int(9, 6)
            },
            walls = new List<Vector2Int>
            {
                new Vector2Int(7, 3),
                new Vector2Int(4, 6)
            },
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 6),
                new Vector2Int(2, 8),
                new Vector2Int(7, 9)
            },
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 6
        };
    }

    private static LevelDefinition Cfg08_BigMaze()
    {
        return new LevelDefinition
        {
            cols = 13, rows = 13,
            emitterCell = new Vector2Int(0, 6),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 6), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(2, 10), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(6, 10), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(6, 2), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(10, 2), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(10, 10), initialRotationStep = 0 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(12, 10),
                new Vector2Int(4, 10),
                new Vector2Int(10, 7)
            },
            walls = new List<Vector2Int>
            {
                new Vector2Int(4, 6),
                new Vector2Int(8, 6),
                new Vector2Int(8, 2)
            },
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 6),
                new Vector2Int(6, 5),
                new Vector2Int(10, 4)
            },
            splitters = new List<SplitterPlacement>(),
            maxMovesForThreeStars = 6
        };
    }

    private static LevelDefinition Cfg09_MonsterFinal()
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
                new MirrorPlacement { cell = new Vector2Int(6, 11), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(6, 3), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(10, 3), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(10, 11), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(13, 11), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(13, 5),
                new Vector2Int(8, 11),
                new Vector2Int(2, 9),
                new Vector2Int(10, 7)
            },
            walls = new List<Vector2Int>
            {
                new Vector2Int(4, 7),
                new Vector2Int(8, 7),
                new Vector2Int(12, 7),
                new Vector2Int(8, 3)
            },
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 7),
                new Vector2Int(4, 11),
                new Vector2Int(11, 11)
            },
            splitters = new List<SplitterPlacement>
            {
                new SplitterPlacement { cell = new Vector2Int(6, 7), rotationStep = 1 }
            },
            maxMovesForThreeStars = 7
        };
    }
}
