using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("HUD")]
    public CanvasGroup topHudGroup;
    public RectTransform topHudRect;
    public Button backButton;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI movesText;
    public CanvasGroup subHudGroup;
    public RectTransform subHudRect;

    [Header("Field")]
    public RectTransform fieldRoot;
    public CanvasGroup fieldGroup;
    public GridSystem grid;
    public LaserEmitter emitter;
    public RayRenderer rayRenderer;

    [Header("Bottom")]
    public Button resetButton;
    public RectTransform resetRect;
    public CanvasGroup resetGroup;

    [Header("Default Level Config")]
    public int defaultRows = 5;
    public int defaultCols = 5;
    public Vector2Int defaultEmitterCell = new Vector2Int(0, 2);
    public Vector2Int defaultEmitterDir = new Vector2Int(1, 0);

    [Header("Scenes")]
    public string levelSelectSceneName = "LevelSelect";

    private int _moves;
    private Vector2 _topHudTarget;
    private Vector2 _subHudTarget;
    private Vector2 _resetTarget;

    private void OnEnable()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
            backButton.onClick.AddListener(OnBackClicked);
        }
        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(OnResetClicked);
            resetButton.onClick.AddListener(OnResetClicked);
        }
    }

    private void OnDisable()
    {
        if (backButton != null) backButton.onClick.RemoveListener(OnBackClicked);
        if (resetButton != null) resetButton.onClick.RemoveListener(OnResetClicked);
    }

    private void Start()
    {
        UpdateCoins();
        UpdateLevelText();
        BuildLevel();
        UpdateMoves(0);
        AnimateIn();
        RecalculateRay();
        if (rayRenderer != null) rayRenderer.RevealAnimation();
        if (emitter != null) emitter.PulseAppear();
    }

    private void UpdateCoins()
    {
        if (coinsText != null) coinsText.text = SaveSystem.Data.coins.ToString();
    }

    private void UpdateLevelText()
    {
        if (levelText != null) levelText.text = "LEVEL " + GameSession.CurrentLevel;
    }

    private void UpdateMoves(int value)
    {
        _moves = value;
        if (movesText != null) movesText.text = "MOVES: " + _moves;
    }

    private void BuildLevel()
    {
        Debug.Assert(grid != null, "GameController: grid is null");
        Debug.Assert(emitter != null, "GameController: emitter is null");

        grid.Build(defaultCols, defaultRows);

        emitter.cell = defaultEmitterCell;
        emitter.direction = defaultEmitterDir;
        emitter.PlaceOnGrid(grid);
    }

    public void RecalculateRay()
    {
        if (rayRenderer == null || grid == null || emitter == null) return;
        var segments = RayCalculator.Calculate(grid, emitter);
        rayRenderer.Render(segments, grid.CellSize);
    }

    private void OnBackClicked()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(levelSelectSceneName);
        else
            SceneManager.LoadScene(levelSelectSceneName);
    }

    private void OnResetClicked()
    {
        emitter.cell = defaultEmitterCell;
        emitter.direction = defaultEmitterDir;
        emitter.PlaceOnGrid(grid);

        UpdateMoves(0);
        RecalculateRay();
        if (rayRenderer != null) rayRenderer.RevealAnimation();

        if (resetRect != null)
        {
            resetRect.DOKill();
            resetRect.localScale = Vector3.one;
            resetRect.DOPunchScale(Vector3.one * 0.08f, 0.25f, 6, 0.5f);
        }
        HapticManager.Trigger(HapticManager.HapticType.Medium);
    }

    private void AnimateIn()
    {
        if (topHudRect != null)
        {
            _topHudTarget = topHudRect.anchoredPosition;
            topHudRect.anchoredPosition = _topHudTarget + new Vector2(0, 60);
        }
        if (subHudRect != null)
        {
            _subHudTarget = subHudRect.anchoredPosition;
            subHudRect.anchoredPosition = _subHudTarget + new Vector2(0, 30);
        }
        if (resetRect != null)
        {
            _resetTarget = resetRect.anchoredPosition;
            resetRect.anchoredPosition = _resetTarget + new Vector2(0, -90);
        }

        if (topHudGroup != null) topHudGroup.alpha = 0f;
        if (subHudGroup != null) subHudGroup.alpha = 0f;
        if (fieldGroup != null) fieldGroup.alpha = 0f;
        if (resetGroup != null) resetGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        if (topHudGroup != null && topHudRect != null)
        {
            seq.Append(topHudGroup.DOFade(1f, 0.3f));
            seq.Join(topHudRect.DOAnchorPos(_topHudTarget, 0.3f).SetEase(Ease.OutCubic));
        }
        if (subHudGroup != null && subHudRect != null)
        {
            seq.Append(subHudGroup.DOFade(1f, 0.25f));
            seq.Join(subHudRect.DOAnchorPos(_subHudTarget, 0.25f).SetEase(Ease.OutCubic));
        }
        if (fieldGroup != null)
        {
            seq.Append(fieldGroup.DOFade(1f, 0.4f));
        }
        if (resetGroup != null && resetRect != null)
        {
            seq.Append(resetGroup.DOFade(1f, 0.3f).SetDelay(-0.2f));
            seq.Join(resetRect.DOAnchorPos(_resetTarget, 0.3f).SetEase(Ease.OutCubic));
        }
    }
}
