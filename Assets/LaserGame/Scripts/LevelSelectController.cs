using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    [Header("HUD")]
    public CanvasGroup hudGroup;
    public RectTransform hudRect;
    public Button backButton;
    public TextMeshProUGUI coinsText;

    [Header("Content")]
    public ScrollRect scrollRect;
    public RectTransform contentRect;
    public CanvasGroup contentGroup;
    public LevelButton buttonTemplate;

    [Header("Config")]
    public int totalLevels = 30;
    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName = "Game";

    private Vector2 _hudTarget;

    private void OnEnable()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    private void OnDisable()
    {
        if (backButton != null) backButton.onClick.RemoveListener(OnBackClicked);
    }

    private void Start()
    {
        UpdateCoins();
        SpawnLevels();
        AnimateIn();
    }

    private void UpdateCoins()
    {
        if (coinsText != null) coinsText.text = SaveSystem.Data.coins.ToString();
    }

    private void SpawnLevels()
    {
        Debug.Assert(buttonTemplate != null, "LevelSelectController: buttonTemplate is null");
        Debug.Assert(contentRect != null, "LevelSelectController: contentRect is null");

        for (int i = contentRect.childCount - 1; i >= 0; i--)
        {
            var child = contentRect.GetChild(i);
            if (child == buttonTemplate.transform) continue;
            Destroy(child.gameObject);
        }

        buttonTemplate.gameObject.SetActive(false);

        var data = SaveSystem.Data;

        for (int i = 1; i <= totalLevels; i++)
        {
            var go = Instantiate(buttonTemplate.gameObject, contentRect);
            go.name = "LevelButton_" + i;
            go.SetActive(true);

            var lb = go.GetComponent<LevelButton>();
            int stars = data.GetStarsForLevel(i);
            bool unlocked = i <= data.unlockedLevel;
            lb.Setup(i, stars, unlocked);
            lb.OnClicked += (idx, ok) => HandleLevelClicked(idx);
        }
    }

    private void HandleLevelClicked(int levelIndex)
    {
        GameSession.CurrentLevel = levelIndex;
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(gameSceneName);
        else
            SceneManager.LoadScene(gameSceneName);
    }

    private void OnBackClicked()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(mainMenuSceneName);
        else
            SceneManager.LoadScene(mainMenuSceneName);
    }

    private void AnimateIn()
    {
        if (hudGroup != null) hudGroup.alpha = 0f;
        if (hudRect != null)
        {
            _hudTarget = hudRect.anchoredPosition;
            hudRect.anchoredPosition = _hudTarget + new Vector2(0, 60f);
        }
        if (contentGroup != null) contentGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        if (hudGroup != null && hudRect != null)
        {
            seq.Append(hudGroup.DOFade(1f, 0.35f));
            seq.Join(hudRect.DOAnchorPos(_hudTarget, 0.35f).SetEase(Ease.OutCubic));
        }

        if (contentGroup != null)
        {
            seq.Append(contentGroup.DOFade(1f, 0.25f));
        }

        AnimateButtonsStagger();
    }

    private void AnimateButtonsStagger()
    {
        for (int i = 0; i < contentRect.childCount; i++)
        {
            var child = contentRect.GetChild(i);
            if (buttonTemplate != null && child == buttonTemplate.transform) continue;
            var lb = child.GetComponent<LevelButton>();
            if (lb == null) continue;
            float delay = 0.25f + i * 0.025f;
            lb.AnimateAppear(delay);
        }
    }
}
