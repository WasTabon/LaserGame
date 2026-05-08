using System;
using UnityEngine;
using UnityEngine.UI;

public class PausePopup : PopupBase
{
    public Button resumeButton;
    public Button restartButton;
    public Button settingsButton;
    public Button homeButton;

    public Action OnResume;
    public Action OnRestart;
    public Action OnSettings;
    public Action OnHome;

    private void OnEnable()
    {
        Wire(resumeButton, HandleResume);
        Wire(restartButton, HandleRestart);
        Wire(settingsButton, HandleSettings);
        Wire(homeButton, HandleHome);
    }

    private void OnDisable()
    {
        Unwire(resumeButton, HandleResume);
        Unwire(restartButton, HandleRestart);
        Unwire(settingsButton, HandleSettings);
        Unwire(homeButton, HandleHome);
    }

    private void Wire(Button b, UnityEngine.Events.UnityAction action)
    {
        if (b == null) return;
        b.onClick.RemoveListener(action);
        b.onClick.AddListener(action);
    }

    private void Unwire(Button b, UnityEngine.Events.UnityAction action)
    {
        if (b == null) return;
        b.onClick.RemoveListener(action);
    }

    private void HandleResume()
    {
        Close();
        OnResume?.Invoke();
    }

    private void HandleRestart()
    {
        Close();
        OnRestart?.Invoke();
    }

    private void HandleSettings()
    {
        OnSettings?.Invoke();
    }

    private void HandleHome()
    {
        Close();
        OnHome?.Invoke();
    }
}
