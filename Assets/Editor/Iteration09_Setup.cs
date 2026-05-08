using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class Iteration09_Setup
{
    private const string GamePath = "Assets/LaserGame/Scenes/Game.unity";

    private static readonly Color BgColor = new Color(0.058f, 0.058f, 0.117f, 1f);
    private static readonly Color BgColorAccent = new Color(0.078f, 0.078f, 0.156f, 1f);
    private static readonly Color CyanNeon = new Color(0.2f, 0.95f, 1f, 1f);
    private static readonly Color MagentaNeon = new Color(1f, 0.25f, 0.85f, 1f);
    private static readonly Color WhiteSoft = new Color(0.92f, 0.95f, 1f, 1f);
    private static readonly Color GreyDim = new Color(0.45f, 0.5f, 0.6f, 1f);
    private static readonly Color PanelColor = new Color(0.105f, 0.105f, 0.18f, 0.98f);

    [MenuItem("LaserGame/Iteration 09/Update Game Scene")]
    public static void UpdateGameScene()
    {
        if (!File.Exists(GamePath)) { Debug.LogWarning("Game scene not found."); return; }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null) { Debug.LogWarning("GameController not found."); return; }

        var canvas = ctrl.fieldRoot.GetComponentInParent<Canvas>();
        if (canvas == null) { Debug.LogWarning("Canvas not found."); return; }
        var canvasRT = canvas.GetComponent<RectTransform>();

        var topHud = canvasRT.Find("TopHud") as RectTransform;
        if (topHud == null) { Debug.LogWarning("TopHud not found."); return; }

        var pauseBtn = BuildPauseButton(topHud);
        ctrl.pauseButton = pauseBtn.GetComponent<Button>();

        var pausePopup = BuildPausePopup(canvasRT);
        ctrl.pausePopup = pausePopup.GetComponent<PausePopup>();

        var settingsPopup = BuildGameSettingsPopup(canvasRT);
        ctrl.gameSettingsPopup = settingsPopup.GetComponent<SettingsPopup>();

        ReorderSiblings(canvasRT);

        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 09] Pause button + PausePopup + in-game SettingsPopup added.");
    }

    private static GameObject BuildPauseButton(RectTransform topHud)
    {
        var coinsHud = topHud.Find("CoinsHud") as RectTransform;
        if (coinsHud != null)
        {
            coinsHud.anchoredPosition = new Vector2(-150, -10);
        }

        var btn = FindOrCreateChild(topHud, "PauseButton");
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
        b.targetGraphic = img;
        EnsureComponent<ButtonAnimator>(btn);

        var rt = btn.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(1, 0.5f);
        rt.sizeDelta = new Vector2(100, 100);
        rt.anchoredPosition = new Vector2(-30, -10);

        var lbl = FindOrCreateChild(rt, "Label");
        var tmp = EnsureComponent<TextMeshProUGUI>(lbl);
        tmp.text = "II";
        tmp.fontSize = 56;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = CyanNeon;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.characterSpacing = 4;
        tmp.raycastTarget = false;
        StretchFull(lbl.GetComponent<RectTransform>());

        return btn;
    }

    private static GameObject BuildPausePopup(RectTransform parent)
    {
        var popup = FindOrCreateChild(parent, "PausePopup");
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

        var title = FindOrCreateChild(contentRT, "Title");
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.sizeDelta = new Vector2(0, 110);
        titleRT.anchoredPosition = new Vector2(0, -30);
        var titleTMP = EnsureComponent<TextMeshProUGUI>(title);
        titleTMP.text = "PAUSED";
        titleTMP.fontSize = 64;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = CyanNeon;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.characterSpacing = 8;
        titleTMP.raycastTarget = false;

        var resume = CreatePillButton(contentRT, "ResumeButton", "RESUME", CyanNeon, BgColor, 56);
        var resumeRT = resume.GetComponent<RectTransform>();
        resumeRT.anchorMin = new Vector2(0.5f, 1);
        resumeRT.anchorMax = new Vector2(0.5f, 1);
        resumeRT.pivot = new Vector2(0.5f, 0.5f);
        resumeRT.sizeDelta = new Vector2(520, 130);
        resumeRT.anchoredPosition = new Vector2(0, -250);

        var restart = CreatePillButton(contentRT, "RestartButton", "RESTART", PanelColor, MagentaNeon, 40);
        var restartRT = restart.GetComponent<RectTransform>();
        restartRT.anchorMin = new Vector2(0.5f, 1);
        restartRT.anchorMax = new Vector2(0.5f, 1);
        restartRT.pivot = new Vector2(0.5f, 0.5f);
        restartRT.sizeDelta = new Vector2(520, 110);
        restartRT.anchoredPosition = new Vector2(0, -410);

        var settings = CreatePillButton(contentRT, "SettingsButton", "SETTINGS", PanelColor, MagentaNeon, 40);
        var settingsRT = settings.GetComponent<RectTransform>();
        settingsRT.anchorMin = new Vector2(0.5f, 1);
        settingsRT.anchorMax = new Vector2(0.5f, 1);
        settingsRT.pivot = new Vector2(0.5f, 0.5f);
        settingsRT.sizeDelta = new Vector2(520, 110);
        settingsRT.anchoredPosition = new Vector2(0, -545);

        var home = CreatePillButton(contentRT, "HomeButton", "HOME", PanelColor, MagentaNeon, 40);
        var homeRT = home.GetComponent<RectTransform>();
        homeRT.anchorMin = new Vector2(0.5f, 1);
        homeRT.anchorMax = new Vector2(0.5f, 1);
        homeRT.pivot = new Vector2(0.5f, 0.5f);
        homeRT.sizeDelta = new Vector2(520, 110);
        homeRT.anchoredPosition = new Vector2(0, -680);

        var pp = EnsureComponent<PausePopup>(popup);
        pp.canvasGroup = popupGroup;
        pp.contentRect = contentRT;
        pp.backdrop = backdropImg;
        pp.resumeButton = resume.GetComponent<Button>();
        pp.restartButton = restart.GetComponent<Button>();
        pp.settingsButton = settings.GetComponent<Button>();
        pp.homeButton = home.GetComponent<Button>();

        popup.SetActive(false);
        return popup;
    }

    private static GameObject BuildGameSettingsPopup(RectTransform parent)
    {
        var popup = FindOrCreateChild(parent, "GameSettingsPopup");
        var popupRT = popup.GetComponent<RectTransform>();
        StretchFull(popupRT);
        var popupGroup = EnsureComponent<CanvasGroup>(popup);
        popupGroup.alpha = 0f;
        popupGroup.blocksRaycasts = false;

        var backdrop = FindOrCreateChild(popupRT, "Backdrop");
        var backdropImg = EnsureComponent<Image>(backdrop);
        backdropImg.color = new Color(0, 0, 0, 0.65f);
        StretchFull(backdrop.GetComponent<RectTransform>());
        var backdropBtn = EnsureComponent<Button>(backdrop);
        var bcol = backdropBtn.colors;
        bcol.normalColor = Color.white; bcol.highlightedColor = Color.white; bcol.pressedColor = Color.white;
        backdropBtn.colors = bcol;
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

        var closeBtn = CreatePillButton(contentRT, "CloseButton", "CLOSE", MagentaNeon, BgColor, 56);
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

    private static void ReorderSiblings(RectTransform canvasRT)
    {
        var pause = canvasRT.Find("PausePopup");
        var settings = canvasRT.Find("GameSettingsPopup");
        var levelComplete = canvasRT.Find("LevelCompletePopup");
        if (pause != null) pause.SetAsLastSibling();
        if (levelComplete != null) levelComplete.SetAsLastSibling();
        if (settings != null) settings.SetAsLastSibling();
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
