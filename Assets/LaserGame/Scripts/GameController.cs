using System.Collections;
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
    public BatteryElement batteryTemplate;

    [Header("Bottom")]
    public Button resetButton;
    public RectTransform resetRect;
    public CanvasGroup resetGroup;

    [Header("Win FX")]
    public Image winFlashOverlay;
    public LevelCompletePopup levelCompletePopup;

    [Header("Test Level")]
    public LevelDefinition testLevel = new LevelDefinition();

    [Header("Scenes")]
    public string levelSelectSceneName = "LevelSelect";

    public Color winFlashColor = new Color(0.2f, 0.95f, 1f, 1f);

    private int _moves;
    private bool _isWon;
    private Vector2 _topHudTarget;
    private Vector2 _subHudTarget;
    private Vector2 _resetTarget;
    private List<MirrorElement> _activeMirrors = new List<MirrorElement>();
    private List<BatteryElement> _activeBatteries = new List<BatteryElement>();

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
        if (levelCompletePopup != null)
        {
            levelCompletePopup.OnReplay = HandleReplay;
            levelCompletePopup.OnNext = HandleNext;
            levelCompletePopup.OnMenu = HandleMenu;
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

        if (levelCompletePopup != null)
        {
            levelCompletePopup.OnReplay = null;
            levelCompletePopup.OnNext = null;
            levelCompletePopup.OnMenu = null;
        }
    }

    private void Start()
    {
        UpdateCoins();
        UpdateLevelText();
        ApplyLevelDefinition(testLevel);
        UpdateMoves(0);
        _isWon = false;
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

        if (def.mirrors == null) def.mirrors = new List<MirrorPlacement>();
        if (def.batteries == null) def.batteries = new List<Vector2Int>();
        if (def.energyStars == null) def.energyStars = new List<Vector2Int>();

        grid.Build(def.cols, def.rows);
        emitter.cell = def.emitterCell;
        emitter.direction = def.emitterDir;
        emitter.PlaceOnGrid(grid);

        ClearMirrors();
        ClearBatteries();
        SpawnMirrors(def.mirrors);
        SpawnBatteries(def.batteries);
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
    }

    private void ClearBatteries()
    {
        for (int i = 0; i < _activeBatteries.Count; i++)
        {
            var b = _activeBatteries[i];
            if (b == null) continue;
            if (Application.isPlaying) Destroy(b.gameObject);
            else DestroyImmediate(b.gameObject);
        }
        _activeBatteries.Clear();

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

    private void SpawnBatteries(List<Vector2Int> placements)
    {
        if (placements == null || placements.Count == 0) return;
        if (batteryTemplate == null) { Debug.LogWarning("GameController: batteryTemplate is null"); return; }
        if (elementsHolder == null) { Debug.LogWarning("GameController: elementsHolder is null"); return; }

        for (int i = 0; i < placements.Count; i++)
        {
            var go = Instantiate(batteryTemplate.gameObject, elementsHolder);
            go.name = "Battery_" + placements[i].x + "_" + placements[i].y;
            go.SetActive(true);
            var b = go.GetComponent<BatteryElement>();
            b.cell = placements[i];
            b.PlaceOnGrid(grid);
            b.SetChargedImmediate(false);
            _activeBatteries.Add(b);
        }
    }

    public void RecalculateRay()
    {
        if (rayRenderer == null || grid == null || emitter == null) return;
        var result = RayCalculator.Calculate(grid, emitter, _activeMirrors);
        rayRenderer.Render(result.segments, grid.CellSize);

        UpdateBatteryStates(result.visitedCells);
        CheckWinCondition();
    }

    private void UpdateBatteryStates(HashSet<Vector2Int> visited)
    {
        for (int i = 0; i < _activeBatteries.Count; i++)
        {
            var b = _activeBatteries[i];
            if (b == null) continue;
            bool shouldBeCharged = visited.Contains(b.cell);
            if (shouldBeCharged && !b.IsCharged) b.Charge();
            else if (!shouldBeCharged && b.IsCharged) b.Discharge();
        }
    }

    private void CheckWinCondition()
    {
        if (_isWon) return;
        if (_activeBatteries.Count == 0) return;
        for (int i = 0; i < _activeBatteries.Count; i++)
        {
            if (_activeBatteries[i] == null || !_activeBatteries[i].IsCharged) return;
        }
        _isWon = true;
        StartCoroutine(WinSequenceRoutine());
    }

    private IEnumerator WinSequenceRoutine()
    {
        HapticManager.Trigger(HapticManager.HapticType.Success);

        if (grid != null) grid.PlayWinPulse(winFlashColor);

        if (winFlashOverlay != null)
        {
            winFlashOverlay.gameObject.SetActive(true);
            winFlashOverlay.color = new Color(winFlashColor.r, winFlashColor.g, winFlashColor.b, 0f);
            Sequence s = DOTween.Sequence().SetUpdate(true);
            s.Append(winFlashOverlay.DOFade(0.55f, 0.15f).SetEase(Ease.OutQuad));
            s.Append(winFlashOverlay.DOFade(0f, 0.6f).SetEase(Ease.InOutSine));
            s.OnComplete(() => winFlashOverlay.gameObject.SetActive(false));
        }

        yield return new WaitForSecondsRealtime(0.95f);

        int stars = CalculateStars();
        int coinsReward = CalculateCoinReward(stars);

        SaveSystem.Data.SetStarsForLevel(GameSession.CurrentLevel, stars);
        if (GameSession.CurrentLevel >= SaveSystem.Data.unlockedLevel)
        {
            SaveSystem.Data.unlockedLevel = Mathf.Min(GameSession.CurrentLevel + 1, 30);
        }
        SaveSystem.Data.coins += coinsReward;
        SaveSystem.Save();

        UpdateCoins();

        if (levelCompletePopup != null)
        {
            bool hasNext = GameSession.CurrentLevel < 30;
            levelCompletePopup.Show(stars, coinsReward, hasNext);
        }
    }

    private int CalculateStars()
    {
        int stars = 1;
        if (AllEnergyStarsCollected()) stars++;
        if (_moves <= testLevel.maxMovesForThreeStars) stars++;
        return Mathf.Clamp(stars, 1, 3);
    }

    private bool AllEnergyStarsCollected()
    {
        if (testLevel.energyStars == null || testLevel.energyStars.Count == 0) return true;
        return false;
    }

    private int CalculateCoinReward(int stars)
    {
        return 10 + stars * 5;
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
        _isWon = false;
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

    private void HandleReplay()
    {
        _isWon = false;
        ApplyLevelDefinition(testLevel);
        UpdateMoves(0);
        RecalculateRay();
        if (rayRenderer != null) rayRenderer.RevealAnimation();
    }

    private void HandleNext()
    {
        GameSession.CurrentLevel = Mathf.Min(GameSession.CurrentLevel + 1, 30);
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void HandleMenu()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(levelSelectSceneName);
        else
            SceneManager.LoadScene(levelSelectSceneName);
    }

    public void DebugTriggerWin()
    {
        for (int i = 0; i < _activeBatteries.Count; i++)
        {
            if (_activeBatteries[i] != null) _activeBatteries[i].Charge();
        }
        if (_activeBatteries.Count == 0) _isWon = false;
        else CheckWinCondition();
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
