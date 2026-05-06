using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompletePopup : PopupBase
{
    [Header("Title")]
    public TextMeshProUGUI titleText;

    [Header("Stars")]
    public RectTransform[] starRects = new RectTransform[3];
    public CanvasGroup[] starGroups = new CanvasGroup[3];
    public TextMeshProUGUI[] starTexts = new TextMeshProUGUI[3];

    [Header("Coins")]
    public RectTransform coinsBlockRect;
    public CanvasGroup coinsBlockGroup;
    public TextMeshProUGUI coinsRewardText;

    [Header("Buttons")]
    public Button replayButton;
    public Button nextButton;
    public Button menuButton;

    public Color starFilled = new Color(1f, 0.85f, 0.25f, 1f);
    public Color starEmpty = new Color(0.45f, 0.5f, 0.6f, 0.45f);

    public Action OnReplay;
    public Action OnNext;
    public Action OnMenu;

    private void OnEnable()
    {
        if (replayButton != null)
        {
            replayButton.onClick.RemoveListener(HandleReplay);
            replayButton.onClick.AddListener(HandleReplay);
        }
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(HandleNext);
            nextButton.onClick.AddListener(HandleNext);
        }
        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(HandleMenu);
            menuButton.onClick.AddListener(HandleMenu);
        }
    }

    private void OnDisable()
    {
        if (replayButton != null) replayButton.onClick.RemoveListener(HandleReplay);
        if (nextButton != null) nextButton.onClick.RemoveListener(HandleNext);
        if (menuButton != null) menuButton.onClick.RemoveListener(HandleMenu);
    }

    public void Show(int stars, int coinsReward, bool hasNext)
    {
        for (int i = 0; i < starGroups.Length; i++)
        {
            if (starGroups[i] != null) starGroups[i].alpha = 0f;
            if (starRects[i] != null) starRects[i].localScale = Vector3.one * 0.5f;
            if (starTexts[i] != null) starTexts[i].color = starEmpty;
        }
        if (coinsBlockGroup != null) coinsBlockGroup.alpha = 0f;
        if (coinsRewardText != null) coinsRewardText.text = "+ " + coinsReward;

        if (nextButton != null) nextButton.gameObject.SetActive(hasNext);

        Open();
        StartStarSequence(stars);
    }

    private void StartStarSequence(int stars)
    {
        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.AppendInterval(0.2f);

        for (int i = 0; i < starRects.Length; i++)
        {
            int idx = i;
            seq.AppendCallback(() => RevealStar(idx, idx < stars));
            seq.AppendInterval(0.18f);
        }

        seq.Append(coinsBlockGroup.DOFade(1f, 0.3f));
    }

    private void RevealStar(int index, bool filled)
    {
        if (starGroups[index] != null) starGroups[index].DOFade(1f, 0.2f).SetUpdate(true);
        if (starRects[index] != null)
        {
            starRects[index].DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
        }
        if (filled && starTexts[index] != null)
        {
            starTexts[index].DOColor(starFilled, 0.2f).SetUpdate(true);
        }
        HapticManager.Trigger(filled ? HapticManager.HapticType.Light : HapticManager.HapticType.Medium);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
    }

    private void HandleReplay()
    {
        Close();
        OnReplay?.Invoke();
    }

    private void HandleNext()
    {
        Close();
        OnNext?.Invoke();
    }

    private void HandleMenu()
    {
        Close();
        OnMenu?.Invoke();
    }
}
