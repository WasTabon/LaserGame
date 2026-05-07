using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class Iteration07_Setup
{
    private const string GamePath = "Assets/LaserGame/Scenes/Game.unity";

    private static readonly Color PanelColor = new Color(0.105f, 0.105f, 0.18f, 0.98f);
    private static readonly Color CyanNeon = new Color(0.2f, 0.95f, 1f, 1f);
    private static readonly Color CyanDim = new Color(0.2f, 0.95f, 1f, 0.32f);
    private static readonly Color CyanLine2 = new Color(0.2f, 0.95f, 1f, 0.55f);

    [MenuItem("LaserGame/Iteration 07/Update Game Scene")]
    public static void UpdateGameScene()
    {
        if (!File.Exists(GamePath))
        {
            Debug.LogWarning("Game scene not found. Run earlier setups first.");
            return;
        }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null)
        {
            Debug.LogWarning("GameController not found.");
            return;
        }

        var canvas = ctrl.fieldRoot.GetComponentInParent<Canvas>();
        if (canvas == null) { Debug.LogWarning("Canvas not found."); return; }
        var canvasRT = canvas.GetComponent<RectTransform>();

        var splitterTpl = BuildSplitterTemplate(canvasRT);
        ctrl.splitterTemplate = splitterTpl.GetComponent<SplitterElement>();

        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 07] Game scene updated. Splitter template ready.");
    }

    [MenuItem("LaserGame/Iteration 07/Test Level - Basic Splitter")]
    public static void TestLevelBasicSplitter()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>(),
            batteries = new List<Vector2Int>
            {
                new Vector2Int(4, 2),
                new Vector2Int(2, 4)
            },
            splitters = new List<SplitterPlacement>
            {
                new SplitterPlacement { cell = new Vector2Int(2, 2), rotationStep = 0 }
            },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int>(),
            maxMovesForThreeStars = 0
        }, "Basic splitter (5x5, instant win) - splits right-going beam into right + up");
    }

    [MenuItem("LaserGame/Iteration 07/Test Level - Splitter + Mirror")]
    public static void TestLevelSplitterMirror()
    {
        ApplyTestLevel(new LevelDefinition
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
            splitters = new List<SplitterPlacement>
            {
                new SplitterPlacement { cell = new Vector2Int(3, 3), rotationStep = 1 }
            },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int>(),
            maxMovesForThreeStars = 1
        }, "Splitter + mirror (rotate mirror to /, then splitter sends beams up & left)");
    }

    [MenuItem("LaserGame/Iteration 07/Test Level - Two Splitters")]
    public static void TestLevelTwoSplitters()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 7, rows = 7,
            emitterCell = new Vector2Int(0, 3),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>(),
            batteries = new List<Vector2Int>
            {
                new Vector2Int(6, 3),
                new Vector2Int(2, 6),
                new Vector2Int(4, 0)
            },
            splitters = new List<SplitterPlacement>
            {
                new SplitterPlacement { cell = new Vector2Int(2, 3), rotationStep = 0 },
                new SplitterPlacement { cell = new Vector2Int(4, 3), rotationStep = 1 }
            },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int>(),
            maxMovesForThreeStars = 0
        }, "Two splitters chain (7x7, instant win, 3 batteries from one beam)");
    }

    [MenuItem("LaserGame/Iteration 07/Test Level - Splitter Full Demo")]
    public static void TestLevelSplitterFullDemo()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 7, rows = 7,
            emitterCell = new Vector2Int(0, 3),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 3), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(2, 6),
                new Vector2Int(6, 5)
            },
            splitters = new List<SplitterPlacement>
            {
                new SplitterPlacement { cell = new Vector2Int(2, 5), rotationStep = 0 }
            },
            walls = new List<Vector2Int>
            {
                new Vector2Int(4, 3)
            },
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 3),
                new Vector2Int(4, 5)
            },
            maxMovesForThreeStars = 1
        }, "Full demo: splitter + mirror + walls + energy stars (7x7, rotate mirror to / for win)");
    }

    private static void ApplyTestLevel(LevelDefinition def, string label)
    {
        if (!File.Exists(GamePath)) { Debug.LogWarning("Game scene not found."); return; }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null) { Debug.LogWarning("GameController not found."); return; }
        if (ctrl.splitterTemplate == null)
        {
            Debug.LogWarning("Splitter template not set. Run Update Game Scene (Iteration 07) first.");
            return;
        }

        ctrl.testLevel = def;

        if (ctrl.grid != null)
        {
            ctrl.grid.cols = def.cols;
            ctrl.grid.rows = def.rows;
            ctrl.grid.Build();
        }
        if (ctrl.emitter != null && ctrl.grid != null)
        {
            ctrl.emitter.cell = def.emitterCell;
            ctrl.emitter.direction = def.emitterDir;
            ctrl.emitter.PlaceOnGrid(ctrl.grid);
        }

        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 07] Test level applied: " + label);
    }

    private static GameObject BuildSplitterTemplate(RectTransform parent)
    {
        var tpl = FindOrCreateChild(parent, "SplitterElementTemplate");
        var tplRT = tpl.GetComponent<RectTransform>();
        tplRT.anchorMin = new Vector2(0.5f, 0.5f);
        tplRT.anchorMax = new Vector2(0.5f, 0.5f);
        tplRT.pivot = new Vector2(0.5f, 0.5f);
        tplRT.sizeDelta = new Vector2(160, 160);

        var glow = FindOrCreateChild(tplRT, "Glow");
        var glowImg = EnsureComponent<Image>(glow);
        glowImg.color = CyanDim;
        glowImg.raycastTarget = false;
        var glowRT = glow.GetComponent<RectTransform>();
        glowRT.anchorMin = Vector2.zero;
        glowRT.anchorMax = Vector2.one;
        glowRT.offsetMin = new Vector2(-22, -22);
        glowRT.offsetMax = new Vector2(22, 22);

        var body = FindOrCreateChild(tplRT, "Body");
        var bodyImg = EnsureComponent<Image>(body);
        bodyImg.color = PanelColor;
        bodyImg.raycastTarget = false;
        StretchFull(body.GetComponent<RectTransform>());

        var lineA = FindOrCreateChild(tplRT, "DiagonalLineA");
        var lineAImg = EnsureComponent<Image>(lineA);
        lineAImg.color = CyanNeon;
        lineAImg.raycastTarget = false;
        var lineART = lineA.GetComponent<RectTransform>();
        lineART.anchorMin = new Vector2(0.5f, 0.5f);
        lineART.anchorMax = new Vector2(0.5f, 0.5f);
        lineART.pivot = new Vector2(0.5f, 0.5f);
        lineART.sizeDelta = new Vector2(160 * 1.4f, 160 * 0.13f);
        lineART.anchoredPosition = Vector2.zero;
        lineART.localEulerAngles = new Vector3(0, 0, 45f);

        var lineB = FindOrCreateChild(tplRT, "DiagonalLineB");
        var lineBImg = EnsureComponent<Image>(lineB);
        lineBImg.color = CyanLine2;
        lineBImg.raycastTarget = false;
        var lineBRT = lineB.GetComponent<RectTransform>();
        lineBRT.anchorMin = new Vector2(0.5f, 0.5f);
        lineBRT.anchorMax = new Vector2(0.5f, 0.5f);
        lineBRT.pivot = new Vector2(0.5f, 0.5f);
        lineBRT.sizeDelta = new Vector2(160 * 1.4f, 160 * 0.13f);
        lineBRT.anchoredPosition = Vector2.zero;
        lineBRT.localEulerAngles = new Vector3(0, 0, 135f);

        glow.transform.SetSiblingIndex(0);
        body.transform.SetSiblingIndex(1);
        lineB.transform.SetSiblingIndex(2);
        lineA.transform.SetSiblingIndex(3);

        var splitter = EnsureComponent<SplitterElement>(tpl);
        splitter.bodyImage = bodyImg;
        splitter.glowImage = glowImg;
        splitter.diagonalLineA = lineAImg;
        splitter.diagonalLineB = lineBImg;
        splitter.rectTransform = tplRT;

        tpl.SetActive(false);
        return tpl;
    }

    private static GameObject FindOrCreateChild(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var c = parent.GetChild(i);
            if (c.name == name) return c.gameObject;
        }
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject FindOrCreateChild(RectTransform parent, string name)
    {
        return FindOrCreateChild((Transform)parent, name);
    }

    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) c = go.AddComponent<T>();
        return c;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }
}
