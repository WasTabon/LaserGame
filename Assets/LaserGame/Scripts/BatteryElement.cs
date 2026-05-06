using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BatteryElement : MonoBehaviour
{
    public Image bodyImage;
    public Image glowImage;
    public Image fillImage;
    public TextMeshProUGUI iconText;
    public RectTransform rectTransform;

    public Vector2Int cell;

    public Color bodyColor = new Color(0.105f, 0.105f, 0.18f, 0.98f);
    public Color glowDimColor = new Color(1f, 0.85f, 0.25f, 0.18f);
    public Color glowChargedColor = new Color(1f, 0.85f, 0.25f, 0.7f);
    public Color fillDimColor = new Color(0.45f, 0.5f, 0.6f, 0.5f);
    public Color fillChargedColor = new Color(1f, 0.85f, 0.25f, 1f);
    public Color iconDimColor = new Color(0.55f, 0.6f, 0.7f, 1f);
    public Color iconChargedColor = new Color(0.058f, 0.058f, 0.117f, 1f);

    private bool _isCharged;
    private Sequence _pulseSeq;

    public bool IsCharged => _isCharged;

    public void PlaceOnGrid(GridSystem grid)
    {
        Debug.Assert(rectTransform != null, "BatteryElement: rectTransform is null");
        rectTransform.anchoredPosition = grid.GetCellLocalPos(cell);
        float size = grid.CellSize * 0.78f;
        rectTransform.sizeDelta = new Vector2(size, size);
        if (iconText != null)
        {
            iconText.fontSize = size * 0.55f;
        }
    }

    public void SetChargedImmediate(bool charged)
    {
        _isCharged = charged;
        ApplyVisualImmediate();
    }

    public void Charge()
    {
        if (_isCharged) return;
        _isCharged = true;
        AnimateCharge();
    }

    public void Discharge()
    {
        if (!_isCharged) return;
        _isCharged = false;
        ApplyVisualImmediate();
        KillPulse();
    }

    private void ApplyVisualImmediate()
    {
        if (glowImage != null) glowImage.color = _isCharged ? glowChargedColor : glowDimColor;
        if (fillImage != null) fillImage.color = _isCharged ? fillChargedColor : fillDimColor;
        if (iconText != null) iconText.color = _isCharged ? iconChargedColor : iconDimColor;
        if (_isCharged) StartPulse();
        else KillPulse();
    }

    private void AnimateCharge()
    {
        if (rectTransform != null)
        {
            rectTransform.DOKill(false);
            rectTransform.localScale = Vector3.one;
            rectTransform.DOPunchScale(Vector3.one * 0.18f, 0.4f, 8, 0.6f);
        }
        if (glowImage != null)
        {
            glowImage.DOKill();
            glowImage.DOColor(glowChargedColor, 0.25f).SetEase(Ease.OutQuad);
        }
        if (fillImage != null)
        {
            fillImage.DOKill();
            fillImage.DOColor(fillChargedColor, 0.25f).SetEase(Ease.OutQuad);
        }
        if (iconText != null)
        {
            iconText.DOKill();
            iconText.DOColor(iconChargedColor, 0.2f).SetEase(Ease.OutQuad);
        }
        HapticManager.Trigger(HapticManager.HapticType.Light);
        StartPulse();
    }

    private void StartPulse()
    {
        KillPulse();
        if (glowImage == null) return;
        _pulseSeq = DOTween.Sequence();
        _pulseSeq.Append(glowImage.DOFade(glowChargedColor.a * 0.7f, 0.7f).SetEase(Ease.InOutSine));
        _pulseSeq.Append(glowImage.DOFade(glowChargedColor.a, 0.7f).SetEase(Ease.InOutSine));
        _pulseSeq.SetLoops(-1);
    }

    private void KillPulse()
    {
        if (_pulseSeq != null && _pulseSeq.IsActive())
        {
            _pulseSeq.Kill();
            _pulseSeq = null;
        }
        if (glowImage != null)
        {
            glowImage.DOKill();
            glowImage.color = _isCharged ? glowChargedColor : glowDimColor;
        }
    }

    private void OnDisable()
    {
        KillPulse();
        if (rectTransform != null) rectTransform.DOKill();
        if (fillImage != null) fillImage.DOKill();
        if (iconText != null) iconText.DOKill();
    }
}
