using System;
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
    public RectTransform coinsIconRect;
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
    public WallElement wallTemplate;
    public EnergyStarElement energyStarTemplate;
    public SplitterElement splitterTemplate;

    [Header("Bottom")]
    public Button resetButton;
    public RectTransform resetRect;
    public CanvasGroup resetGroup;

    [Header("Win FX")]
    public Image winFlashOverlay;
    public LevelCompletePopup levelCompletePopup;

    [Header("Pause")]
    public Button pauseButton;
    public PausePopup pausePopup;
    public SettingsPopup gameSettingsPopup;

    [Header("Tutorial")]
    public TutorialHint tutorialHint;

    [Header("Coin Fly")]
    public RectTransform coinFlyHost;

    [Header("Test Level")]
    public LevelDefinition testLevel = new LevelDefinition();

    [Header("Levels Database")]
    public LevelsDatabaseSO levelsDatabase;

    [Header("Scenes")]
    public string levelSelectSceneName = "LevelSelect";

    public Color winFlashColor = new Color(0.2f, 0.95f, 1f, 1f);
    public Color coinFlyColor = new Color(1f, 0.85f, 0.25f, 1f);

    private int _moves;
    private int _displayedCoins;
    private bool _isWon;
    private Vector2 _topHudTarget;
    private Vector2 _subHudTarget;
    private Vector2 _resetTarget;
    private List<MirrorElement> _activeMirrors = new List<MirrorElement>();
    private List<BatteryElement> _activeBatteries = new List<BatteryElement>();
    private List<WallElement> _activeWalls = new List<WallElement>();
    private List<EnergyStarElement> _activeEnergyStars = new List<EnergyStarElement>();
    private List<SplitterElement> _activeSplitters = new List<SplitterElement>();
    private LevelDefinition _activeLevel;

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
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(OnPauseClicked);
            pauseButton.onClick.AddListener(OnPauseClicked);
        }
        if (levelCompletePopup != null)
        {
            levelCompletePopup.OnReplay = HandleReplay;
            levelCompletePopup.OnNext = HandleNext;
            levelCompletePopup.OnMenu = HandleMenu;
        }
        if (pausePopup != null)
        {
            pausePopup.OnResume = HandleResume;
            pausePopup.OnRestart = HandleRestart;
            pausePopup.OnSettings = HandleSettings;
            pausePopup.OnHome = HandleHome;
        }
    }

    private void OnDisable()
    {
        if (backButton != null) backButton.onClick.RemoveListener(OnBackClicked);
        if (resetButton != null) resetButton.onClick.RemoveListener(OnResetClicked);
        if (pauseButton != null) pauseButton.onClick.RemoveListener(OnPauseClicked);

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

        if (pausePopup != null)
        {
            pausePopup.OnResume = null;
            pausePopup.OnRestart = null;
            pausePopup.OnSettings = null;
            pausePopup.OnHome = null;
        }

        Time.timeScale = 1f;
    }

    private void Start()
    {
        _displayedCoins = SaveSystem.Data.coins;
        UpdateCoinsText(_displayedCoins);
        UpdateLevelText();
        var def = LevelLoader.LoadForLevel(GameSession.CurrentLevel, levelsDatabase, testLevel);
        _activeLevel = def;
        ApplyLevelDefinition(def);
        UpdateMoves(0);
        _isWon = false;
        AnimateIn();
        RecalculateRay();
        if (rayRenderer != null) rayRenderer.RevealAnimation();
        if (emitter != null) emitter.PulseAppear();

        if (AudioManager.Instance != null) AudioManager.Instance.PlayGameMusic();

        TryShowTutorial();
    }

    private void TryShowTutorial()
    {
        if (tutorialHint == null) return;
        if (GameSession.CurrentLevel != 1) { tutorialHint.gameObject.SetActive(false); return; }
        if (PlayerPrefs.GetInt("tutorial_shown_v1", 0) == 1) { tutorialHint.gameObject.SetActive(false); return; }
        if (_activeMirrors.Count == 0) { tutorialHint.gameObject.SetActive(false); return; }
        StartCoroutine(ShowTutorialDelayed());
    }

    private System.Collections.IEnumerator ShowTutorialDelayed()
    {
        yield return new WaitForSecondsRealtime(0.7f);
        if (_activeMirrors.Count > 0 && _activeMirrors[0] != null && _activeMirrors[0].rectTransform != null)
        {
            tutorialHint.ShowOn(_activeMirrors[0].rectTransform);
        }
    }

    private void UpdateCoinsText(int value)
    {
        if (coinsText != null) coinsText.text = value.ToString();
    }

    private void UpdateLevelText()
    {
        if (levelText != null) levelText.text = "LEVEL " + GameSession.CurrentLevel;
    }

    private void UpdateMoves(int value)
    {
        bool changed = value != _moves;
        _moves = value;
        if (movesText != null)
        {
            movesText.text = "MOVES: " + _moves;
            if (changed)
            {
                movesText.rectTransform.DOKill();
                movesText.rectTransform.localScale = Vector3.one;
                movesText.rectTransform.DOPunchScale(Vector3.one * 0.18f, 0.25f, 6, 0.5f);
            }
        }
    }

    public void ApplyLevelDefinition(LevelDefinition def)
    {
        Debug.Assert(grid != null, "GameController: grid is null");
        Debug.Assert(emitter != null, "GameController: emitter is null");
        Debug.Assert(def != null, "GameController: level definition is null");

        if (def.mirrors == null) def.mirrors = new List<MirrorPlacement>();
        if (def.batteries == null) def.batteries = new List<Vector2Int>();
        if (def.energyStars == null) def.energyStars = new List<Vector2Int>();
        if (def.walls == null) def.walls = new List<Vector2Int>();
        if (def.splitters == null) def.splitters = new List<SplitterPlacement>();

        grid.Build(def.cols, def.rows);
        emitter.cell = def.emitterCell;
        emitter.direction = def.emitterDir;
        emitter.PlaceOnGrid(grid);

        ClearAllElements();
        SpawnWalls(def.walls);
        SpawnEnergyStars(def.energyStars);
        SpawnSplitters(def.splitters);
        SpawnBatteries(def.batteries);
        SpawnMirrors(def.mirrors);
    }

    private void ClearAllElements()
    {
        ClearList(_activeMirrors, m =>
        {
            if (m != null) m.OnRotated -= HandleMirrorRotated;
        });
        ClearList(_activeBatteries, null);
        ClearList(_activeWalls, null);
        ClearList(_activeEnergyStars, null);
        ClearList(_activeSplitters, null);

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

    private void ClearList<T>(List<T> list, Action<T> onPreDestroy) where T : MonoBehaviour
    {
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item == null) continue;
            onPreDestroy?.Invoke(item);
            if (Application.isPlaying) Destroy(item.gameObject);
            else DestroyImmediate(item.gameObject);
        }
        list.Clear();
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
        if (elementsHolder == null) return;

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

    private void SpawnWalls(List<Vector2Int> placements)
    {
        if (placements == null || placements.Count == 0) return;
        if (wallTemplate == null) { Debug.LogWarning("GameController: wallTemplate is null"); return; }
        if (elementsHolder == null) return;

        for (int i = 0; i < placements.Count; i++)
        {
            var go = Instantiate(wallTemplate.gameObject, elementsHolder);
            go.name = "Wall_" + placements[i].x + "_" + placements[i].y;
            go.SetActive(true);
            var w = go.GetComponent<WallElement>();
            w.cell = placements[i];
            w.PlaceOnGrid(grid);
            _activeWalls.Add(w);
        }
    }

    private void SpawnEnergyStars(List<Vector2Int> placements)
    {
        if (placements == null || placements.Count == 0) return;
        if (energyStarTemplate == null) { Debug.LogWarning("GameController: energyStarTemplate is null"); return; }
        if (elementsHolder == null) return;

        for (int i = 0; i < placements.Count; i++)
        {
            var go = Instantiate(energyStarTemplate.gameObject, elementsHolder);
            go.name = "EnergyStar_" + placements[i].x + "_" + placements[i].y;
            go.SetActive(true);
            var s = go.GetComponent<EnergyStarElement>();
            s.cell = placements[i];
            s.PlaceOnGrid(grid);
            s.SetCollectedImmediate(false);
            _activeEnergyStars.Add(s);
        }
    }

    private void SpawnSplitters(List<SplitterPlacement> placements)
    {
        if (placements == null || placements.Count == 0) return;
        if (splitterTemplate == null) { Debug.LogWarning("GameController: splitterTemplate is null"); return; }
        if (elementsHolder == null) return;

        for (int i = 0; i < placements.Count; i++)
        {
            var p = placements[i];
            var go = Instantiate(splitterTemplate.gameObject, elementsHolder);
            go.name = "Splitter_" + p.cell.x + "_" + p.cell.y;
            go.SetActive(true);
            var sp = go.GetComponent<SplitterElement>();
            sp.cell = p.cell;
            sp.rotationStep = p.rotationStep;
            sp.PlaceOnGrid(grid);
            _activeSplitters.Add(sp);
        }
    }

    public void RecalculateRay()
    {
        if (rayRenderer == null || grid == null || emitter == null) return;
        var result = RayCalculator.Calculate(grid, emitter, _activeMirrors, GetWallCells(), _activeSplitters);
        rayRenderer.Render(result.segments, grid.CellSize);

        UpdateBatteryStates(result.visitedCells);
        UpdateEnergyStarStates(result.visitedCells);
        CheckWinCondition();
    }

    private List<Vector2Int> GetWallCells()
    {
        var list = new List<Vector2Int>(_activeWalls.Count);
        for (int i = 0; i < _activeWalls.Count; i++)
        {
            if (_activeWalls[i] != null) list.Add(_activeWalls[i].cell);
        }
        return list;
    }

    private void UpdateBatteryStates(HashSet<Vector2Int> visited)
    {
        for (int i = 0; i < _activeBatteries.Count; i++)
        {
            var b = _activeBatteries[i];
            if (b == null) continue;
            bool shouldBeCharged = visited.Contains(b.cell);
            if (shouldBeCharged && !b.IsCharged)
            {
                b.Charge();
                if (AudioManager.Instance != null) AudioManager.Instance.PlayBatteryCharge();
            }
            else if (!shouldBeCharged && b.IsCharged) b.Discharge();
        }
    }

    private void UpdateEnergyStarStates(HashSet<Vector2Int> visited)
    {
        for (int i = 0; i < _activeEnergyStars.Count; i++)
        {
            var s = _activeEnergyStars[i];
            if (s == null || s.IsCollected) continue;
            if (visited.Contains(s.cell))
            {
                CollectEnergyStar(s);
            }
        }
    }

    private void CollectEnergyStar(EnergyStarElement star)
    {
        Vector3 worldStart = star.rectTransform.position;
        star.Collect();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayEnergyStarCollect();

        const int rewardPerStar = 5;
        int oldTotal = SaveSystem.Data.coins;
        SaveSystem.Data.coins += rewardPerStar;
        SaveSystem.Save();

        PlayCoinFly(worldStart, oldTotal, SaveSystem.Data.coins);
    }

    private void PlayCoinFly(Vector3 worldStart, int displayStart, int displayEnd)
    {
        if (coinFlyHost == null || coinsIconRect == null) { UpdateCoinsText(displayEnd); return; }

        Vector3 worldEnd = coinsIconRect.position;

        var go = new GameObject("CoinFlyer", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(coinFlyHost, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(50, 50);
        rt.position = worldStart;

        var img = go.GetComponent<Image>();
        img.color = coinFlyColor;
        img.raycastTarget = false;

        rt.localScale = Vector3.one * 0.4f;
        Sequence s = DOTween.Sequence().SetUpdate(true);
        s.Append(rt.DOScale(1.1f, 0.18f).SetEase(Ease.OutBack));
        s.Append(rt.DOMove(worldEnd, 0.55f).SetEase(Ease.InOutQuad));
        s.Join(rt.DOScale(0.55f, 0.55f).SetEase(Ease.InQuad));
        s.OnComplete(() =>
        {
            UpdateCoinsText(displayEnd);
            PunchCoinsIcon();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            HapticManager.Trigger(HapticManager.HapticType.Light);
            Destroy(go);
        });

        StartCoroutine(TickCounterDuringFly(displayStart, displayEnd, 0.7f));
    }

    private IEnumerator TickCounterDuringFly(int from, int to, float duration)
    {
        if (from == to) yield break;
        int diff = to - from;
        float t = 0f;
        int prev = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            int current = from + Mathf.RoundToInt(diff * k);
            if (current != prev)
            {
                UpdateCoinsText(current);
                prev = current;
            }
            yield return null;
        }
        UpdateCoinsText(to);
    }

    private void PunchCoinsIcon()
    {
        if (coinsIconRect == null) return;
        coinsIconRect.DOKill();
        coinsIconRect.localScale = Vector3.one;
        coinsIconRect.DOPunchScale(Vector3.one * 0.35f, 0.3f, 6, 0.6f);
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
        if (AudioManager.Instance != null) AudioManager.Instance.PlayWin();

        if (grid != null) grid.PlayWinPulse(winFlashColor);
        if (fieldRoot != null)
        {
            fieldRoot.DOKill(false);
            fieldRoot.DOShakeAnchorPos(0.5f, 25f, 18, 90f, false, true);
        }

        SpawnConfetti();

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
        int winBonus = CalculateWinBonus(stars);

        SaveSystem.Data.SetStarsForLevel(GameSession.CurrentLevel, stars);
        if (GameSession.CurrentLevel >= SaveSystem.Data.unlockedLevel)
        {
            SaveSystem.Data.unlockedLevel = Mathf.Min(GameSession.CurrentLevel + 1, 30);
        }
        SaveSystem.Data.coins += winBonus;
        SaveSystem.Save();

        UpdateCoinsText(SaveSystem.Data.coins);
        _displayedCoins = SaveSystem.Data.coins;

        if (levelCompletePopup != null)
        {
            bool hasNext = GameSession.CurrentLevel < 30;
            levelCompletePopup.Show(stars, winBonus, hasNext);
        }
    }

    private void SpawnConfetti()
    {
        if (coinFlyHost == null || fieldRoot == null) return;
        Vector3 center = fieldRoot.position;
        var colors = new Color[]
        {
            new Color(0.2f, 0.95f, 1f, 1f),
            new Color(1f, 0.25f, 0.85f, 1f),
            new Color(1f, 0.85f, 0.25f, 1f),
            new Color(0.5f, 1f, 0.5f, 1f)
        };
        for (int i = 0; i < 24; i++)
        {
            var go = new GameObject("Confetti", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(coinFlyHost, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(UnityEngine.Random.Range(14f, 26f), UnityEngine.Random.Range(14f, 26f));
            rt.position = center;
            var img = go.GetComponent<Image>();
            img.color = colors[UnityEngine.Random.Range(0, colors.Length)];
            img.raycastTarget = false;

            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float distance = UnityEngine.Random.Range(280f, 620f);
            Vector3 target = center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0);

            float dur = UnityEngine.Random.Range(0.9f, 1.6f);
            Sequence s = DOTween.Sequence().SetUpdate(true);
            s.Append(rt.DOMove(target, dur).SetEase(Ease.OutQuad));
            s.Join(rt.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(-720f, 720f)), dur, RotateMode.FastBeyond360));
            s.Join(img.DOFade(0f, dur).SetEase(Ease.InQuad));
            var captured = go;
            s.OnComplete(() => Destroy(captured));
        }
    }

    private int CalculateStars()
    {
        int stars = 1;
        if (AllEnergyStarsCollected()) stars++;
        var lvl = _activeLevel != null ? _activeLevel : testLevel;
        if (_moves <= lvl.maxMovesForThreeStars) stars++;
        return Mathf.Clamp(stars, 1, 3);
    }

    private bool AllEnergyStarsCollected()
    {
        if (_activeEnergyStars.Count == 0) return true;
        for (int i = 0; i < _activeEnergyStars.Count; i++)
        {
            if (_activeEnergyStars[i] == null) continue;
            if (!_activeEnergyStars[i].IsCollected) return false;
        }
        return true;
    }

    private int CalculateWinBonus(int stars)
    {
        return 10 + stars * 5;
    }

    private void HandleMirrorRotated(MirrorElement m)
    {
        UpdateMoves(_moves + 1);
        RecalculateRay();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMirrorRotate();

        if (tutorialHint != null && tutorialHint.gameObject.activeSelf)
        {
            tutorialHint.Hide();
            PlayerPrefs.SetInt("tutorial_shown_v1", 1);
            PlayerPrefs.Save();
        }
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
        var lvl = _activeLevel != null ? _activeLevel : testLevel;

        float maxDist = 0.01f;
        for (int i = 0; i < _activeMirrors.Count; i++)
        {
            if (_activeMirrors[i] == null || _activeMirrors[i].rectTransform == null) continue;
            float d = _activeMirrors[i].rectTransform.anchoredPosition.magnitude;
            if (d > maxDist) maxDist = d;
        }

        for (int i = 0; i < _activeMirrors.Count && i < lvl.mirrors.Count; i++)
        {
            var m = _activeMirrors[i];
            if (m == null) continue;
            float dist = m.rectTransform != null ? m.rectTransform.anchoredPosition.magnitude : 0f;
            float delay = (dist / maxDist) * 0.25f;
            m.AnimateResetTo(lvl.mirrors[i].initialRotationStep, delay);
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

    private void OnPauseClicked()
    {
        Time.timeScale = 0f;
        if (pausePopup != null)
        {
            pausePopup.transform.SetAsLastSibling();
            pausePopup.Open();
        }
    }

    private void HandleResume()
    {
        Time.timeScale = 1f;
    }

    private void HandleRestart()
    {
        Time.timeScale = 1f;
        var def = _activeLevel != null ? _activeLevel : testLevel;
        ApplyLevelDefinition(def);
        UpdateMoves(0);
        _isWon = false;
        RecalculateRay();
        if (rayRenderer != null) rayRenderer.RevealAnimation();
        if (emitter != null) emitter.PulseAppear();
    }

    private void HandleSettings()
    {
        if (gameSettingsPopup != null)
        {
            gameSettingsPopup.transform.SetAsLastSibling();
            gameSettingsPopup.Open();
        }
    }

    private void HandleHome()
    {
        Time.timeScale = 1f;
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(levelSelectSceneName);
        else
            SceneManager.LoadScene(levelSelectSceneName);
    }

    private void HandleReplay()
    {
        _isWon = false;
        var def = _activeLevel != null ? _activeLevel : testLevel;
        ApplyLevelDefinition(def);
        UpdateMoves(0);
        RecalculateRay();
        if (rayRenderer != null) rayRenderer.RevealAnimation();
    }

    private void HandleNext()
    {
        Time.timeScale = 1f;
        GameSession.CurrentLevel = Mathf.Min(GameSession.CurrentLevel + 1, 30);
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void HandleMenu()
    {
        Time.timeScale = 1f;
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
