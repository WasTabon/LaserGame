using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class Iteration12_Setup
{
    private const string GamePath = "Assets/LaserGame/Scenes/Game.unity";

    private static readonly Color BgColor = new Color(0.058f, 0.058f, 0.117f, 1f);
    private static readonly Color BgAccent = new Color(0.078f, 0.078f, 0.156f, 1f);
    private static readonly Color PanelColor = new Color(0.105f, 0.105f, 0.18f, 0.98f);
    private static readonly Color CyanNeon = new Color(0.2f, 0.95f, 1f, 1f);
    private static readonly Color MagentaNeon = new Color(1f, 0.25f, 0.85f, 1f);
    private static readonly Color YellowNeon = new Color(1f, 0.85f, 0.25f, 1f);
    private static readonly Color GreenSoft = new Color(0.5f, 1f, 0.5f, 1f);
    private static readonly Color WhiteSoft = new Color(0.92f, 0.95f, 1f, 1f);

    [MenuItem("LaserGame/Iteration 12/Update Game Scene")]
    public static void UpdateGameScene()
    {
        if (!File.Exists(GamePath)) { Debug.LogWarning("Game scene not found."); return; }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null) { Debug.LogWarning("GameController not found."); return; }
        var canvas = ctrl.fieldRoot.GetComponentInParent<Canvas>();
        if (canvas == null) { Debug.LogWarning("Canvas not found."); return; }
        var canvasRT = canvas.GetComponent<RectTransform>();

        var boostBar = BuildBoostBar(canvasRT);
        ctrl.boostBarRect = boostBar.GetComponent<RectTransform>();
        ctrl.boostBarGroup = boostBar.GetComponent<CanvasGroup>();

        var hint = boostBar.transform.Find("HintBoost").gameObject;
        var undo = boostBar.transform.Find("UndoBoost").gameObject;
        var skip = boostBar.transform.Find("SkipBoost").gameObject;

        ctrl.hintBoostButton = hint.GetComponent<Button>();
        ctrl.hintBoostCountText = hint.transform.Find("CountBadge/CountText").GetComponent<TextMeshProUGUI>();
        ctrl.undoBoostButton = undo.GetComponent<Button>();
        ctrl.undoBoostCountText = undo.transform.Find("CountBadge/CountText").GetComponent<TextMeshProUGUI>();
        ctrl.skipBoostButton = skip.GetComponent<Button>();
        ctrl.skipBoostCountText = skip.transform.Find("CountBadge/CountText").GetComponent<TextMeshProUGUI>();

        ReorderSiblings(canvasRT);

        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 12] Boost bar added with 3 boost buttons (Hint/Undo/Skip).");
    }

    [MenuItem("LaserGame/Iteration 12/Grant 5 Of Each Boost (Test)")]
    public static void GrantBoosts()
    {
        SaveSystem.Load();
        SaveSystem.Data.hintCount += 5;
        SaveSystem.Data.undoCount += 5;
        SaveSystem.Data.skipCount += 5;
        SaveSystem.Save();
        Debug.Log("[Iteration 12] +5 of each boost. Hint=" + SaveSystem.Data.hintCount + " Undo=" + SaveSystem.Data.undoCount + " Skip=" + SaveSystem.Data.skipCount);
    }

    private static GameObject BuildBoostBar(RectTransform parent)
    {
        var bar = FindOrCreateChild(parent, "BoostBar");
        var barRT = bar.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0.5f, 0);
        barRT.anchorMax = new Vector2(0.5f, 0);
        barRT.pivot = new Vector2(0.5f, 0);
        barRT.sizeDelta = new Vector2(420, 130);
        barRT.anchoredPosition = new Vector2(0, 220);
        EnsureComponent<CanvasGroup>(bar);

        BuildBoostButton(barRT, "HintBoost", "?", CyanNeon, new Vector2(-140, 0));
        BuildBoostButton(barRT, "UndoBoost", "U", MagentaNeon, new Vector2(0, 0));
        BuildBoostButton(barRT, "SkipBoost", ">", GreenSoft, new Vector2(140, 0));

        return bar;
    }

    private static GameObject BuildBoostButton(RectTransform parent, string name, string letter, Color iconColor, Vector2 anchoredPos)
    {
        var btn = FindOrCreateChild(parent, name);
        var img = EnsureComponent<Image>(btn);
        img.color = PanelColor;
        EnsureComponent<CanvasGroup>(btn);
        var b = EnsureComponent<Button>(btn);
        b.targetGraphic = img;
        b.transition = Selectable.Transition.ColorTint;
        EnsureComponent<ButtonAnimator>(btn);

        var rt = btn.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(110, 110);
        rt.anchoredPosition = anchoredPos;

        var border = FindOrCreateChild(rt, "Border");
        var borderImg = EnsureComponent<Image>(border);
        borderImg.color = new Color(iconColor.r, iconColor.g, iconColor.b, 0.5f);
        borderImg.raycastTarget = false;
        var borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = new Vector2(-3, -3);
        borderRT.offsetMax = new Vector2(3, 3);
        border.transform.SetAsFirstSibling();

        var letterGo = FindOrCreateChild(rt, "Letter");
        var letterRT = letterGo.GetComponent<RectTransform>();
        StretchFull(letterRT);
        var letterTMP = EnsureComponent<TextMeshProUGUI>(letterGo);
        letterTMP.text = letter;
        letterTMP.fontSize = 60;
        letterTMP.fontStyle = FontStyles.Bold;
        letterTMP.color = iconColor;
        letterTMP.alignment = TextAlignmentOptions.Center;
        letterTMP.raycastTarget = false;

        var badge = FindOrCreateChild(rt, "CountBadge");
        var badgeRT = badge.GetComponent<RectTransform>();
        badgeRT.anchorMin = new Vector2(1, 1);
        badgeRT.anchorMax = new Vector2(1, 1);
        badgeRT.pivot = new Vector2(1, 1);
        badgeRT.sizeDelta = new Vector2(50, 40);
        badgeRT.anchoredPosition = new Vector2(8, 8);
        var badgeImg = EnsureComponent<Image>(badge);
        badgeImg.color = YellowNeon;
        badgeImg.raycastTarget = false;

        var countTxt = FindOrCreateChild(badgeRT, "CountText");
        var countRT = countTxt.GetComponent<RectTransform>();
        StretchFull(countRT);
        var countTMP = EnsureComponent<TextMeshProUGUI>(countTxt);
        countTMP.text = "x0";
        countTMP.fontSize = 22;
        countTMP.fontStyle = FontStyles.Bold;
        countTMP.color = BgColor;
        countTMP.alignment = TextAlignmentOptions.Center;
        countTMP.raycastTarget = false;

        return btn;
    }

    private static void ReorderSiblings(RectTransform canvasRT)
    {
        var resetBtn = canvasRT.Find("ResetButton");
        var boostBar = canvasRT.Find("BoostBar");
        var coinFly = canvasRT.Find("CoinFlyHost");
        var winFlash = canvasRT.Find("WinFlashOverlay");
        var hint = canvasRT.Find("TutorialHint");
        var levelComplete = canvasRT.Find("LevelCompletePopup");
        var pause = canvasRT.Find("PausePopup");
        var settings = canvasRT.Find("GameSettingsPopup");

        if (resetBtn != null && boostBar != null)
        {
            int resetIdx = resetBtn.GetSiblingIndex();
            boostBar.SetSiblingIndex(resetIdx + 1);
        }
        if (coinFly != null) coinFly.SetAsLastSibling();
        if (hint != null) hint.SetAsLastSibling();
        if (winFlash != null) winFlash.SetAsLastSibling();
        if (levelComplete != null) levelComplete.SetAsLastSibling();
        if (pause != null) pause.SetAsLastSibling();
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
