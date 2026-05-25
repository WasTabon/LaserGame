using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class Iteration15_Setup
{
    private const string GamePath = "Assets/LaserGame/Scenes/Game.unity";

    [MenuItem("LaserGame/Iteration 15/Setup Emitter Auto-Center")]
    public static void Setup()
    {
        if (!File.Exists(GamePath)) { Debug.LogWarning("Game scene not found."); return; }
        var scene = EditorSceneManager.OpenScene(GamePath, OpenSceneMode.Single);

        var emitter = Object.FindObjectOfType<LaserEmitter>();
        var pan = Object.FindObjectOfType<FieldPanController>();
        if (emitter == null) { Debug.LogWarning("[Iteration 15] LaserEmitter not found in scene."); return; }
        if (pan == null) { Debug.LogWarning("[Iteration 15] FieldPanController not found. Run Iteration 14 setup first."); return; }

        var autoCenter = emitter.gameObject.GetComponent<EmitterAutoCenter>();
        if (autoCenter == null) autoCenter = emitter.gameObject.AddComponent<EmitterAutoCenter>();
        autoCenter.emitter = emitter;
        autoCenter.panController = pan;
        EditorUtility.SetDirty(autoCenter);

        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl != null)
        {
            var resetField = ctrl.GetType().GetField("resetButton");
            if (resetField != null)
            {
                var btn = resetField.GetValue(ctrl) as Button;
                if (btn != null)
                {
                    int count = btn.onClick.GetPersistentEventCount();
                    bool alreadyHooked = false;
                    for (int i = 0; i < count; i++)
                    {
                        var tgt = btn.onClick.GetPersistentTarget(i);
                        var mth = btn.onClick.GetPersistentMethodName(i);
                        if (tgt == autoCenter && mth == "Recenter") { alreadyHooked = true; break; }
                    }
                    if (!alreadyHooked)
                    {
                        UnityEventTools.AddPersistentListener(btn.onClick, autoCenter.Recenter);
                        Debug.Log("[Iteration 15] Hooked EmitterAutoCenter.Recenter to ResetButton.onClick");
                    }
                    EditorUtility.SetDirty(btn);
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Iteration 15] Done. Emitter arrow auto-rotates to laser direction + pan auto-centers on emitter at level start and reset.");
    }
}
