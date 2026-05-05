using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Iteration03_Setup
{
    private const string ScenesFolder = "Assets/LaserGame/Scenes";
    private const string GamePath = "Assets/LaserGame/Scenes/Game.unity";

    private static readonly Color BgColor = new Color(0.058f, 0.058f, 0.117f, 1f);
    private static readonly Color FieldBgColor = new Color(0.04f, 0.04f, 0.09f, 1f);
    private static readonly Color CellColor = new Color(0.07f, 0.07f, 0.13f, 1f);
    private static readonly Color CyanNeon = new Color(0.2f, 0.95f, 1f, 1f);
    private static readonly Color CyanDim = new Color(0.2f, 0.95f, 1f, 0.4f);
    private static readonly Color MagentaNeon = new Color(1f, 0.25f, 0.85f, 1f);
    private static readonly Color YellowNeon = new Color(1f, 0.85f, 0.25f, 1f);
    private static readonly Color WhiteSoft = new Color(0.92f, 0.95f, 1f, 1f);
    private static readonly Color GreyDim = new Color(0.45f, 0.5f, 0.6f, 1f);
    private static readonly Color PanelColor = new Color(0.105f, 0.105f, 0.18f, 0.98f);

    [MenuItem("LaserGame/Iteration 03/Setup Game Scene")]
    public static void SetupGameScene()
    {
        EnsureFolders();
        var scene = OpenOrCreateScene(GamePath);
        CleanupPlaceholder(scene);
        BuildGameScene(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddSceneToBuildSettings(GamePath);
        Debug.Log("[Iteration 03] Game scene setup complete.");
    }

    [MenuItem("LaserGame/Iteration 03/Test 5x5 Grid")]
    public static void Test5x5()
    {
        SetGridConfig(5, 5, new Vector2Int(0, 2), new Vector2Int(1, 0));
    }

    [MenuItem("LaserGame/Iteration 03/Test 7x7 Grid")]
    public static void Test7x7()
    {
        SetGridConfig(7, 7, new Vector2Int(0, 3), new Vector2Int(1, 0));
    }

    [MenuItem("LaserGame/Iteration 03/Test 10x10 Grid")]
    public static void Test10x10()
    {
        SetGridConfig(10, 10, new Vector2Int(0, 5), new Vector2Int(1, 0));
    }

    [MenuItem("LaserGame/Iteration 03/Toggle Laser Direction")]
    public static void ToggleDirection()
    {
        if (!File.Exists(GamePath)) { Debug.LogWarning("Game scene not found. Run Setup Game Scene first."); return; }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null) { Debug.LogWarning("GameController not found."); return; }

        Vector2Int d = ctrl.testLevel.emitterDir;
        Vector2Int next;
        if (d == new Vector2Int(1, 0)) next = new Vector2Int(0, 1);
        else if (d == new Vector2Int(0, 1)) next = new Vector2Int(-1, 0);
        else if (d == new Vector2Int(-1, 0)) next = new Vector2Int(0, -1);
        else next = new Vector2Int(1, 0);

        Vector2Int cell;
        if (next == new Vector2Int(1, 0)) cell = new Vector2Int(0, ctrl.testLevel.rows / 2);
        else if (next == new Vector2Int(-1, 0)) cell = new Vector2Int(ctrl.testLevel.cols - 1, ctrl.testLevel.rows / 2);
        else if (next == new Vector2Int(0, 1)) cell = new Vector2Int(ctrl.testLevel.cols / 2, 0);
        else cell = new Vector2Int(ctrl.testLevel.cols / 2, ctrl.testLevel.rows - 1);

        ctrl.testLevel.emitterDir = next;
        ctrl.testLevel.emitterCell = cell;

        ApplyConfigToScene(ctrl);

        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 03] Direction toggled to (" + next.x + "," + next.y + "), cell (" + cell.x + "," + cell.y + ").");
    }

    private static void SetGridConfig(int cols, int rows, Vector2Int emitterCell, Vector2Int emitterDir)
    {
        if (!File.Exists(GamePath)) { Debug.LogWarning("Game scene not found. Run Setup Game Scene first."); return; }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null) { Debug.LogWarning("GameController not found."); return; }

        ctrl.testLevel = new LevelDefinition
        {
            cols = cols,
            rows = rows,
            emitterCell = emitterCell,
            emitterDir = emitterDir,
            mirrors = new System.Collections.Generic.List<MirrorPlacement>()
        };

        ApplyConfigToScene(ctrl);

        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 03] Grid set to " + cols + "x" + rows + ", emitter at (" + emitterCell.x + "," + emitterCell.y + ") dir (" + emitterDir.x + "," + emitterDir.y + ").");
    }

    private static void ApplyConfigToScene(GameController ctrl)
    {
        if (ctrl.grid != null)
        {
            ctrl.grid.cols = ctrl.testLevel.cols;
            ctrl.grid.rows = ctrl.testLevel.rows;
            ctrl.grid.Build();
        }
        if (ctrl.emitter != null && ctrl.grid != null)
        {
            ctrl.emitter.cell = ctrl.testLevel.emitterCell;
            ctrl.emitter.direction = ctrl.testLevel.emitterDir;
            ctrl.emitter.PlaceOnGrid(ctrl.grid);
        }
    }

    private static void CleanupPlaceholder(Scene scene)
    {
        var bridge = Object.FindObjectOfType<GameSceneBackBridge>();
        if (bridge != null) Object.DestroyImmediate(bridge.gameObject);

        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "GameCanvas")
            {
                DestroyChildIfExists(root.transform, "PlaceholderText");
                DestroyChildIfExists(root.transform, "BackButton");
                DestroyChildIfExists(root.transform, "BackBridge");
            }
        }
    }

    private static void DestroyChildIfExists(Transform parent, string name)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var c = parent.GetChild(i);
            if (c.name == name) Object.DestroyImmediate(c.gameObject);
        }
    }

    private static void BuildGameScene(Scene scene)
    {
        EnsureCamera(scene);
        EnsureEventSystem(scene);
        EnsurePersistentBootstrap(scene);

        var canvas = EnsureCanvas(scene, "GameCanvas", 0);
        var canvasRT = canvas.GetComponent<RectTransform>();

        var bg = FindOrCreateChild(canvasRT, "Background");
        var bgImg = EnsureComponent<Image>(bg);
        bgImg.color = BgColor;
        bgImg.raycastTarget = false;
        StretchFull(bg.GetComponent<RectTransform>());

        var topHud = BuildTopHud(canvasRT);
        var subHud = BuildSubHud(canvasRT);
        var field = BuildField(canvasRT);
        var resetBtn = BuildResetButton(canvasRT);

        var controllerHost = FindOrCreateChild(canvasRT, "GameController_Host");
        var hostRT = controllerHost.GetComponent<RectTransform>();
        hostRT.anchorMin = Vector2.zero;
        hostRT.anchorMax = Vector2.zero;
        hostRT.sizeDelta = Vector2.zero;
        hostRT.anchoredPosition = Vector2.zero;
        var ctrl = EnsureComponent<GameController>(controllerHost);

        ctrl.topHudGroup = topHud.group;
        ctrl.topHudRect = topHud.rect;
        ctrl.backButton = topHud.backButton;
        ctrl.levelText = topHud.levelText;
        ctrl.coinsText = topHud.coinsText;
        ctrl.subHudGroup = subHud.group;
        ctrl.subHudRect = subHud.rect;
        ctrl.movesText = subHud.movesText;

        ctrl.fieldRoot = field.fieldRect;
        ctrl.fieldGroup = field.fieldGroup;
        ctrl.grid = field.grid;
        ctrl.emitter = field.emitter;
        ctrl.rayRenderer = field.rayRenderer;

        ctrl.resetButton = resetBtn.GetComponent<Button>();
        ctrl.resetRect = resetBtn.GetComponent<RectTransform>();
        ctrl.resetGroup = resetBtn.GetComponent<CanvasGroup>();

        ctrl.testLevel = new LevelDefinition
        {
            cols = 5,
            rows = 5,
            emitterCell = new Vector2Int(0, 2),
            emitterDir = new Vector2Int(1, 0),
            mirrors = new System.Collections.Generic.List<MirrorPlacement>()
        };
        ctrl.levelSelectSceneName = "LevelSelect";

        ApplyConfigToScene(ctrl);
    }

    private struct TopHudResult
    {
        public CanvasGroup group;
        public RectTransform rect;
        public Button backButton;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI coinsText;
    }

    private static TopHudResult BuildTopHud(RectTransform parent)
    {
        var hud = FindOrCreateChild(parent, "TopHud");
        var hudRT = hud.GetComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0, 1);
        hudRT.anchorMax = new Vector2(1, 1);
        hudRT.pivot = new Vector2(0.5f, 1);
        hudRT.sizeDelta = new Vector2(0, 200);
        hudRT.anchoredPosition = Vector2.zero;
        var hudBg = EnsureComponent<Image>(hud);
        hudBg.color = new Color(BgColor.r, BgColor.g, BgColor.b, 0.92f);
        var group = EnsureComponent<CanvasGroup>(hud);

        var back = CreateBackPill(hudRT);
        var backRT = back.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0, 0.5f);
        backRT.anchorMax = new Vector2(0, 0.5f);
        backRT.pivot = new Vector2(0, 0.5f);
        backRT.sizeDelta = new Vector2(150, 100);
        backRT.anchoredPosition = new Vector2(30, -10);

        var level = FindOrCreateChild(hudRT, "LevelText");
        var levelRT = level.GetComponent<RectTransform>();
        levelRT.anchorMin = new Vector2(0.5f, 0.5f);
        levelRT.anchorMax = new Vector2(0.5f, 0.5f);
        levelRT.pivot = new Vector2(0.5f, 0.5f);
        levelRT.sizeDelta = new Vector2(500, 100);
        levelRT.anchoredPosition = new Vector2(0, -10);
        var levelTMP = EnsureComponent<TextMeshProUGUI>(level);
        levelTMP.text = "LEVEL 1";
        levelTMP.fontSize = 52;
        levelTMP.fontStyle = FontStyles.Bold;
        levelTMP.color = CyanNeon;
        levelTMP.alignment = TextAlignmentOptions.Center;
        levelTMP.characterSpacing = 8;
        levelTMP.raycastTarget = false;

        var coinsHud = FindOrCreateChild(hudRT, "CoinsHud");
        var coinsHudRT = coinsHud.GetComponent<RectTransform>();
        coinsHudRT.anchorMin = new Vector2(1, 0.5f);
        coinsHudRT.anchorMax = new Vector2(1, 0.5f);
        coinsHudRT.pivot = new Vector2(1, 0.5f);
        coinsHudRT.sizeDelta = new Vector2(200, 80);
        coinsHudRT.anchoredPosition = new Vector2(-30, -10);
        var coinsBg = EnsureComponent<Image>(coinsHud);
        coinsBg.color = PanelColor;
        coinsBg.raycastTarget = false;

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

        var coinsTextGo = FindOrCreateChild(coinsHudRT, "CoinsText");
        var coinsTextRT = coinsTextGo.GetComponent<RectTransform>();
        coinsTextRT.anchorMin = new Vector2(0, 0);
        coinsTextRT.anchorMax = new Vector2(1, 1);
        coinsTextRT.offsetMin = new Vector2(60, 0);
        coinsTextRT.offsetMax = new Vector2(-15, 0);
        var coinsTMP = EnsureComponent<TextMeshProUGUI>(coinsTextGo);
        coinsTMP.text = "0";
        coinsTMP.fontSize = 32;
        coinsTMP.alignment = TextAlignmentOptions.MidlineLeft;
        coinsTMP.color = WhiteSoft;
        coinsTMP.raycastTarget = false;

        return new TopHudResult
        {
            group = group,
            rect = hudRT,
            backButton = back.GetComponent<Button>(),
            levelText = levelTMP,
            coinsText = coinsTMP
        };
    }

    private struct SubHudResult
    {
        public CanvasGroup group;
        public RectTransform rect;
        public TextMeshProUGUI movesText;
    }

    private static SubHudResult BuildSubHud(RectTransform parent)
    {
        var sub = FindOrCreateChild(parent, "SubHud");
        var subRT = sub.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0, 1);
        subRT.anchorMax = new Vector2(1, 1);
        subRT.pivot = new Vector2(0.5f, 1);
        subRT.sizeDelta = new Vector2(0, 80);
        subRT.anchoredPosition = new Vector2(0, -200);
        var group = EnsureComponent<CanvasGroup>(sub);

        var movesGo = FindOrCreateChild(subRT, "MovesText");
        var movesRT = movesGo.GetComponent<RectTransform>();
        StretchFull(movesRT);
        var movesTMP = EnsureComponent<TextMeshProUGUI>(movesGo);
        movesTMP.text = "MOVES: 0";
        movesTMP.fontSize = 36;
        movesTMP.fontStyle = FontStyles.Bold;
        movesTMP.color = WhiteSoft;
        movesTMP.alignment = TextAlignmentOptions.Center;
        movesTMP.characterSpacing = 6;
        movesTMP.raycastTarget = false;

        return new SubHudResult { group = group, rect = subRT, movesText = movesTMP };
    }

    private struct FieldResult
    {
        public RectTransform fieldRect;
        public CanvasGroup fieldGroup;
        public GridSystem grid;
        public LaserEmitter emitter;
        public RayRenderer rayRenderer;
    }

    private static FieldResult BuildField(RectTransform parent)
    {
        var field = FindOrCreateChild(parent, "Field");
        var fieldRT = field.GetComponent<RectTransform>();
        fieldRT.anchorMin = new Vector2(0.5f, 0.5f);
        fieldRT.anchorMax = new Vector2(0.5f, 0.5f);
        fieldRT.pivot = new Vector2(0.5f, 0.5f);
        fieldRT.sizeDelta = new Vector2(1000, 1000);
        fieldRT.anchoredPosition = new Vector2(0, -50);
        var fieldGroup = EnsureComponent<CanvasGroup>(field);

        var fieldBg = FindOrCreateChild(fieldRT, "FieldBackground");
        var fieldBgImg = EnsureComponent<Image>(fieldBg);
        fieldBgImg.color = FieldBgColor;
        fieldBgImg.raycastTarget = false;
        StretchFull(fieldBg.GetComponent<RectTransform>());

        var cellsHolder = FindOrCreateChild(fieldRT, "CellsHolder");
        var cellsRT = cellsHolder.GetComponent<RectTransform>();
        StretchFull(cellsRT);

        var elements = FindOrCreateChild(fieldRT, "ElementsHolder");
        var elementsRT = elements.GetComponent<RectTransform>();
        StretchFull(elementsRT);

        var emitterHolder = FindOrCreateChild(fieldRT, "EmitterHolder");
        var emitterHolderRT = emitterHolder.GetComponent<RectTransform>();
        StretchFull(emitterHolderRT);

        var emitterGo = FindOrCreateChild(emitterHolderRT, "LaserEmitter");
        var emitterRT = emitterGo.GetComponent<RectTransform>();
        emitterRT.anchorMin = new Vector2(0.5f, 0.5f);
        emitterRT.anchorMax = new Vector2(0.5f, 0.5f);
        emitterRT.pivot = new Vector2(0.5f, 0.5f);
        emitterRT.sizeDelta = new Vector2(160, 160);

        var emitterGlowGo = FindOrCreateChild(emitterRT, "Glow");
        var emitterGlowImg = EnsureComponent<Image>(emitterGlowGo);
        emitterGlowImg.color = CyanDim;
        emitterGlowImg.raycastTarget = false;
        var emitterGlowRT = emitterGlowGo.GetComponent<RectTransform>();
        emitterGlowRT.anchorMin = Vector2.zero;
        emitterGlowRT.anchorMax = Vector2.one;
        emitterGlowRT.offsetMin = new Vector2(-25, -25);
        emitterGlowRT.offsetMax = new Vector2(25, 25);

        var bodyGo = FindOrCreateChild(emitterRT, "Body");
        var bodyImg = EnsureComponent<Image>(bodyGo);
        bodyImg.color = CyanNeon;
        bodyImg.raycastTarget = false;
        StretchFull(bodyGo.GetComponent<RectTransform>());

        var arrowGo = FindOrCreateChild(emitterRT, "Arrow");
        var arrowRT = arrowGo.GetComponent<RectTransform>();
        StretchFull(arrowRT);
        var arrowTMP = EnsureComponent<TextMeshProUGUI>(arrowGo);
        arrowTMP.text = "\u25B6";
        arrowTMP.fontSize = 110;
        arrowTMP.fontStyle = FontStyles.Bold;
        arrowTMP.color = BgColor;
        arrowTMP.alignment = TextAlignmentOptions.Center;
        arrowTMP.raycastTarget = false;

        emitterGlowGo.transform.SetSiblingIndex(0);
        bodyGo.transform.SetSiblingIndex(1);
        arrowGo.transform.SetSiblingIndex(2);

        var emitter = EnsureComponent<LaserEmitter>(emitterGo);
        emitter.bodyImage = bodyImg;
        emitter.glowImage = emitterGlowImg;
        emitter.arrowText = arrowTMP;
        emitter.rectTransform = emitterRT;

        var segmentsHolderGo = FindOrCreateChild(fieldRT, "SegmentsHolder");
        var segmentsRT = segmentsHolderGo.GetComponent<RectTransform>();
        StretchFull(segmentsRT);
        var pulseGroup = EnsureComponent<CanvasGroup>(segmentsHolderGo);
        var rayRenderer = EnsureComponent<RayRenderer>(segmentsHolderGo);
        rayRenderer.segmentsHolder = segmentsRT;
        rayRenderer.pulseGroup = pulseGroup;

        segmentsHolderGo.transform.SetSiblingIndex(2);

        var grid = EnsureComponent<GridSystem>(field);
        grid.fieldRect = fieldRT;
        grid.cellsHolder = cellsRT;
        grid.cols = 5;
        grid.rows = 5;

        return new FieldResult
        {
            fieldRect = fieldRT,
            fieldGroup = fieldGroup,
            grid = grid,
            emitter = emitter,
            rayRenderer = rayRenderer
        };
    }

    private static GameObject BuildResetButton(RectTransform parent)
    {
        var btn = FindOrCreateChild(parent, "ResetButton");
        var img = EnsureComponent<Image>(btn);
        img.color = MagentaNeon;

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
        tmp.text = "RESET";
        tmp.fontSize = 50;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = BgColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.characterSpacing = 8;
        tmp.raycastTarget = false;
        StretchFull(lbl.GetComponent<RectTransform>());

        var btnRT = btn.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0);
        btnRT.anchorMax = new Vector2(0.5f, 0);
        btnRT.pivot = new Vector2(0.5f, 0);
        btnRT.sizeDelta = new Vector2(380, 130);
        btnRT.anchoredPosition = new Vector2(0, 90);

        return btn;
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
