using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : PopupBase
{
    [Header("Toggles")]
    public Toggle soundToggle;
    public Toggle musicToggle;
    public Toggle hapticsToggle;

    [Header("Buttons")]
    public Button closeButton;
    public Button backdropButton;

    private void OnEnable()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
        }
        if (backdropButton != null)
        {
            backdropButton.onClick.RemoveListener(Close);
            backdropButton.onClick.AddListener(Close);
        }

        if (soundToggle != null)
        {
            soundToggle.onValueChanged.RemoveListener(OnSoundChanged);
            soundToggle.onValueChanged.AddListener(OnSoundChanged);
        }
        if (musicToggle != null)
        {
            musicToggle.onValueChanged.RemoveListener(OnMusicChanged);
            musicToggle.onValueChanged.AddListener(OnMusicChanged);
        }
        if (hapticsToggle != null)
        {
            hapticsToggle.onValueChanged.RemoveListener(OnHapticsChanged);
            hapticsToggle.onValueChanged.AddListener(OnHapticsChanged);
        }

        SyncFromData();
    }

    private void OnDisable()
    {
        if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        if (backdropButton != null) backdropButton.onClick.RemoveListener(Close);
        if (soundToggle != null) soundToggle.onValueChanged.RemoveListener(OnSoundChanged);
        if (musicToggle != null) musicToggle.onValueChanged.RemoveListener(OnMusicChanged);
        if (hapticsToggle != null) hapticsToggle.onValueChanged.RemoveListener(OnHapticsChanged);
    }

    private void SyncFromData()
    {
        var d = SaveSystem.Data;
        if (soundToggle != null) soundToggle.SetIsOnWithoutNotify(d.soundEnabled);
        if (musicToggle != null) musicToggle.SetIsOnWithoutNotify(d.musicEnabled);
        if (hapticsToggle != null) hapticsToggle.SetIsOnWithoutNotify(d.hapticsEnabled);
    }

    private void OnSoundChanged(bool v)
    {
        SaveSystem.Data.soundEnabled = v;
        SaveSystem.Save();
        if (AudioManager.Instance != null) AudioManager.Instance.ApplySettings();
    }

    private void OnMusicChanged(bool v)
    {
        SaveSystem.Data.musicEnabled = v;
        SaveSystem.Save();
        if (AudioManager.Instance != null) AudioManager.Instance.ApplySettings();
    }

    private void OnHapticsChanged(bool v)
    {
        SaveSystem.Data.hapticsEnabled = v;
        SaveSystem.Save();
    }

    public override void Open()
    {
        SyncFromData();
        base.Open();
    }
}
