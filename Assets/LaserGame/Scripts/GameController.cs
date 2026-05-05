using System.Collections.Generic;
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
    public RectTransform elementsHolder;
    public MirrorElement mirrorTemplate;

    [Header("Bottom")]
    public Button resetButton;
    public RectTransform resetRect;
    public CanvasGroup resetGroup;

    [Header("Test Level")]
    public LevelDefinition testLevel = new LevelDefinition();

    [Header("Scenes")]
    public string levelSelectSceneName = "LevelSelect";

    private int _moves;
    private Vector2 _topHudTarget;
    private Vector2 _subHudTarget;
    private Vector2 _resetTarget;
    private List<MirrorElement> _activeMirrors = new List<MirrorElement>();

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

        for (int i = 0; i < _activeMirrors.Count; i++)
        {
            if (_activeMirrors[i] != null)
                _activeMirrors[i].OnRotated -= HandleMirrorRotated;
        }
    }

    private void Start()
    {
        UpdateCoins();
        UpdateLevelText();
        ApplyLevelDefinition(testLevel);
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

    public void ApplyLevelDefinition(LevelDefinition def)
    {
        Debug.Assert(grid != null, "GameController: grid is null");
        Debug.Assert(emitter != null, "GameController: emitter is null");
        Debug.Assert(def != null, "GameController: level definition is null");

        grid.Build(def.cols, def.rows);
        emitter.cell = def.emitterCell;
        emitter.direction = def.emitterDir;
        emitter.PlaceOnGrid(grid);

        ClearMirrors();
        SpawnMirrors(def.mirrors);
    }

    private void ClearMirrors()
    {
        for (int i = 0; i < _activeMirrors.Count; i++)
        {
            var m = _activeMirrors[i];
            if (m == null) continue;
            m.OnRotated -= HandleMirrorRotated;
            if (Application.isPlaying) Destroy(m.gameObject);
            else DestroyImmediate(m.gameObject);
        }
        _activeMirrors.Clear();

        if (elementsHolder != null)
        {
            for (int i = elementsHolder.childCount - 1; i >= 0; i--)
            {
                var child = elementsHolder.GetChild(i);
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }
        }
    }

    private void SpawnMirrors(List<MirrorPlacement> placements)
    {
        if (placements == null || placements.Count == 0) return;
        if (mirrorTemplate == null) { Debug.LogWarning("GameController: mirrorTemplate is null"); return; }
        if (elementsHolder == null) { Debug.LogWarning("GameController: elementsHolder is null"); return; }

        for (int i = 0; i < placements.Count; i++)
        {
            var p = placements[i];
            var go = Instantiate(mirrorTemplate.gameObject, elementsHolder);
            go.name = "Mirror_" + p.cell.x + "_" + p.cell.y;
            go.SetActive(true);
            var m = go.GetComponent<MirrorElement>();
            m.cell = p.cell;
            m.rotationStep = p.initialRotationStep;
            m.PlaceOnGrid(grid);
            m.OnRotated += HandleMirrorRotated;
            _activeMirrors.Add(m);
        }
    }

    public void RecalculateRay()
    {
        if (rayRenderer == null || grid == null || emitter == null) return;
        var segments = RayCalculator.Calculate(grid, emitter, _activeMirrors);
        rayRenderer.Render(segments, grid.CellSize);
    }

    private void HandleMirrorRotated(MirrorElement m)
    {
        UpdateMoves(_moves + 1);
        RecalculateRay();
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
        for (int i = 0; i < _activeMirrors.Count && i < testLevel.mirrors.Count; i++)
        {
            if (_activeMirrors[i] != null)
            {
                _activeMirrors[i].ResetRotation(testLevel.mirrors[i].initialRotationStep);
            }
        }

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
