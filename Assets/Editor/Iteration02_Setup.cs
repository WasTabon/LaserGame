using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Iteration02_Setup
{
    private const string ScenesFolder = "Assets/LaserGame/Scenes";
    private const string MainMenuPath = "Assets/LaserGame/Scenes/MainMenu.unity";
    private const string GamePath = "Assets/LaserGame/Scenes/Game.unity";
    private const string LevelSelectPath = "Assets/LaserGame/Scenes/LevelSelect.unity";

    private static readonly Color BgColor = new Color(0.058f, 0.058f, 0.117f, 1f);
    private static readonly Color BgColorAccent = new Color(0.078f, 0.078f, 0.156f, 1f);
    private static readonly Color CyanNeon = new Color(0.2f, 0.95f, 1f, 1f);
    private static readonly Color MagentaNeon = new Color(1f, 0.25f, 0.85f, 1f);
    private static readonly Color YellowNeon = new Color(1f, 0.85f, 0.25f, 1f);
    private static readonly Color WhiteSoft = new Color(0.92f, 0.95f, 1f, 1f);
    private static readonly Color GreyDim = new Color(0.45f, 0.5f, 0.6f, 1f);
    private static readonly Color PanelColor = new Color(0.105f, 0.105f, 0.18f, 0.98f);

    [MenuItem("LaserGame/Iteration 02/Setup Level Select Scene")]
    public static void SetupLevelSelectScene()
    {
        EnsureFolders();
        var scene = OpenOrCreateScene(LevelSelectPath);
        BuildLevelSelect(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddSceneToBuildSettings(LevelSelectPath);
        UpdateMainMenuLink();
        UpdateGameBackLink();
        EnsureBuildSettingsOrder();
        EditorSceneManager.OpenScene(LevelSelectPath, OpenSceneMode.Single);
        Debug.Log("[Iteration 02] Level Select scene + cross-scene links updated.");
    }

    [MenuItem("LaserGame/Iteration 02/Unlock All Levels (Test)")]
    public static void UnlockAllLevels()
    {
        SaveSystem.Load();
        SaveSystem.Data.unlockedLevel = 30;
        SaveSystem.Save();
        Debug.Log("[Iteration 02] All 30 levels unlocked in save data.");
    }

    [MenuItem("LaserGame/Iteration 02/Set Random Stars (Test)")]
    public static void SetRandomStars()
    {
        SaveSystem.Load();
        var d = SaveSystem.Data;
        d.unlockedLevel = 30;
        d.levelProgress.Clear();
        for (int i = 1; i <= 30; i++)
        {
            int s = Random.Range(0, 4);
            if (s > 0) d.SetStarsForLevel(i, s);
        }
        SaveSystem.Save();
        Debug.Log("[Iteration 02] Random stars set for all levels.");
    }

    [MenuItem("LaserGame/Iteration 02/Reset Levels Progress")]
    public static void ResetLevels()
    {
        SaveSystem.Load();
        SaveSystem.Data.unlockedLevel = 1;
        SaveSystem.Data.levelProgress.Clear();
        SaveSystem.Save();
        Debug.Log("[Iteration 02] Level progress reset.");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/LaserGame")) AssetDatabase.CreateFolder("Assets", "LaserGame");
        if (!AssetDatabase.IsValidFolder("Assets/LaserGame/Scenes")) AssetDatabase.CreateFolder("Assets/LaserGame", "Scenes");
        if (!AssetDatabase.IsValidFolder("Assets/LaserGame/Scripts")) AssetDatabase.CreateFolder("Assets/LaserGame", "Scripts");
    }

    private static Scene OpenOrCreateScene(string path)
    {
        if (File.Exists(path)) return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, path);
        return scene;
    }

    private static void AddSceneToBuildSettings(string path)
    {
        if (!File.Exists(path)) return;
        var current = EditorBuildSettings.scenes;
        foreach (var s in current)
        {
            if (s.path == path) return;
        }
        var list = new List<EditorBuildSettingsScene>(current);
        list.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();
    }

    private static void EnsureBuildSettingsOrder()
    {
        var desired = new List<string> { MainMenuPath, LevelSelectPath, GamePath };
        var existing = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var ordered = new List<EditorBuildSettingsScene>();
        foreach (var path in desired)
        {
            var found = existing.Find(s => s.path == path);
            if (found != null && File.Exists(path))
            {
                ordered.Add(new EditorBuildSettingsScene(path, true));
                existing.Remove(found);
            }
        }
        ordered.AddRange(existing);
        EditorBuildSettings.scenes = ordered.ToArray();
    }

    private static void UpdateMainMenuLink()
    {
        if (!File.Exists(MainMenuPath)) return;
        var scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<MainMenuController>();
        if (ctrl != null)
        {
            ctrl.nextSceneName = "LevelSelect";
            EditorUtility.SetDirty(ctrl);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Iteration 02] MainMenu Play button now leads to LevelSelect.");
        }
    }

    private static void UpdateGameBackLink()
    {
        if (!File.Exists(GamePath)) return;
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var bridge = Object.FindObjectOfType<GameSceneBackBridge>();
        if (bridge != null)
        {
            bridge.targetSceneName = "LevelSelect";
            EditorUtility.SetDirty(bridge);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Iteration 02] Game scene Back button now leads to LevelSelect.");
        }
    }

    private static void BuildLevelSelect(Scene scene)
    {
        EnsureCamera(scene);
        EnsureEventSystem(scene);
        EnsurePersistentBootstrap(scene);

        var canvas = EnsureCanvas(scene, "LevelSelectCanvas", 0);
        var canvasRT = canvas.GetComponent<RectTransform>();

        var bg = FindOrCreateChild(canvasRT, "Background");
        var bgImg = EnsureComponent<Image>(bg);
        bgImg.color = BgColor;
        bgImg.raycastTarget = false;
        StretchFull(bg.GetComponent<RectTransform>());

        var scrollGo = BuildScrollView(canvasRT);
        var scrollRect = scrollGo.GetComponent<ScrollRect>();
        var contentRT = scrollRect.content;

        var template = BuildLevelButtonTemplate(canvasRT);

        var hud = BuildTopHud(canvasRT);

        var controllerHost = FindOrCreateChild(canvasRT, "LevelSelectController_Host");
        var controllerHostRT = controllerHost.GetComponent<RectTransform>();
        controllerHostRT.anchorMin = Vector2.zero;
        controllerHostRT.anchorMax = Vector2.zero;
        controllerHostRT.sizeDelta = Vector2.zero;
        controllerHostRT.anchoredPosition = Vector2.zero;
        var controller = EnsureComponent<LevelSelectController>(controllerHost);
        controller.scrollRect = scrollRect;
        controller.contentRect = contentRT;
        controller.contentGroup = EnsureComponent<CanvasGroup>(scrollGo);
        controller.buttonTemplate = template.GetComponent<LevelButton>();
        controller.hudGroup = hud.group;
        controller.hudRect = hud.rect;
        controller.backButton = hud.backButton;
        controller.coinsText = hud.coinsText;
        controller.totalLevels = 30;
        controller.mainMenuSceneName = "MainMenu";
        controller.gameSceneName = "Game";
    }

    private static GameObject BuildScrollView(RectTransform parent)
    {
        var scroll = FindOrCreateChild(parent, "ScrollView");
        var scrollRT = scroll.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0, 0);
        scrollRT.anchorMax = new Vector2(1, 1);
        scrollRT.pivot = new Vector2(0.5f, 0.5f);
        scrollRT.offsetMin = new Vector2(0, 80);
        scrollRT.offsetMax = new Vector2(0, -220);

        var scrollImg = EnsureComponent<Image>(scroll);
        scrollImg.color = new Color(0, 0, 0, 0);
        scrollImg.raycastTarget = true;

        var scrollRect = EnsureComponent<ScrollRect>(scroll);
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.135f;
        scrollRect.scrollSensitivity = 30f;

        var viewport = FindOrCreateChild(scrollRT, "Viewport");
        var viewportRT = viewport.GetComponent<RectTransform>();
        StretchFull(viewportRT);
        var viewportImg = EnsureComponent<Image>(viewport);
        viewportImg.color = new Color(1, 1, 1, 0.003f);
        var mask = EnsureComponent<Mask>(viewport);
        mask.showMaskGraphic = false;

        var content = FindOrCreateChild(viewportRT, "Content");
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta = new Vector2(0, 1000);

        var grid = EnsureComponent<GridLayoutGroup>(content);
        grid.padding = new RectOffset(30, 30, 30, 60);
        grid.cellSize = new Vector2(320, 320);
        grid.spacing = new Vector2(25, 25);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        var fitter = EnsureComponent<ContentSizeFitter>(content);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRT;
        scrollRect.content = contentRT;

        EnsureComponent<CanvasGroup>(scroll);

        return scroll;
    }

    private static GameObject BuildLevelButtonTemplate(RectTransform parent)
    {
        var tpl = FindOrCreateChild(parent, "LevelButtonTemplate");
        var tplRT = tpl.GetComponent<RectTransform>();
        tplRT.sizeDelta = new Vector2(320, 320);

        var bg = EnsureComponent<Image>(tpl);
        bg.color = PanelColor;
        bg.raycastTarget = true;

        EnsureComponent<CanvasGroup>(tpl);
        var btn = EnsureComponent<Button>(tpl);
        btn.transition = Selectable.Transition.None;
        btn.targetGraphic = bg;

        var border = FindOrCreateChild(tplRT, "Border");
        var borderImg = EnsureComponent<Image>(border);
        borderImg.color = CyanNeon;
        borderImg.raycastTarget = false;
        var borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = new Vector2(-4, -4);
        borderRT.offsetMax = new Vector2(4, 4);
        border.transform.SetAsFirstSibling();

        var number = FindOrCreateChild(tplRT, "Number");
        var numberRT = number.GetComponent<RectTransform>();
        numberRT.anchorMin = new Vector2(0, 0.3f);
        numberRT.anchorMax = new Vector2(1, 1);
        numberRT.offsetMin = Vector2.zero;
        numberRT.offsetMax = Vector2.zero;
        var numberTMP = EnsureComponent<TextMeshProUGUI>(number);
        numberTMP.text = "1";
        numberTMP.fontSize = 130;
        numberTMP.fontStyle = FontStyles.Bold;
        numberTMP.color = CyanNeon;
        numberTMP.alignment = TextAlignmentOptions.Center;
        numberTMP.raycastTarget = false;

        var lockText = FindOrCreateChild(tplRT, "LockText");
        var lockRT = lockText.GetComponent<RectTransform>();
        StretchFull(lockRT);
        var lockTMP = EnsureComponent<TextMeshProUGUI>(lockText);
        lockTMP.text = "LOCKED";
        lockTMP.fontSize = 48;
        lockTMP.fontStyle = FontStyles.Bold;
        lockTMP.color = GreyDim;
        lockTMP.alignment = TextAlignmentOptions.Center;
        lockTMP.characterSpacing = 8;
        lockTMP.raycastTarget = false;

        var starsRow = FindOrCreateChild(tplRT, "StarsRow");
        var starsRT = starsRow.GetComponent<RectTransform>();
        starsRT.anchorMin = new Vector2(0, 0);
        starsRT.anchorMax = new Vector2(1, 0.3f);
        starsRT.offsetMin = new Vector2(20, 15);
        starsRT.offsetMax = new Vector2(-20, 0);
        var starsLayout = EnsureComponent<HorizontalLayoutGroup>(starsRow);
        starsLayout.childAlignment = TextAnchor.MiddleCenter;
        starsLayout.spacing = 15;
        starsLayout.childForceExpandWidth = false;
        starsLayout.childForceExpandHeight = false;
        starsLayout.childControlWidth = false;
        starsLayout.childControlHeight = false;

        var stars = new TextMeshProUGUI[3];
        for (int i = 0; i < 3; i++)
        {
            var s = FindOrCreateChild(starsRT, "Star_" + i);
            var sRT = s.GetComponent<RectTransform>();
            sRT.sizeDelta = new Vector2(60, 60);
            var sTMP = EnsureComponent<TextMeshProUGUI>(s);
            sTMP.text = "\u2605";
            sTMP.fontSize = 60;
            sTMP.fontStyle = FontStyles.Bold;
            sTMP.color = YellowNeon;
            sTMP.alignment = TextAlignmentOptions.Center;
            sTMP.raycastTarget = false;
            stars[i] = sTMP;
        }

        var lb = EnsureComponent<LevelButton>(tpl);
        lb.background = bg;
        lb.border = borderImg;
        lb.numberText = numberTMP;
        lb.lockText = lockTMP;
        lb.starTexts = stars;
        lb.button = btn;
        lb.canvasGroup = tpl.GetComponent<CanvasGroup>();
        lb.rectTransform = tplRT;

        tpl.SetActive(false);
        return tpl;
    }

    private struct HudResult
    {
        public CanvasGroup group;
        public RectTransform rect;
        public Button backButton;
        public TextMeshProUGUI coinsText;
    }

    private static HudResult BuildTopHud(RectTransform parent)
    {
        var hud = FindOrCreateChild(parent, "TopHud");
        var hudRT = hud.GetComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0, 1);
        hudRT.anchorMax = new Vector2(1, 1);
        hudRT.pivot = new Vector2(0.5f, 1);
        hudRT.sizeDelta = new Vector2(0, 200);
        hudRT.anchoredPosition = new Vector2(0, 0);

        var hudBg = EnsureComponent<Image>(hud);
        hudBg.color = new Color(BgColor.r, BgColor.g, BgColor.b, 0.92f);
        hudBg.raycastTarget = true;

        var group = EnsureComponent<CanvasGroup>(hud);

        var back = CreateBackPill(hudRT);
        var backRT = back.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0, 0.5f);
        backRT.anchorMax = new Vector2(0, 0.5f);
        backRT.pivot = new Vector2(0, 0.5f);
        backRT.sizeDelta = new Vector2(150, 100);
        backRT.anchoredPosition = new Vector2(30, -10);

        var title = FindOrCreateChild(hudRT, "Title");
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 0.5f);
        titleRT.anchorMax = new Vector2(0.5f, 0.5f);
        titleRT.pivot = new Vector2(0.5f, 0.5f);
        titleRT.sizeDelta = new Vector2(600, 100);
        titleRT.anchoredPosition = new Vector2(0, -10);
        var titleTMP = EnsureComponent<TextMeshProUGUI>(title);
        titleTMP.text = "SELECT LEVEL";
        titleTMP.fontSize = 52;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = CyanNeon;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.characterSpacing = 8;
        titleTMP.raycastTarget = false;

        var coinsHud = FindOrCreateChild(hudRT, "CoinsHud");
        var coinsHudRT = coinsHud.GetComponent<RectTransform>();
        coinsHudRT.anchorMin = new Vector2(1, 0.5f);
        coinsHudRT.anchorMax = new Vector2(1, 0.5f);
        coinsHudRT.pivot = new Vector2(1, 0.5f);
        coinsHudRT.sizeDelta = new Vector2(200, 80);
        coinsHudRT.anchoredPosition = new Vector2(-30, -10);
        var coinsHudBg = EnsureComponent<Image>(coinsHud);
        coinsHudBg.color = PanelColor;
        coinsHudBg.raycastTarget = false;

        var coinsIcon = FindOrCreateChild(coinsHudRT, "CoinIcon");
        var coinsIconImg = EnsureComponent<Image>(coinsIcon);
        coinsIconImg.color = YellowNeon;
        coinsIconImg.raycastTarget = false;
        var coinsIconRT = coinsIcon.GetComponent<RectTransform>();
        coinsIconRT.anchorMin = new Vector2(0, 0.5f);
        coinsIconRT.anchorMax = new Vector2(0, 0.5f);
        coinsIconRT.pivot = new Vector2(0.5f, 0.5f);
        coinsIconRT.sizeDelta = new Vector2(36, 36);
        coinsIconRT.anchoredPosition = new Vector2(30, 0);

        var coinsText = FindOrCreateChild(coinsHudRT, "CoinsText");
        var coinsTextRT = coinsText.GetComponent<RectTransform>();
        coinsTextRT.anchorMin = new Vector2(0, 0);
        coinsTextRT.anchorMax = new Vector2(1, 1);
        coinsTextRT.offsetMin = new Vector2(60, 0);
        coinsTextRT.offsetMax = new Vector2(-15, 0);
        var coinsTMP = EnsureComponent<TextMeshProUGUI>(coinsText);
        coinsTMP.text = "0";
        coinsTMP.fontSize = 32;
        coinsTMP.alignment = TextAlignmentOptions.MidlineLeft;
        coinsTMP.color = WhiteSoft;
        coinsTMP.raycastTarget = false;

        return new HudResult
        {
            group = group,
            rect = hudRT,
            backButton = back.GetComponent<Button>(),
            coinsText = coinsTMP
        };
    }

    private static GameObject CreateBackPill(RectTransform parent)
    {
        var btn = FindOrCreateChild(parent, "BackButton");
        var img = EnsureComponent<Image>(btn);
        img.color = PanelColor;

        EnsureComponent<CanvasGroup>(btn);
        var b = EnsureComponent<Button>(btn);
        var col = b.colors;
        col.normalColor = Color.white;
        col.highlightedColor = Color.white;
        col.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        b.colors = col;
        b.transition = Selectable.Transition.ColorTint;

        EnsureComponent<ButtonAnimator>(btn);

        var lbl = FindOrCreateChild(btn.GetComponent<RectTransform>(), "Label");
        var tmp = EnsureComponent<TextMeshProUGUI>(lbl);
        tmp.text = "<";
        tmp.fontSize = 60;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = MagentaNeon;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        StretchFull(lbl.GetComponent<RectTransform>());

        return btn;
    }

    private static void EnsureCamera(Scene scene)
    {
        var cam = Object.FindObjectOfType<Camera>();
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            go.tag = "MainCamera";
            SceneManager.MoveGameObjectToScene(go, scene);
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BgColor;
        cam.orthographic = true;
    }

    private static void EnsureEventSystem(Scene scene)
    {
        var es = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (es != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<UnityEngine.EventSystems.EventSystem>();
        go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        SceneManager.MoveGameObjectToScene(go, scene);
    }

    private static GameObject EnsureCanvas(Scene scene, string name, int sortOrder)
    {
        GameObject existing = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name) { existing = root; break; }
        }
        if (existing == null)
        {
            existing = new GameObject(name);
            SceneManager.MoveGameObjectToScene(existing, scene);
        }
        var canvas = EnsureComponent<Canvas>(existing);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;
        var scaler = EnsureComponent<CanvasScaler>(existing);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        EnsureComponent<GraphicRaycaster>(existing);
        return existing;
    }

    private static void EnsurePersistentBootstrap(Scene scene)
    {
        var audio = EnsureRootObject(scene, "AudioManager");
        EnsureComponent<AudioManager>(audio);

        var transition = EnsureRootObject(scene, "SceneTransitionManager");
        var transMgr = EnsureComponent<SceneTransitionManager>(transition);

        var fadeCanvasGo = FindOrCreateChild(transition.transform, "FadeCanvas");
        var fadeCanvas = EnsureComponent<Canvas>(fadeCanvasGo);
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999;
        EnsureComponent<CanvasScaler>(fadeCanvasGo);
        EnsureComponent<GraphicRaycaster>(fadeCanvasGo);

        var fadeImageGo = FindOrCreateChild(fadeCanvasGo.transform, "FadeImage");
        var fadeImg = EnsureComponent<Image>(fadeImageGo);
        fadeImg.color = new Color(0, 0, 0, 0);
        StretchFull(fadeImageGo.GetComponent<RectTransform>());

        transMgr.fadeCanvas = fadeCanvas;
        transMgr.fadeImage = fadeImg;
    }

    private static GameObject EnsureRootObject(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name) return root;
        }
        var go = new GameObject(name);
        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
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
