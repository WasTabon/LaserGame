using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SplitterElement : MonoBehaviour
{
    public Image bodyImage;
    public Image glowImage;
    public Image diagonalLineA;
    public Image diagonalLineB;
    public RectTransform rectTransform;

    public Vector2Int cell;
    public int rotationStep = 0;

    private Sequence _pulseSeq;

    public void PlaceOnGrid(GridSystem grid)
    {
        Debug.Assert(rectTransform != null, "SplitterElement: rectTransform is null");
        rectTransform.anchoredPosition = grid.GetCellLocalPos(cell);
        float size = grid.CellSize * 0.78f;
        rectTransform.sizeDelta = new Vector2(size, size);
        float diag = size * 1.4f * 0.95f;
        float thickness = size * 0.13f;
        if (diagonalLineA != null) diagonalLineA.rectTransform.sizeDelta = new Vector2(diag, thickness);
        if (diagonalLineB != null) diagonalLineB.rectTransform.sizeDelta = new Vector2(diag, thickness);
        ApplyRotationImmediate();
    }

    public void ApplyRotationImmediate()
    {
        float angleA = rotationStep == 0 ? 45f : -45f;
        float angleB = angleA + 90f;
        if (diagonalLineA != null) diagonalLineA.rectTransform.localEulerAngles = new Vector3(0, 0, angleA);
        if (diagonalLineB != null) diagonalLineB.rectTransform.localEulerAngles = new Vector3(0, 0, angleB);
    }

    private void OnEnable()
    {
        StartPulse();
    }

    private void OnDisable()
    {
        KillPulse();
        if (rectTransform != null) rectTransform.DOKill();
        if (glowImage != null) glowImage.DOKill();
    }

    private void StartPulse()
    {
        KillPulse();
        if (glowImage == null) return;
        Color baseColor = glowImage.color;
        _pulseSeq = DOTween.Sequence();
        _pulseSeq.Append(glowImage.DOFade(baseColor.a * 0.55f, 0.9f).SetEase(Ease.InOutSine));
        _pulseSeq.Append(glowImage.DOFade(baseColor.a, 0.9f).SetEase(Ease.InOutSine));
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
}
