using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnergyStarElement : MonoBehaviour
{
    public Image glowImage;
    public TextMeshProUGUI starText;
    public RectTransform rectTransform;

    public Vector2Int cell;

    public Color glowColor = new Color(1f, 0.85f, 0.25f, 0.45f);
    public Color starColorActive = new Color(1f, 0.85f, 0.25f, 1f);
    public Color starColorCollected = new Color(1f, 0.85f, 0.25f, 0.25f);

    private bool _isCollected;
    private Sequence _pulseSeq;

    public bool IsCollected => _isCollected;

    public void PlaceOnGrid(GridSystem grid)
    {
        Debug.Assert(rectTransform != null, "EnergyStarElement: rectTransform is null");
        rectTransform.anchoredPosition = grid.GetCellLocalPos(cell);
        float size = grid.CellSize * 0.7f;
        rectTransform.sizeDelta = new Vector2(size, size);
        if (starText != null) starText.fontSize = size * 0.85f;
    }

    public void SetCollectedImmediate(bool collected)
    {
        _isCollected = collected;
        ApplyVisualImmediate();
    }

    private void ApplyVisualImmediate()
    {
        if (_isCollected)
        {
            if (glowImage != null) glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);
            if (starText != null) starText.color = starColorCollected;
            if (rectTransform != null) rectTransform.localScale = Vector3.one * 0.7f;
            KillPulse();
        }
        else
        {
            if (glowImage != null) glowImage.color = glowColor;
            if (starText != null) starText.color = starColorActive;
            if (rectTransform != null) rectTransform.localScale = Vector3.one;
            StartPulse();
        }
    }

    public void Collect()
    {
        if (_isCollected) return;
        _isCollected = true;

        KillPulse();

        if (glowImage != null)
        {
            glowImage.DOKill();
            var glowRT = glowImage.rectTransform;
            glowRT.DOKill();
            glowRT.localScale = Vector3.one;
            Sequence burst = DOTween.Sequence();
            burst.Append(glowRT.DOScale(2.4f, 0.4f).SetEase(Ease.OutQuad));
            burst.Join(glowImage.DOFade(0f, 0.4f).SetEase(Ease.OutQuad));
        }

        if (starText != null)
        {
            starText.DOKill();
            starText.rectTransform.DOKill();
            starText.rectTransform.DOScale(0.7f, 0.3f).SetEase(Ease.InQuad);
            starText.DOColor(starColorCollected, 0.3f).SetEase(Ease.OutQuad);
        }

        HapticManager.Trigger(HapticManager.HapticType.Light);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
    }

    private void StartPulse()
    {
        KillPulse();
        if (glowImage == null) return;
        _pulseSeq = DOTween.Sequence();
        _pulseSeq.Append(glowImage.DOFade(glowColor.a * 0.4f, 0.8f).SetEase(Ease.InOutSine));
        _pulseSeq.Append(glowImage.DOFade(glowColor.a, 0.8f).SetEase(Ease.InOutSine));
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

    private void OnDisable()
    {
        KillPulse();
        if (rectTransform != null) rectTransform.DOKill();
        if (glowImage != null)
        {
            glowImage.DOKill();
            if (glowImage.rectTransform != null) glowImage.rectTransform.DOKill();
        }
        if (starText != null)
        {
            starText.DOKill();
            if (starText.rectTransform != null) starText.rectTransform.DOKill();
        }
    }
}
