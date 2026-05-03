using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform titleRect;
    public CanvasGroup titleGroup;
    public RectTransform playButtonRect;
    public CanvasGroup playButtonGroup;
    public RectTransform settingsButtonRect;
    public CanvasGroup settingsButtonGroup;
    public RectTransform coinsHudRect;
    public CanvasGroup coinsHudGroup;
    public TextMeshProUGUI coinsText;

    public Button playButton;
    public Button settingsButton;

    public SettingsPopup settingsPopup;

    public string nextSceneName = "LevelSelect";

    private Vector2 _titleTarget;
    private Vector2 _playTarget;
    private Vector2 _settingsTarget;
    private Vector2 _coinsTarget;

    private void OnEnable()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
            playButton.onClick.AddListener(OnPlayClicked);
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OnSettingsClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }
    }

    private void OnDisable()
    {
        if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClicked);
    }

    private void Start()
    {
        UpdateCoins();
        CacheTargets();
        AnimateIn();
    }

    private void UpdateCoins()
    {
        if (coinsText != null) coinsText.text = SaveSystem.Data.coins.ToString();
    }

    private void CacheTargets()
    {
        if (titleRect != null) _titleTarget = titleRect.anchoredPosition;
        if (playButtonRect != null) _playTarget = playButtonRect.anchoredPosition;
        if (settingsButtonRect != null) _settingsTarget = settingsButtonRect.anchoredPosition;
        if (coinsHudRect != null) _coinsTarget = coinsHudRect.anchoredPosition;
    }

    private void AnimateIn()
    {
        if (titleRect != null) titleRect.anchoredPosition = _titleTarget + new Vector2(0f, 60f);
        if (coinsHudRect != null) coinsHudRect.anchoredPosition = _coinsTarget + new Vector2(0f, 40f);
        if (playButtonRect != null) playButtonRect.anchoredPosition = _playTarget + new Vector2(0f, -80f);
        if (settingsButtonRect != null) settingsButtonRect.anchoredPosition = _settingsTarget + new Vector2(0f, -80f);

        if (titleGroup != null) titleGroup.alpha = 0f;
        if (coinsHudGroup != null) coinsHudGroup.alpha = 0f;
        if (playButtonGroup != null) playButtonGroup.alpha = 0f;
        if (settingsButtonGroup != null) settingsButtonGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        if (coinsHudRect != null && coinsHudGroup != null)
        {
            seq.Append(coinsHudGroup.DOFade(1f, 0.35f));
            seq.Join(coinsHudRect.DOAnchorPos(_coinsTarget, 0.35f).SetEase(Ease.OutCubic));
        }

        if (titleRect != null && titleGroup != null)
        {
            seq.Append(titleGroup.DOFade(1f, 0.5f));
            seq.Join(titleRect.DOAnchorPos(_titleTarget, 0.5f).SetEase(Ease.OutBack));
        }

        if (playButtonRect != null && playButtonGroup != null)
        {
            seq.Append(playButtonGroup.DOFade(1f, 0.4f));
            seq.Join(playButtonRect.DOAnchorPos(_playTarget, 0.4f).SetEase(Ease.OutCubic));
        }

        if (settingsButtonRect != null && settingsButtonGroup != null)
        {
            seq.Append(settingsButtonGroup.DOFade(1f, 0.4f).SetDelay(-0.3f));
            seq.Join(settingsButtonRect.DOAnchorPos(_settingsTarget, 0.4f).SetEase(Ease.OutCubic));
        }
    }

    private void OnPlayClicked()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(nextSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnSettingsClicked()
    {
        if (settingsPopup != null) settingsPopup.Open();
    }
}
