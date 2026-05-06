using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class Iteration05_Setup
{
    private const string GamePath = "Assets/LaserGame/Scenes/Game.unity";

    private static readonly Color BgColor = new Color(0.058f, 0.058f, 0.117f, 1f);
    private static readonly Color PanelColor = new Color(0.105f, 0.105f, 0.18f, 0.98f);
    private static readonly Color CyanNeon = new Color(0.2f, 0.95f, 1f, 1f);
    private static readonly Color MagentaNeon = new Color(1f, 0.25f, 0.85f, 1f);
    private static readonly Color YellowNeon = new Color(1f, 0.85f, 0.25f, 1f);
    private static readonly Color YellowDim = new Color(1f, 0.85f, 0.25f, 0.18f);
    private static readonly Color GreyDim = new Color(0.55f, 0.6f, 0.7f, 1f);
    private static readonly Color WhiteSoft = new Color(0.92f, 0.95f, 1f, 1f);

    [MenuItem("LaserGame/Iteration 05/Update Game Scene")]
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

        var batteryTemplate = BuildBatteryTemplate(canvasRT);
        ctrl.batteryTemplate = batteryTemplate.GetComponent<BatteryElement>();

        var winFlash = BuildWinFlashOverlay(canvasRT);
        ctrl.winFlashOverlay = winFlash.GetComponent<Image>();

        var popup = BuildLevelCompletePopup(canvasRT);
        ctrl.levelCompletePopup = popup.GetComponent<LevelCompletePopup>();

        winFlash.transform.SetAsLastSibling();
        popup.transform.SetAsLastSibling();

        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 05] Game scene updated. Battery template + win flash + level complete popup ready.");
    }

    [MenuItem("LaserGame/Iteration 05/Apply Default Test Level (5x5 + Battery)")]
    public static void ApplyDefaultBatteryLevel()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>(),
            batteries = new List<Vector2Int> { new Vector2Int(4, 2) },
            energyStars = new List<Vector2Int>(),
            maxMovesForThreeStars = 0
        }, "5x5 + 1 battery (instant win)");
    }

    [MenuItem("LaserGame/Iteration 05/Test Level - 1 Mirror + Battery")]
    public static void TestLevel1Mirror()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(2, 2), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int> { new Vector2Int(2, 4) },
            energyStars = new List<Vector2Int>(),
            maxMovesForThreeStars = 1
        }, "1 mirror + 1 battery (rotate to /)");
    }

    [MenuItem("LaserGame/Iteration 05/Test Level - 2 Batteries Pass-Through")]
    public static void TestLevel2BatteriesPassThrough()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>(),
            batteries = new List<Vector2Int>
            {
                new Vector2Int(2, 2),
                new Vector2Int(4, 2)
            },
            energyStars = new List<Vector2Int>(),
            maxMovesForThreeStars = 0
        }, "2 batteries on straight line (instant win)");
    }

    [MenuItem("LaserGame/Iteration 05/Test Level - Mirror + 2 Batteries")]
    public static void TestLevelMirror2Batteries()
    {
        ApplyTestLevel(new LevelDefinition
        {
            cols = 5, rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new List<MirrorPlacement>
            {
                new MirrorPlacement { cell = new Vector2Int(4, 2), initialRotationStep = 1 }
            },
            batteries = new List<Vector2Int>
            {
                new Vector2Int(2, 2),
                new Vector2Int(4, 4)
            },
            energyStars = new List<Vector2Int>(),
            maxMovesForThreeStars = 1
        }, "1 mirror + 2 batteries (rotate to /, then up to top battery)");
    }

    [MenuItem("LaserGame/Iteration 05/Trigger Win (In Play Mode)")]
    public static void TriggerWinDebug()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Trigger Win works only in Play Mode.");
            return;
        }
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null) return;
        ctrl.DebugTriggerWin();
    }

    private static void ApplyTestLevel(LevelDefinition def, string label)
    {
        if (!File.Exists(GamePath)) { Debug.LogWarning("Game scene not found."); return; }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null) { Debug.LogWarning("GameController not found."); return; }
        if (ctrl.mirrorTemplate == null || ctrl.batteryTemplate == null)
        {
            Debug.LogWarning("Templates not set. Run Update Game Scene (Iteration 04 + 05) first.");
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
        Debug.Log("[Iteration 05] Test level applied: " + label);
    }

    private static GameObject BuildBatteryTemplate(RectTransform parent)
    {
        var tpl = FindOrCreateChild(parent, "BatteryElementTemplate");
        var tplRT = tpl.GetComponent<RectTransform>();
        tplRT.anchorMin = new Vector2(0.5f, 0.5f);
        tplRT.anchorMax = new Vector2(0.5f, 0.5f);
        tplRT.pivot = new Vector2(0.5f, 0.5f);
        tplRT.sizeDelta = new Vector2(160, 160);

        var glow = FindOrCreateChild(tplRT, "Glow");
        var glowImg = EnsureComponent<Image>(glow);
        glowImg.color = YellowDim;
        glowImg.raycastTarget = false;
        var glowRT = glow.GetComponent<RectTransform>();
        glowRT.anchorMin = Vector2.zero;
        glowRT.anchorMax = Vector2.one;
        glowRT.offsetMin = new Vector2(-25, -25);
        glowRT.offsetMax = new Vector2(25, 25);

        var body = FindOrCreateChild(tplRT, "Body");
        var bodyImg = EnsureComponent<Image>(body);
        bodyImg.color = PanelColor;
        bodyImg.raycastTarget = false;
        StretchFull(body.GetComponent<RectTransform>());

        var fill = FindOrCreateChild(tplRT, "Fill");
        var fillImg = EnsureComponent<Image>(fill);
        fillImg.color = new Color(GreyDim.r, GreyDim.g, GreyDim.b, 0.5f);
        fillImg.raycastTarget = false;
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0.5f, 0.5f);
        fillRT.anchorMax = new Vector2(0.5f, 0.5f);
        fillRT.pivot = new Vector2(0.5f, 0.5f);
        fillRT.sizeDelta = new Vector2(160 * 0.55f, 160 * 0.55f);
        fillRT.anchoredPosition = Vector2.zero;

        var icon = FindOrCreateChild(tplRT, "Icon");
        var iconRT = icon.GetComponent<RectTransform>();
        StretchFull(iconRT);
        var iconTMP = EnsureComponent<TextMeshProUGUI>(icon);
        iconTMP.text = "\u26A1";
        iconTMP.fontSize = 88;
        iconTMP.fontStyle = FontStyles.Bold;
        iconTMP.color = GreyDim;
        iconTMP.alignment = TextAlignmentOptions.Center;
        iconTMP.raycastTarget = false;

        glow.transform.SetSiblingIndex(0);
        body.transform.SetSiblingIndex(1);
        fill.transform.SetSiblingIndex(2);
        icon.transform.SetSiblingIndex(3);

        var battery = EnsureComponent<BatteryElement>(tpl);
        battery.bodyImage = bodyImg;
        battery.glowImage = glowImg;
        battery.fillImage = fillImg;
        battery.iconText = iconTMP;
        battery.rectTransform = tplRT;

        tpl.SetActive(false);
        return tpl;
    }

    private static GameObject BuildWinFlashOverlay(RectTransform parent)
    {
        var go = FindOrCreateChild(parent, "WinFlashOverlay");
        var img = EnsureComponent<Image>(go);
        img.color = new Color(CyanNeon.r, CyanNeon.g, CyanNeon.b, 0f);
        img.raycastTarget = false;
        StretchFull(go.GetComponent<RectTransform>());
        go.SetActive(false);
        return go;
    }

    private static GameObject BuildLevelCompletePopup(RectTransform parent)
    {
        var popup = FindOrCreateChild(parent, "LevelCompletePopup");
        var popupRT = popup.GetComponent<RectTransform>();
        StretchFull(popupRT);
        var popupGroup = EnsureComponent<CanvasGroup>(popup);
        popupGroup.alpha = 0f;
        popupGroup.blocksRaycasts = false;

        var backdrop = FindOrCreateChild(popupRT, "Backdrop");
        var backdropImg = EnsureComponent<Image>(backdrop);
        backdropImg.color = new Color(0, 0, 0, 0.7f);
        backdropImg.raycastTarget = true;
        StretchFull(backdrop.GetComponent<RectTransform>());

        var content = FindOrCreateChild(popupRT, "Content");
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.5f, 0.5f);
        contentRT.anchorMax = new Vector2(0.5f, 0.5f);
        contentRT.pivot = new Vector2(0.5f, 0.5f);
        contentRT.sizeDelta = new Vector2(720, 880);
        contentRT.anchoredPosition = Vector2.zero;
        var contentImg = EnsureComponent<Image>(content);
        contentImg.color = PanelColor;

        var border = FindOrCreateChild(contentRT, "Border");
        var borderImg = EnsureComponent<Image>(border);
        borderImg.color = new Color(CyanNeon.r, CyanNeon.g, CyanNeon.b, 0.3f);
        borderImg.raycastTarget = false;
        var borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = new Vector2(-3, -3);
        borderRT.offsetMax = new Vector2(3, 3);
        border.transform.SetAsFirstSibling();

        var titleGo = FindOrCreateChild(contentRT, "Title");
        var titleRT = titleGo.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.sizeDelta = new Vector2(0, 110);
        titleRT.anchoredPosition = new Vector2(0, -30);
        var titleTMP = EnsureComponent<TextMeshProUGUI>(titleGo);
        titleTMP.text = "LEVEL COMPLETE";
        titleTMP.fontSize = 52;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = CyanNeon;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.characterSpacing = 6;
        titleTMP.raycastTarget = false;

        var starsRow = FindOrCreateChild(contentRT, "StarsRow");
        var starsRowRT = starsRow.GetComponent<RectTransform>();
        starsRowRT.anchorMin = new Vector2(0, 1);
        starsRowRT.anchorMax = new Vector2(1, 1);
        starsRowRT.pivot = new Vector2(0.5f, 1);
        starsRowRT.sizeDelta = new Vector2(0, 160);
        starsRowRT.anchoredPosition = new Vector2(0, -170);

        var starsLayout = EnsureComponent<HorizontalLayoutGroup>(starsRow);
        starsLayout.spacing = 30;
        starsLayout.childAlignment = TextAnchor.MiddleCenter;
        starsLayout.childForceExpandWidth = false;
        starsLayout.childForceExpandHeight = false;
        starsLayout.childControlWidth = false;
        starsLayout.childControlHeight = false;

        var starRects = new RectTransform[3];
        var starGroups = new CanvasGroup[3];
        var starTexts = new TextMeshProUGUI[3];

        for (int i = 0; i < 3; i++)
        {
            var s = FindOrCreateChild(starsRowRT, "Star_" + i);
            var sRT = s.GetComponent<RectTransform>();
            sRT.sizeDelta = new Vector2(150, 150);
            var sg = EnsureComponent<CanvasGroup>(s);
            var sTMP = EnsureComponent<TextMeshProUGUI>(s);
            sTMP.text = "\u2605";
            sTMP.fontSize = 140;
            sTMP.fontStyle = FontStyles.Bold;
            sTMP.color = new Color(0.45f, 0.5f, 0.6f, 0.45f);
            sTMP.alignment = TextAlignmentOptions.Center;
            sTMP.raycastTarget = false;
            starRects[i] = sRT;
            starGroups[i] = sg;
            starTexts[i] = sTMP;
        }

        var coinsBlock = FindOrCreateChild(contentRT, "CoinsBlock");
        var coinsBlockRT = coinsBlock.GetComponent<RectTransform>();
        coinsBlockRT.anchorMin = new Vector2(0.5f, 1);
        coinsBlockRT.anchorMax = new Vector2(0.5f, 1);
        coinsBlockRT.pivot = new Vector2(0.5f, 1);
        coinsBlockRT.sizeDelta = new Vector2(280, 80);
        coinsBlockRT.anchoredPosition = new Vector2(0, -360);
        var coinsBlockGroup = EnsureComponent<CanvasGroup>(coinsBlock);
        var coinsBlockBg = EnsureComponent<Image>(coinsBlock);
        coinsBlockBg.color = new Color(BgColor.r, BgColor.g, BgColor.b, 0.7f);
        coinsBlockBg.raycastTarget = false;

        var coinsIcon = FindOrCreateChild(coinsBlockRT, "Icon");
        var coinsIconImg = EnsureComponent<Image>(coinsIcon);
        coinsIconImg.color = YellowNeon;
        coinsIconImg.raycastTarget = false;
        var coinsIconRT = coinsIcon.GetComponent<RectTransform>();
        coinsIconRT.anchorMin = new Vector2(0, 0.5f);
        coinsIconRT.anchorMax = new Vector2(0, 0.5f);
        coinsIconRT.pivot = new Vector2(0.5f, 0.5f);
        coinsIconRT.sizeDelta = new Vector2(40, 40);
        coinsIconRT.anchoredPosition = new Vector2(45, 0);

        var coinsRewardGo = FindOrCreateChild(coinsBlockRT, "Reward");
        var coinsRewardRT = coinsRewardGo.GetComponent<RectTransform>();
        coinsRewardRT.anchorMin = new Vector2(0, 0);
        coinsRewardRT.anchorMax = new Vector2(1, 1);
        coinsRewardRT.offsetMin = new Vector2(80, 0);
        coinsRewardRT.offsetMax = new Vector2(-15, 0);
        var coinsRewardTMP = EnsureComponent<TextMeshProUGUI>(coinsRewardGo);
        coinsRewardTMP.text = "+ 25";
        coinsRewardTMP.fontSize = 40;
        coinsRewardTMP.fontStyle = FontStyles.Bold;
        coinsRewardTMP.alignment = TextAlignmentOptions.MidlineLeft;
        coinsRewardTMP.color = WhiteSoft;
        coinsRewardTMP.raycastTarget = false;

        var nextBtn = CreatePillButton(contentRT, "NextButton", "NEXT", CyanNeon, BgColor, 56);
        var nextRT = nextBtn.GetComponent<RectTransform>();
        nextRT.anchorMin = new Vector2(0.5f, 0);
        nextRT.anchorMax = new Vector2(0.5f, 0);
        nextRT.pivot = new Vector2(0.5f, 0);
        nextRT.sizeDelta = new Vector2(480, 130);
        nextRT.anchoredPosition = new Vector2(0, 200);

        var replayBtn = CreatePillButton(contentRT, "ReplayButton", "REPLAY", PanelColor, MagentaNeon, 36);
        var replayRT = replayBtn.GetComponent<RectTransform>();
        replayRT.anchorMin = new Vector2(0.5f, 0);
        replayRT.anchorMax = new Vector2(0.5f, 0);
        replayRT.pivot = new Vector2(0.5f, 0);
        replayRT.sizeDelta = new Vector2(220, 100);
        replayRT.anchoredPosition = new Vector2(-130, 60);

        var menuBtn = CreatePillButton(contentRT, "MenuButton", "MENU", PanelColor, MagentaNeon, 36);
        var menuRT = menuBtn.GetComponent<RectTransform>();
        menuRT.anchorMin = new Vector2(0.5f, 0);
        menuRT.anchorMax = new Vector2(0.5f, 0);
        menuRT.pivot = new Vector2(0.5f, 0);
        menuRT.sizeDelta = new Vector2(220, 100);
        menuRT.anchoredPosition = new Vector2(130, 60);

        var lcp = EnsureComponent<LevelCompletePopup>(popup);
        lcp.canvasGroup = popupGroup;
        lcp.contentRect = contentRT;
        lcp.backdrop = backdropImg;
        lcp.titleText = titleTMP;
        lcp.starRects = starRects;
        lcp.starGroups = starGroups;
        lcp.starTexts = starTexts;
        lcp.coinsBlockRect = coinsBlockRT;
        lcp.coinsBlockGroup = coinsBlockGroup;
        lcp.coinsRewardText = coinsRewardTMP;
        lcp.replayButton = replayBtn.GetComponent<Button>();
        lcp.nextButton = nextBtn.GetComponent<Button>();
        lcp.menuButton = menuBtn.GetComponent<Button>();

        popup.SetActive(false);
        return popup;
    }

    private static GameObject CreatePillButton(RectTransform parent, string name, string label, Color fill, Color textColor, int fontSize)
    {
        var btn = FindOrCreateChild(parent, name);
        var img = EnsureComponent<Image>(btn);
        img.color = fill;

        EnsureComponent<CanvasGroup>(btn);
        var b = EnsureComponent<Button>(btn);
        var col = b.colors;
        col.normalColor = Color.white;
        col.highlightedColor = Color.white;
        col.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        b.colors = col;
        b.transition = Selectable.Transition.ColorTint;
        b.targetGraphic = img;

        EnsureComponent<ButtonAnimator>(btn);

        var lbl = FindOrCreateChild(btn.GetComponent<RectTransform>(), "Label");
        var tmp = EnsureComponent<TextMeshProUGUI>(lbl);
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.characterSpacing = 6;
        tmp.raycastTarget = false;
        StretchFull(lbl.GetComponent<RectTransform>());

        return btn;
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
