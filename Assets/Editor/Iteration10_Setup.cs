using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class Iteration10_Setup
{
    private const string GamePath = "Assets/LaserGame/Scenes/Game.unity";

    private static readonly Color CyanNeon = new Color(0.2f, 0.95f, 1f, 1f);
    private static readonly Color YellowNeon = new Color(1f, 0.85f, 0.25f, 1f);

    [MenuItem("LaserGame/Iteration 10/Update Game Scene")]
    public static void UpdateGameScene()
    {
        if (!File.Exists(GamePath)) { Debug.LogWarning("Game scene not found."); return; }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);
        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl == null) { Debug.LogWarning("GameController not found."); return; }

        var canvas = ctrl.fieldRoot.GetComponentInParent<Canvas>();
        if (canvas == null) { Debug.LogWarning("Canvas not found."); return; }
        var canvasRT = canvas.GetComponent<RectTransform>();

        var hint = BuildTutorialHint(canvasRT);
        ctrl.tutorialHint = hint.GetComponent<TutorialHint>();

        ReorderHint(canvasRT);

        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 10] TutorialHint added to Game scene. Confetti + canvas shake + audio cues active in code.");
    }

    [MenuItem("LaserGame/Iteration 10/Reset Tutorial Flag")]
    public static void ResetTutorialFlag()
    {
        PlayerPrefs.DeleteKey("tutorial_shown_v1");
        PlayerPrefs.Save();
        Debug.Log("[Iteration 10] Tutorial flag reset. Will show again on Level 1.");
    }

    private static GameObject BuildTutorialHint(RectTransform parent)
    {
        var hint = FindOrCreateChild(parent, "TutorialHint");
        var rt = hint.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(180, 180);

        var cg = EnsureComponent<CanvasGroup>(hint);
        cg.blocksRaycasts = false;
        cg.interactable = false;

        var pointer = FindOrCreateChild(rt, "Pointer");
        var pointerRT = pointer.GetComponent<RectTransform>();
        StretchFull(pointerRT);
        var pointerTMP = EnsureComponent<TextMeshProUGUI>(pointer);
        pointerTMP.text = "\u261F";
        pointerTMP.fontSize = 130;
        pointerTMP.fontStyle = FontStyles.Bold;
        pointerTMP.color = YellowNeon;
        pointerTMP.alignment = TextAlignmentOptions.Center;
        pointerTMP.raycastTarget = false;

        var th = EnsureComponent<TutorialHint>(hint);
        th.rectTransform = rt;
        th.canvasGroup = cg;
        th.pointerText = pointerTMP;

        hint.SetActive(false);
        return hint;
    }

    private static void ReorderHint(RectTransform canvasRT)
    {
        var hint = canvasRT.Find("TutorialHint");
        var levelComplete = canvasRT.Find("LevelCompletePopup");
        var pause = canvasRT.Find("PausePopup");
        var settings = canvasRT.Find("GameSettingsPopup");
        if (hint != null) hint.SetAsLastSibling();
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
