using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Iteration01_Setup
{
    private const string ScenesFolder = "Assets/LaserGame/Scenes";
    private const string MainMenuPath = "Assets/LaserGame/Scenes/MainMenu.unity";
    private const string GamePath = "Assets/LaserGame/Scenes/Game.unity";

    private static readonly Color BgColor = new Color(0.058f, 0.058f, 0.117f, 1f);
    private static readonly Color BgColorAccent = new Color(0.078f, 0.078f, 0.156f, 1f);
    private static readonly Color CyanNeon = new Color(0.2f, 0.95f, 1f, 1f);
    private static readonly Color MagentaNeon = new Color(1f, 0.25f, 0.85f, 1f);
    private static readonly Color YellowNeon = new Color(1f, 0.85f, 0.25f, 1f);
    private static readonly Color WhiteSoft = new Color(0.92f, 0.95f, 1f, 1f);
    private static readonly Color GreyDim = new Color(0.45f, 0.5f, 0.6f, 1f);
    private static readonly Color PanelColor = new Color(0.105f, 0.105f, 0.18f, 0.98f);

    [MenuItem("LaserGame/Iteration 01/Setup Main Menu Scene")]
    public static void SetupMainMenuScene()
    {
        EnsureFolders();
        var scene = OpenOrCreateScene(MainMenuPath);
        BuildMainMenu(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddSceneToBuildSettings(MainMenuPath);
        AddSceneToBuildSettings(GamePath);
        Debug.Log("[Iteration 01] Main Menu scene setup complete.");
    }

    [MenuItem("LaserGame/Iteration 01/Setup Game Scene (placeholder)")]
    public static void SetupGameScene()
    {
        EnsureFolders();
        var scene = OpenOrCreateScene(GamePath);
        BuildGamePlaceholder(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddSceneToBuildSettings(MainMenuPath);
        AddSceneToBuildSettings(GamePath);
        Debug.Log("[Iteration 01] Game placeholder scene setup complete.");
    }

    [MenuItem("LaserGame/Iteration 01/Setup Both Scenes")]
    public static void SetupBoth()
    {
        SetupGameScene();
        SetupMainMenuScene();
    }

    [MenuItem("LaserGame/Iteration 01/Reset Save Data")]
    public static void ResetSave()
    {
        PlayerPrefs.DeleteKey("LaserGame_Save");
        PlayerPrefs.Save();
        Debug.Log("[Iteration 01] Save data reset.");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/LaserGame")) AssetDatabase.CreateFolder("Assets", "LaserGame");
        if (!AssetDatabase.IsValidFolder("Assets/LaserGame/Scenes")) AssetDatabase.CreateFolder("Assets/LaserGame", "Scenes");
        if (!AssetDatabase.IsValidFolder("Assets/LaserGame/Scripts")) AssetDatabase.CreateFolder("Assets/LaserGame", "Scripts");
    }

    private static Scene OpenOrCreateScene(string path)
    {
        if (File.Exists(path))
        {
            return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }
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

    private static void BuildMainMenu(Scene scene)
    {
        EnsureCamera(scene);
        EnsureEventSystem(scene);
        EnsurePersistentBootstrap(scene);

        var canvas = EnsureCanvas(scene, "MainMenuCanvas", 0);
        var canvasRT = canvas.GetComponent<RectTransform>();

        var bg = FindOrCreateChild(canvasRT, "Background");
        var bgImg = EnsureComponent<Image>(bg);
        bgImg.color = BgColor;
        bgImg.raycastTarget = false;
        StretchFull(bg.GetComponent<RectTransform>());

        var bgAccent = FindOrCreateChild(canvasRT, "BackgroundAccent");
        var bgAccentImg = EnsureComponent<Image>(bgAccent);
        bgAccentImg.color = new Color(CyanNeon.r, CyanNeon.g, CyanNeon.b, 0.04f);
        bgAccentImg.raycastTarget = false;
        var bgAccentRT = bgAccent.GetComponent<RectTransform>();
        bgAccentRT.anchorMin = new Vector2(0.5f, 0.5f);
        bgAccentRT.anchorMax = new Vector2(0.5f, 0.5f);
        bgAccentRT.pivot = new Vector2(0.5f, 0.5f);
        bgAccentRT.sizeDelta = new Vector2(900, 900);
        bgAccentRT.anchoredPosition = new Vector2(0, 100);

        var coinsHud = FindOrCreateChild(canvasRT, "CoinsHud");
        var coinsHudRT = coinsHud.GetComponent<RectTransform>();
        coinsHudRT.anchorMin = new Vector2(1, 1);
        coinsHudRT.anchorMax = new Vector2(1, 1);
        coinsHudRT.pivot = new Vector2(1, 1);
        coinsHudRT.sizeDelta = new Vector2(200, 70);
        coinsHudRT.anchoredPosition = new Vector2(-30, -50);
        var coinsHudGroup = EnsureComponent<CanvasGroup>(coinsHud);
        var coinsHudBg = EnsureComponent<Image>(coinsHud);
        coinsHudBg.color = PanelColor;

        var coinsIcon = FindOrCreateChild(coinsHudRT, "CoinIcon");
        var coinsIconImg = EnsureComponent<Image>(coinsIcon);
        coinsIconImg.color = YellowNeon;
        var coinsIconRT = coinsIcon.GetComponent<RectTransform>();
        coinsIconRT.anchorMin = new Vector2(0, 0.5f);
        coinsIconRT.anchorMax = new Vector2(0, 0.5f);
        coinsIconRT.pivot = new Vector2(0.5f, 0.5f);
        coinsIconRT.sizeDelta = new Vector2(40, 40);
        coinsIconRT.anchoredPosition = new Vector2(35, 0);

        var coinsText = FindOrCreateChild(coinsHudRT, "CoinsText");
        var coinsTMP = EnsureComponent<TextMeshProUGUI>(coinsText);
        coinsTMP.text = "0";
        coinsTMP.fontSize = 32;
        coinsTMP.alignment = TextAlignmentOptions.MidlineLeft;
        coinsTMP.color = WhiteSoft;
        var coinsTextRT = coinsText.GetComponent<RectTransform>();
        coinsTextRT.anchorMin = new Vector2(0, 0);
        coinsTextRT.anchorMax = new Vector2(1, 1);
        coinsTextRT.pivot = new Vector2(0.5f, 0.5f);
        coinsTextRT.offsetMin = new Vector2(70, 0);
        coinsTextRT.offsetMax = new Vector2(-15, 0);

        var title = FindOrCreateChild(canvasRT, "Title");
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1f);
        titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.sizeDelta = new Vector2(700, 200);
        titleRT.anchoredPosition = new Vector2(0, -260);
        var titleGroup = EnsureComponent<CanvasGroup>(title);
        var titleTMP = EnsureComponent<TextMeshProUGUI>(title);
        titleTMP.text = "LASER";
        titleTMP.fontSize = 140;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = CyanNeon;
        titleTMP.enableVertexGradient = false;

        var subtitle = FindOrCreateChild(titleRT, "Subtitle");
        var subRT = subtitle.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0.5f, 0);
        subRT.anchorMax = new Vector2(0.5f, 0);
        subRT.pivot = new Vector2(0.5f, 1);
        subRT.sizeDelta = new Vector2(700, 60);
        subRT.anchoredPosition = new Vector2(0, -10);
        var subTMP = EnsureComponent<TextMeshProUGUI>(subtitle);
        subTMP.text = "GAME";
        subTMP.fontSize = 60;
        subTMP.fontStyle = FontStyles.Bold;
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.color = MagentaNeon;
        subTMP.characterSpacing = 20;

        var playBtn = CreatePillButton(canvasRT, "PlayButton", "PLAY", CyanNeon, BgColor);
        var playRT = playBtn.GetComponent<RectTransform>();
        playRT.anchorMin = new Vector2(0.5f, 0.5f);
        playRT.anchorMax = new Vector2(0.5f, 0.5f);
        playRT.pivot = new Vector2(0.5f, 0.5f);
        playRT.sizeDelta = new Vector2(420, 130);
        playRT.anchoredPosition = new Vector2(0, -180);

        var settingsBtn = CreateIconButton(canvasRT, "SettingsButton", "SETTINGS");
        var settingsRT = settingsBtn.GetComponent<RectTransform>();
        settingsRT.anchorMin = new Vector2(0.5f, 0.5f);
        settingsRT.anchorMax = new Vector2(0.5f, 0.5f);
        settingsRT.pivot = new Vector2(0.5f, 0.5f);
        settingsRT.sizeDelta = new Vector2(280, 90);
        settingsRT.anchoredPosition = new Vector2(0, -340);

        var settingsPopupGo = BuildSettingsPopup(canvasRT);

        var controllerHost = FindOrCreateChild(canvasRT, "MainMenuController_Host");
        var controllerRT = controllerHost.GetComponent<RectTransform>();
        controllerRT.anchorMin = Vector2.zero;
        controllerRT.anchorMax = Vector2.zero;
        controllerRT.sizeDelta = Vector2.zero;
        controllerRT.anchoredPosition = Vector2.zero;
        var controller = EnsureComponent<MainMenuController>(controllerHost);
        controller.titleRect = titleRT;
        controller.titleGroup = titleGroup;
        controller.playButtonRect = playRT;
        controller.playButtonGroup = EnsureComponent<CanvasGroup>(playBtn);
        controller.settingsButtonRect = settingsRT;
        controller.settingsButtonGroup = EnsureComponent<CanvasGroup>(settingsBtn);
        controller.coinsHudRect = coinsHudRT;
        controller.coinsHudGroup = coinsHudGroup;
        controller.coinsText = coinsTMP;
        controller.playButton = playBtn.GetComponent<Button>();
        controller.settingsButton = settingsBtn.GetComponent<Button>();
        controller.settingsPopup = settingsPopupGo.GetComponent<SettingsPopup>();
        controller.gameSceneName = "Game";
    }

    private static GameObject BuildSettingsPopup(RectTransform parent)
    {
        var popup = FindOrCreateChild(parent, "SettingsPopup");
        var popupRT = popup.GetComponent<RectTransform>();
        StretchFull(popupRT);
        var popupGroup = EnsureComponent<CanvasGroup>(popup);
        popupGroup.alpha = 0;
        popupGroup.blocksRaycasts = false;

        var backdrop = FindOrCreateChild(popupRT, "Backdrop");
        var backdropImg = EnsureComponent<Image>(backdrop);
        backdropImg.color = new Color(0, 0, 0, 0.65f);
        StretchFull(backdrop.GetComponent<RectTransform>());
        var backdropBtn = EnsureComponent<Button>(backdrop);
        var backdropColors = backdropBtn.colors;
        backdropColors.normalColor = new Color(1, 1, 1, 1);
        backdropColors.highlightedColor = new Color(1, 1, 1, 1);
        backdropColors.pressedColor = new Color(1, 1, 1, 1);
        backdropBtn.colors = backdropColors;
        backdropBtn.transition = Selectable.Transition.None;

        var content = FindOrCreateChild(popupRT, "Content");
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.5f, 0.5f);
        contentRT.anchorMax = new Vector2(0.5f, 0.5f);
        contentRT.pivot = new Vector2(0.5f, 0.5f);
        contentRT.sizeDelta = new Vector2(720, 820);
        contentRT.anchoredPosition = Vector2.zero;
        var contentImg = EnsureComponent<Image>(content);
        contentImg.color = PanelColor;

        var contentBorder = FindOrCreateChild(contentRT, "Border");
        var borderImg = EnsureComponent<Image>(contentBorder);
        borderImg.color = new Color(CyanNeon.r, CyanNeon.g, CyanNeon.b, 0.25f);
        borderImg.raycastTarget = false;
        var borderRT = contentBorder.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = new Vector2(-3, -3);
        borderRT.offsetMax = new Vector2(3, 3);
        borderRT.SetAsFirstSibling();

        var titleHolder = FindOrCreateChild(contentRT, "Title");
        var titleHRT = titleHolder.GetComponent<RectTransform>();
        titleHRT.anchorMin = new Vector2(0, 1);
        titleHRT.anchorMax = new Vector2(1, 1);
        titleHRT.pivot = new Vector2(0.5f, 1);
        titleHRT.sizeDelta = new Vector2(0, 110);
        titleHRT.anchoredPosition = new Vector2(0, -20);
        var titleTMP = EnsureComponent<TextMeshProUGUI>(titleHolder);
        titleTMP.text = "SETTINGS";
        titleTMP.fontSize = 64;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = CyanNeon;
        titleTMP.alignment = TextAlignmentOptions.Center;

        var soundRow = BuildSettingsRow(contentRT, "SoundRow", "SOUND", new Vector2(0, -160));
        var musicRow = BuildSettingsRow(contentRT, "MusicRow", "MUSIC", new Vector2(0, -290));
        var hapticsRow = BuildSettingsRow(contentRT, "HapticsRow", "HAPTICS", new Vector2(0, -420));

        var closeBtn = CreatePillButton(contentRT, "CloseButton", "CLOSE", MagentaNeon, BgColor);
        var closeRT = closeBtn.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(0.5f, 0);
        closeRT.anchorMax = new Vector2(0.5f, 0);
        closeRT.pivot = new Vector2(0.5f, 0);
        closeRT.sizeDelta = new Vector2(360, 110);
        closeRT.anchoredPosition = new Vector2(0, 50);

        var sp = EnsureComponent<SettingsPopup>(popup);
        sp.canvasGroup = popupGroup;
        sp.contentRect = contentRT;
        sp.backdrop = backdropImg;
        sp.soundToggle = soundRow.GetComponentInChildren<Toggle>(true);
        sp.musicToggle = musicRow.GetComponentInChildren<Toggle>(true);
        sp.hapticsToggle = hapticsRow.GetComponentInChildren<Toggle>(true);
        sp.closeButton = closeBtn.GetComponent<Button>();
        sp.backdropButton = backdropBtn;

        popup.SetActive(false);

        return popup;
    }

    private static GameObject BuildSettingsRow(RectTransform parent, string name, string label, Vector2 anchoredPos)
    {
        var row = FindOrCreateChild(parent, name);
        var rowRT = row.GetComponent<RectTransform>();
        rowRT.anchorMin = new Vector2(0, 1);
        rowRT.anchorMax = new Vector2(1, 1);
        rowRT.pivot = new Vector2(0.5f, 1);
        rowRT.sizeDelta = new Vector2(-80, 100);
        rowRT.anchoredPosition = anchoredPos;
        var rowImg = EnsureComponent<Image>(row);
        rowImg.color = BgColorAccent;

        var lbl = FindOrCreateChild(rowRT, "Label");
        var lblRT = lbl.GetComponent<RectTransform>();
        lblRT.anchorMin = new Vector2(0, 0);
        lblRT.anchorMax = new Vector2(0.55f, 1);
        lblRT.offsetMin = new Vector2(30, 0);
        lblRT.offsetMax = Vector2.zero;
        var lblTMP = EnsureComponent<TextMeshProUGUI>(lbl);
        lblTMP.text = label;
        lblTMP.fontSize = 40;
        lblTMP.fontStyle = FontStyles.Bold;
        lblTMP.color = WhiteSoft;
        lblTMP.alignment = TextAlignmentOptions.MidlineLeft;

        var toggleHolder = FindOrCreateChild(rowRT, "Toggle");
        var thRT = toggleHolder.GetComponent<RectTransform>();
        thRT.anchorMin = new Vector2(1, 0.5f);
        thRT.anchorMax = new Vector2(1, 0.5f);
        thRT.pivot = new Vector2(1, 0.5f);
        thRT.sizeDelta = new Vector2(130, 70);
        thRT.anchoredPosition = new Vector2(-25, 0);

        var toggle = EnsureComponent<Toggle>(toggleHolder);
        toggle.transition = Selectable.Transition.None;

        var bg = FindOrCreateChild(thRT, "Bg");
        var bgImg = EnsureComponent<Image>(bg);
        bgImg.color = GreyDim;
        StretchFull(bg.GetComponent<RectTransform>());

        var handleBg = FindOrCreateChild(thRT, "HandleBg");
        var hbgImg = EnsureComponent<Image>(handleBg);
        hbgImg.color = CyanNeon;
        StretchFull(handleBg.GetComponent<RectTransform>());
        toggle.graphic = hbgImg;
        toggle.targetGraphic = bgImg;

        var handle = FindOrCreateChild(thRT, "Handle");
        var handleImg = EnsureComponent<Image>(handle);
        handleImg.color = WhiteSoft;
        var handleRT = handle.GetComponent<RectTransform>();
        handleRT.anchorMin = new Vector2(0, 0);
        handleRT.anchorMax = new Vector2(0, 1);
        handleRT.pivot = new Vector2(0, 0.5f);
        handleRT.sizeDelta = new Vector2(70, -10);
        handleRT.anchoredPosition = new Vector2(5, 0);

        return row;
    }

    private static GameObject CreatePillButton(RectTransform parent, string name, string label, Color fill, Color textColor)
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
        col.selectedColor = Color.white;
        col.disabledColor = new Color(1, 1, 1, 0.5f);
        b.colors = col;
        b.transition = Selectable.Transition.ColorTint;

        EnsureComponent<ButtonAnimator>(btn);

        var lbl = FindOrCreateChild(btn.GetComponent<RectTransform>(), "Label");
        var tmp = EnsureComponent<TextMeshProUGUI>(lbl);
        tmp.text = label;
        tmp.fontSize = 56;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.characterSpacing = 8;
        tmp.raycastTarget = false;
        StretchFull(lbl.GetComponent<RectTransform>());

        return btn;
    }

    private static GameObject CreateIconButton(RectTransform parent, string name, string label)
    {
        var btn = FindOrCreateChild(parent, name);
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
        tmp.text = label;
        tmp.fontSize = 36;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = WhiteSoft;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.characterSpacing = 6;
        tmp.raycastTarget = false;
        StretchFull(lbl.GetComponent<RectTransform>());

        return btn;
    }

    private static void BuildGamePlaceholder(Scene scene)
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

        var info = FindOrCreateChild(canvasRT, "PlaceholderText");
        var infoRT = info.GetComponent<RectTransform>();
        infoRT.anchorMin = new Vector2(0.5f, 0.5f);
        infoRT.anchorMax = new Vector2(0.5f, 0.5f);
        infoRT.pivot = new Vector2(0.5f, 0.5f);
        infoRT.sizeDelta = new Vector2(900, 300);
        infoRT.anchoredPosition = Vector2.zero;
        var infoTMP = EnsureComponent<TextMeshProUGUI>(info);
        infoTMP.text = "GAME SCENE\n(placeholder)\n\nGameplay will arrive in Iteration 3.";
        infoTMP.fontSize = 50;
        infoTMP.fontStyle = FontStyles.Bold;
        infoTMP.color = CyanNeon;
        infoTMP.alignment = TextAlignmentOptions.Center;

        var backBtn = CreatePillButton(canvasRT, "BackButton", "BACK", MagentaNeon, BgColor);
        var backRT = backBtn.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0.5f, 0);
        backRT.anchorMax = new Vector2(0.5f, 0);
        backRT.pivot = new Vector2(0.5f, 0);
        backRT.sizeDelta = new Vector2(340, 110);
        backRT.anchoredPosition = new Vector2(0, 120);

        var bridge = FindOrCreateChild(canvasRT, "BackBridge");
        var bridgeBridge = EnsureComponent<GameSceneBackBridge>(bridge);
        bridgeBridge.backButton = backBtn.GetComponent<Button>();
        bridgeBridge.targetSceneName = "MainMenu";
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
