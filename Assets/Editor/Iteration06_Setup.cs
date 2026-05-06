using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class Iteration06_Setup
{
    private const string GamePath = "Assets/LaserGame/Scenes/Game.unity";

    private static readonly Color WallBodyColor = new Color(0.35f, 0.08f, 0.25f, 1f);
    private static readonly Color WallBorderColor = new Color(1f, 0.25f, 0.85f, 0.85f);
    private static readonly Color YellowNeon = new Color(1f, 0.85f, 0.25f, 1f);
    private static readonly Color YellowDim = new Color(1f, 0.85f, 0.25f, 0.45f);

    [MenuItem("LaserGame/Iteration 06/Update Game Scene")]
    public static void UpdateGameScene()
    {
        if (!File.Exists(GamePath))
        {
            Debug.LogWarning("Game scene not found. Run earlier Iteration setups first.");
            return;
        }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null)
        {
            Debug.LogWarning("GameController not found. Run Iteration 03+04+05 setups first.");
            return;
        }

        var canvas = ctrl.fieldRoot.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Canvas not found.");
            return;
        }
        var canvasRT = canvas.GetComponent<RectTransform>();

        var wallTpl = BuildWallTemplate(canvasRT);
        ctrl.wallTemplate = wallTpl.GetComponent<WallElement>();

        var starTpl = BuildEnergyStarTemplate(canvasRT);
        ctrl.energyStarTemplate = starTpl.GetComponent<EnergyStarElement>();

        var coinFlyHost = BuildCoinFlyHost(canvasRT);
        ctrl.coinFlyHost = coinFlyHost.GetComponent<RectTransform>();

        var coinsIconRT = FindCoinsIcon(ctrl);
        if (coinsIconRT != null) ctrl.coinsIconRect = coinsIconRT;

        ReorderCanvasSiblings(canvasRT);

        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 06] Game scene updated. Wall + EnergyStar templates + CoinFlyHost ready.");
    }

    [MenuItem("LaserGame/Iteration 06/Test Level - Wall Block")]
    public static void TestLevelWallBlock()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>(),
            batteries = new List<Vector2Int> { new Vector2Int(4, 2) },
            walls = new List<Vector2Int> { new Vector2Int(2, 2) },
            energyStars = new List<Vector2Int>(),
            maxMovesForThreeStars = 0
        }, "Wall blocks straight path (unwinnable without mirrors — demo of wall blocking)");
    }

    [MenuItem("LaserGame/Iteration 06/Test Level - Wall Detour")]
    public static void TestLevelWallDetour()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(1, 2), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(1, 4), initialRotationStep = 0 }
            },
            batteries = new List<Vector2Int> { new Vector2Int(4, 4) },
            walls = new List<Vector2Int> { new Vector2Int(2, 2) },
            energyStars = new List<Vector2Int>(),
            maxMovesForThreeStars = 2
        }, "Wall + 2 mirrors detour (rotate both to /, \\)");
    }

    [MenuItem("LaserGame/Iteration 06/Test Level - Energy Star Bonus")]
    public static void TestLevelEnergyStar()
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
            batteries = new List<Vector2Int> { new Vector2Int(3, 4) },
            walls = new List<Vector2Int>(),
            energyStars = new List<Vector2Int> { new Vector2Int(1, 2) },
            maxMovesForThreeStars = 1
        }, "1 mirror + 1 battery + 1 energy star (star on initial path, collected on first frame)");
    }

    [MenuItem("LaserGame/Iteration 06/Test Level - All Mechanics")]
    public static void TestLevelAllMechanics()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 7, rows = 7,
            emitterCell = new Vector2Int(0, 3),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 3), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(2, 5), initialRotationStep = 1 },
                new MirrorPlacement { cell = new Vector2Int(5, 5), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(5, 1),
                new Vector2Int(5, 5)
            },
            walls = new List<Vector2Int>
            {
                new Vector2Int(4, 3),
                new Vector2Int(3, 1)
            },
            energyStars = new List<Vector2Int>
            {
                new Vector2Int(1, 3),
                new Vector2Int(2, 5)
            },
            maxMovesForThreeStars = 3
        }, "All mechanics demo (7x7)");
    }

    private static void ApplyTestLevel(LevelDefinition def, string label)
    {
        if (!File.Exists(GamePath)) { Debug.LogWarning("Game scene not found."); return; }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null) { Debug.LogWarning("GameController not found."); return; }
        if (ctrl.wallTemplate == null || ctrl.energyStarTemplate == null)
        {
            Debug.LogWarning("Templates not set. Run Update Game Scene (Iteration 06) first.");
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
        Debug.Log("[Iteration 06] Test level applied: " + label);
    }

    private static GameObject BuildWallTemplate(RectTransform parent)
    {
        var tpl = FindOrCreateChild(parent, "WallElementTemplate");
        var tplRT = tpl.GetComponent<RectTransform>();
        tplRT.anchorMin = new Vector2(0.5f, 0.5f);
        tplRT.anchorMax = new Vector2(0.5f, 0.5f);
        tplRT.pivot = new Vector2(0.5f, 0.5f);
        tplRT.sizeDelta = new Vector2(160, 160);

        var border = FindOrCreateChild(tplRT, "Border");
        var borderImg = EnsureComponent<Image>(border);
        borderImg.color = WallBorderColor;
        borderImg.raycastTarget = false;
        StretchFull(border.GetComponent<RectTransform>());

        var body = FindOrCreateChild(tplRT, "Body");
        var bodyImg = EnsureComponent<Image>(body);
        bodyImg.color = WallBodyColor;
        bodyImg.raycastTarget = false;
        var bodyRT = body.GetComponent<RectTransform>();
        bodyRT.anchorMin = Vector2.zero;
        bodyRT.anchorMax = Vector2.one;
        bodyRT.offsetMin = new Vector2(5, 5);
        bodyRT.offsetMax = new Vector2(-5, -5);

        BuildStripe(bodyRT, "Stripe_0", new Vector2(-25, 25));
        BuildStripe(bodyRT, "Stripe_1", new Vector2(0, 0));
        BuildStripe(bodyRT, "Stripe_2", new Vector2(25, -25));

        border.transform.SetSiblingIndex(0);
        body.transform.SetSiblingIndex(1);

        var wall = EnsureComponent<WallElement>(tpl);
        wall.bodyImage = bodyImg;
        wall.borderImage = borderImg;
        wall.rectTransform = tplRT;

        tpl.SetActive(false);
        return tpl;
    }

    private static void BuildStripe(RectTransform parent, string name, Vector2 offset)
    {
        var stripe = FindOrCreateChild(parent, name);
        var img = EnsureComponent<Image>(stripe);
        img.color = new Color(WallBorderColor.r, WallBorderColor.g, WallBorderColor.b, 0.22f);
        img.raycastTarget = false;
        var rt = stripe.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(180, 8);
        rt.anchoredPosition = offset;
        rt.localEulerAngles = new Vector3(0, 0, 45f);
    }

    private static GameObject BuildEnergyStarTemplate(RectTransform parent)
    {
        var tpl = FindOrCreateChild(parent, "EnergyStarElementTemplate");
        var tplRT = tpl.GetComponent<RectTransform>();
        tplRT.anchorMin = new Vector2(0.5f, 0.5f);
        tplRT.anchorMax = new Vector2(0.5f, 0.5f);
        tplRT.pivot = new Vector2(0.5f, 0.5f);
        tplRT.sizeDelta = new Vector2(120, 120);

        var glow = FindOrCreateChild(tplRT, "Glow");
        var glowImg = EnsureComponent<Image>(glow);
        glowImg.color = YellowDim;
        glowImg.raycastTarget = false;
        var glowRT = glow.GetComponent<RectTransform>();
        glowRT.anchorMin = Vector2.zero;
        glowRT.anchorMax = Vector2.one;
        glowRT.offsetMin = new Vector2(-25, -25);
        glowRT.offsetMax = new Vector2(25, 25);

        var starGo = FindOrCreateChild(tplRT, "Star");
        var starRT = starGo.GetComponent<RectTransform>();
        StretchFull(starRT);
        var starTMP = EnsureComponent<TextMeshProUGUI>(starGo);
        starTMP.text = "\u2605";
        starTMP.fontSize = 110;
        starTMP.fontStyle = FontStyles.Bold;
        starTMP.color = YellowNeon;
        starTMP.alignment = TextAlignmentOptions.Center;
        starTMP.raycastTarget = false;

        glow.transform.SetSiblingIndex(0);
        starGo.transform.SetSiblingIndex(1);

        var star = EnsureComponent<EnergyStarElement>(tpl);
        star.glowImage = glowImg;
        star.starText = starTMP;
        star.rectTransform = tplRT;

        tpl.SetActive(false);
        return tpl;
    }

    private static GameObject BuildCoinFlyHost(RectTransform parent)
    {
        var host = FindOrCreateChild(parent, "CoinFlyHost");
        var rt = host.GetComponent<RectTransform>();
        StretchFull(rt);
        var cg = EnsureComponent<CanvasGroup>(host);
        cg.blocksRaycasts = false;
        cg.interactable = false;
        return host;
    }

    private static RectTransform FindCoinsIcon(GameController ctrl)
    {
        if (ctrl.coinsText == null) return null;
        var coinsHud = ctrl.coinsText.transform.parent;
        if (coinsHud == null) return null;
        var icon = coinsHud.Find("CoinIcon");
        return icon as RectTransform;
    }

    private static void ReorderCanvasSiblings(RectTransform canvasRT)
    {
        var background = canvasRT.Find("Background");
        var topHud = canvasRT.Find("TopHud");
        var subHud = canvasRT.Find("SubHud");
        var field = canvasRT.Find("Field");
        var resetBtn = canvasRT.Find("ResetButton");
        var ctrlHost = canvasRT.Find("GameController_Host");
        var winFlash = canvasRT.Find("WinFlashOverlay");
        var levelComplete = canvasRT.Find("LevelCompletePopup");
        var coinFly = canvasRT.Find("CoinFlyHost");
        var mirrorTpl = canvasRT.Find("MirrorElementTemplate");
        var batteryTpl = canvasRT.Find("BatteryElementTemplate");
        var wallTpl = canvasRT.Find("WallElementTemplate");
        var starTpl = canvasRT.Find("EnergyStarElementTemplate");

        int idx = 0;
        SetIdx(background, ref idx);
        SetIdx(field, ref idx);
        SetIdx(topHud, ref idx);
        SetIdx(subHud, ref idx);
        SetIdx(resetBtn, ref idx);
        SetIdx(coinFly, ref idx);
        SetIdx(winFlash, ref idx);
        SetIdx(levelComplete, ref idx);
        SetIdx(ctrlHost, ref idx);
        SetIdx(mirrorTpl, ref idx);
        SetIdx(batteryTpl, ref idx);
        SetIdx(wallTpl, ref idx);
        SetIdx(starTpl, ref idx);
    }

    private static void SetIdx(Transform t, ref int idx)
    {
        if (t == null) return;
        t.SetSiblingIndex(idx);
        idx++;
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
