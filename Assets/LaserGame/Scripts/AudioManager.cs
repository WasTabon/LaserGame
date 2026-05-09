using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip buttonClickClip;
    public AudioClip popupOpenClip;
    public AudioClip popupCloseClip;
    public AudioClip menuMusicClip;
    public AudioClip gameMusicClip;
    public AudioClip mirrorRotateClip;
    public AudioClip batteryChargeClip;
    public AudioClip energyStarClip;
    public AudioClip winClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.5f;
            musicSource.playOnAwake = false;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = 0.8f;
            sfxSource.playOnAwake = false;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ApplySettings();
        PlayMenuMusic();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        var data = SaveSystem.Data;
        musicSource.mute = !data.musicEnabled;
        sfxSource.mute = !data.soundEnabled;
    }

    public void PlayMenuMusic()
    {
        if (menuMusicClip == null) return;
        if (musicSource.clip == menuMusicClip && musicSource.isPlaying) return;
        musicSource.clip = menuMusicClip;
        musicSource.Play();
    }

    public void PlayGameMusic()
    {
        if (gameMusicClip == null) return;
        if (musicSource.clip == gameMusicClip && musicSource.isPlaying) return;
        musicSource.clip = gameMusicClip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickClip);
    }

    public void PlayPopupOpen()
    {
        PlaySFX(popupOpenClip);
    }

    public void PlayPopupClose()
    {
        PlaySFX(popupCloseClip);
    }

    public void PlayMirrorRotate()
    {
        PlaySFX(mirrorRotateClip);
    }

    public void PlayBatteryCharge()
    {
        PlaySFX(batteryChargeClip);
    }

    public void PlayEnergyStarCollect()
    {
        PlaySFX(energyStarClip);
    }

    public void PlayWin()
    {
        PlaySFX(winClip);
    }
}
