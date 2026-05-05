using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class Iteration04_Setup
{
    private const string GamePath = "Assets/LaserGame/Scenes/Game.unity";

    private static readonly Color BgColor = new Color(0.058f, 0.058f, 0.117f, 1f);
    private static readonly Color PanelColor = new Color(0.105f, 0.105f, 0.18f, 0.98f);
    private static readonly Color MagentaNeon = new Color(1f, 0.25f, 0.85f, 1f);
    private static readonly Color MagentaDim = new Color(1f, 0.25f, 0.85f, 0.32f);

    [MenuItem("LaserGame/Iteration 04/Update Game Scene")]
    public static void UpdateGameScene()
    {
        if (!File.Exists(GamePath))
        {
            Debug.LogWarning("Game scene not found. Run Iteration 03 Setup first.");
            return;
        }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null)
        {
            Debug.LogWarning("GameController not found. Run Iteration 03 Setup first.");
            return;
        }

        var canvas = ctrl.fieldRoot.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Canvas not found.");
            return;
        }
        var canvasRT = canvas.GetComponent<RectTransform>();

        var template = BuildMirrorTemplate(canvasRT);
        ctrl.mirrorTemplate = template.GetComponent<MirrorElement>();

        var elementsHolder = ctrl.fieldRoot.Find("ElementsHolder") as RectTransform;
        if (elementsHolder == null)
        {
            Debug.LogWarning("ElementsHolder not found in Field. Run Iteration 03 Setup first.");
            return;
        }
        ctrl.elementsHolder = elementsHolder;

        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 04] Game scene updated. Mirror template ready.");
    }

    [MenuItem("LaserGame/Iteration 04/Apply Default Test Level (5x5 Empty)")]
    public static void ApplyEmpty()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>()
        }, "5x5 empty (no mirrors)");
    }

    [MenuItem("LaserGame/Iteration 04/Test Level - 1 Mirror Diagonal")]
    public static void TestLevel1()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 2), initialRotationStep = 0 }
            }
        }, "1 mirror diagonal (5x5)");
    }

    [MenuItem("LaserGame/Iteration 04/Test Level - 2 Mirrors Bounce")]
    public static void TestLevel2()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(3, 2), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(3, 4), initialRotationStep = 1 }
            }
        }, "2 mirrors bounce (5x5)");
    }

    [MenuItem("LaserGame/Iteration 04/Test Level - 3 Mirrors Zigzag")]
    public static void TestLevel3()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 7, rows = 7,
            emitterCell = new Vector2Int(0, 3),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(3, 3), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(3, 1), initialRotationStep = 0 },
                new MirrorPlacement { cell = new Vector2Int(5, 1), initialRotationStep = 1 }
            }
        }, "3 mirrors zigzag (7x7)");
    }

    private static void ApplyTestLevel(LevelDefinition def, string label)
    {
        if (!File.Exists(GamePath)) { Debug.LogWarning("Game scene not found."); return; }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null) { Debug.LogWarning("GameController not found."); return; }
        if (ctrl.mirrorTemplate == null)
        {
            Debug.LogWarning("Mirror template not set. Run Update Game Scene (Iteration 04) first.");
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
        Debug.Log("[Iteration 04] Test level applied: " + label);
    }

    private static GameObject BuildMirrorTemplate(RectTransform parent)
    {
        var tpl = FindOrCreateChild(parent, "MirrorElementTemplate");
        var tplRT = tpl.GetComponent<RectTransform>();
        tplRT.anchorMin = new Vector2(0.5f, 0.5f);
        tplRT.anchorMax = new Vector2(0.5f, 0.5f);
        tplRT.pivot = new Vector2(0.5f, 0.5f);
        tplRT.sizeDelta = new Vector2(160, 160);

        var ripple = FindOrCreateChild(tplRT, "Ripple");
        var rippleImg = EnsureComponent<Image>(ripple);
        rippleImg.color = new Color(MagentaNeon.r, MagentaNeon.g, MagentaNeon.b, 0f);
        rippleImg.raycastTarget = false;
        StretchFull(ripple.GetComponent<RectTransform>());

        var glow = FindOrCreateChild(tplRT, "Glow");
        var glowImg = EnsureComponent<Image>(glow);
        glowImg.color = MagentaDim;
        glowImg.raycastTarget = false;
        var glowRT = glow.GetComponent<RectTransform>();
        glowRT.anchorMin = Vector2.zero;
        glowRT.anchorMax = Vector2.one;
        glowRT.offsetMin = new Vector2(-22, -22);
        glowRT.offsetMax = new Vector2(22, 22);

        var body = FindOrCreateChild(tplRT, "Body");
        var bodyImg = EnsureComponent<Image>(body);
        bodyImg.color = PanelColor;
        bodyImg.raycastTarget = true;
        StretchFull(body.GetComponent<RectTransform>());

        var btn = EnsureComponent<Button>(body);
        var col = btn.colors;
        col.normalColor = Color.white;
        col.highlightedColor = Color.white;
        col.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        btn.colors = col;
        btn.transition = Selectable.Transition.ColorTint;
        btn.targetGraphic = bodyImg;

        var line = FindOrCreateChild(tplRT, "DiagonalLine");
        var lineImg = EnsureComponent<Image>(line);
        lineImg.color = MagentaNeon;
        lineImg.raycastTarget = false;
        var lineRT = line.GetComponent<RectTransform>();
        lineRT.anchorMin = new Vector2(0.5f, 0.5f);
        lineRT.anchorMax = new Vector2(0.5f, 0.5f);
        lineRT.pivot = new Vector2(0.5f, 0.5f);
        lineRT.sizeDelta = new Vector2(160 * 1.4f, 160 * 0.13f);
        lineRT.anchoredPosition = Vector2.zero;
        lineRT.localEulerAngles = new Vector3(0, 0, 45f);

        ripple.transform.SetSiblingIndex(0);
        glow.transform.SetSiblingIndex(1);
        body.transform.SetSiblingIndex(2);
        line.transform.SetSiblingIndex(3);

        var mirror = EnsureComponent<MirrorElement>(tpl);
        mirror.bodyImage = bodyImg;
        mirror.glowImage = glowImg;
        mirror.diagonalLine = lineImg;
        mirror.ripple = rippleImg;
        mirror.button = btn;
        mirror.canvasGroup = EnsureComponent<CanvasGroup>(tpl);
        mirror.rectTransform = tplRT;

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
