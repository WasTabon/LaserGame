using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public Image background;
    public Image border;
    public TextMeshProUGUI numberText;
    public TextMeshProUGUI lockText;
    public TextMeshProUGUI[] starTexts = new TextMeshProUGUI[3];
    public Button button;
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;

    public Color lockedBgColor = new Color(0.10f, 0.10f, 0.16f, 1f);
    public Color unlockedBgColor = new Color(0.105f, 0.105f, 0.18f, 1f);
    public Color completedBgColor = new Color(0.13f, 0.16f, 0.22f, 1f);

    public Color cyanBorder = new Color(0.2f, 0.95f, 1f, 0.55f);
    public Color yellowBorder = new Color(1f, 0.85f, 0.25f, 0.7f);

    public Color numberCyan = new Color(0.2f, 0.95f, 1f, 1f);
    public Color numberWhite = new Color(0.92f, 0.95f, 1f, 1f);

    public Color starFilled = new Color(1f, 0.85f, 0.25f, 1f);
    public Color starEmpty = new Color(0.45f, 0.5f, 0.6f, 0.45f);

    public Color lockColor = new Color(0.45f, 0.5f, 0.6f, 0.7f);

    public event Action<int, bool> OnClicked;

    private int _levelIndex;
    private bool _unlocked;
    private Sequence _pulseSeq;

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }
    }

    private void OnDisable()
    {
        if (button != null) button.onClick.RemoveListener(HandleClick);
        KillPulse();
    }

    public void Setup(int levelIndex, int stars, bool unlocked)
    {
        _levelIndex = levelIndex;
        _unlocked = unlocked;

        if (numberText != null) numberText.text = levelIndex.ToString();

        if (!unlocked)
        {
            ApplyLockedVisual();
        }
        else if (stars <= 0)
        {
            ApplyUnlockedVisual();
        }
        else
        {
            ApplyCompletedVisual(stars);
        }
    }

    private void ApplyLockedVisual()
    {
        if (background != null) background.color = lockedBgColor;
        if (border != null) border.gameObject.SetActive(false);
        if (numberText != null) numberText.gameObject.SetActive(false);
        if (lockText != null)
        {
            lockText.gameObject.SetActive(true);
            lockText.color = lockColor;
        }
        for (int i = 0; i < starTexts.Length; i++)
        {
            if (starTexts[i] != null) starTexts[i].gameObject.SetActive(false);
        }
        KillPulse();
    }

    private void ApplyUnlockedVisual()
    {
        if (background != null) background.color = unlockedBgColor;
        if (border != null)
        {
            border.gameObject.SetActive(true);
            border.color = cyanBorder;
        }
        if (lockText != null) lockText.gameObject.SetActive(false);
        if (numberText != null)
        {
            numberText.gameObject.SetActive(true);
            numberText.color = numberCyan;
        }
        for (int i = 0; i < starTexts.Length; i++)
        {
            if (starTexts[i] != null)
            {
                starTexts[i].gameObject.SetActive(true);
                starTexts[i].color = starEmpty;
            }
        }
        StartPulse();
    }

    private void ApplyCompletedVisual(int stars)
    {
        if (background != null) background.color = completedBgColor;
        if (border != null)
        {
            border.gameObject.SetActive(true);
            border.color = yellowBorder;
        }
        if (lockText != null) lockText.gameObject.SetActive(false);
        if (numberText != null)
        {
            numberText.gameObject.SetActive(true);
            numberText.color = numberWhite;
        }
        for (int i = 0; i < starTexts.Length; i++)
        {
            if (starTexts[i] != null)
            {
                starTexts[i].gameObject.SetActive(true);
                starTexts[i].color = i < stars ? starFilled : starEmpty;
            }
        }
        KillPulse();
    }

    private void StartPulse()
    {
        KillPulse();
        if (border == null) return;
        _pulseSeq = DOTween.Sequence();
        _pulseSeq.Append(border.DOFade(1f, 0.9f).SetEase(Ease.InOutSine));
        _pulseSeq.Append(border.DOFade(0.45f, 0.9f).SetEase(Ease.InOutSine));
        _pulseSeq.SetLoops(-1);
    }

    private void KillPulse()
    {
        if (_pulseSeq != null && _pulseSeq.IsActive())
        {
            _pulseSeq.Kill();
            _pulseSeq = null;
        }
    }

    private void HandleClick()
    {
        if (!_unlocked)
        {
            DoLockedShake();
            HapticManager.Trigger(HapticManager.HapticType.Heavy);
            return;
        }

        if (rectTransform != null)
        {
            rectTransform.DOKill();
            rectTransform.localScale = Vector3.one;
            rectTransform.DOPunchScale(Vector3.one * 0.08f, 0.25f, 6, 0.5f);
        }
        HapticManager.Trigger(HapticManager.HapticType.Light);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        OnClicked?.Invoke(_levelIndex, true);
    }

    private void DoLockedShake()
    {
        if (rectTransform == null) return;
        rectTransform.DOKill();
        rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0);
        rectTransform.DOPunchAnchorPos(new Vector2(15f, 0f), 0.4f, 12, 0.9f);
    }

    public void AnimateAppear(float delay)
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.one * 0.5f;

        Sequence s = DOTween.Sequence();
        s.AppendInterval(delay);
        s.Append(canvasGroup.DOFade(1f, 0.35f));
        s.Join(rectTransform.DOScale(1f, 0.45f).SetEase(Ease.OutBack));
    }
}
