using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MirrorElement : MonoBehaviour
{
    public Image bodyImage;
    public Image glowImage;
    public Image diagonalLine;
    public Image ripple;
    public Button button;
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;

    public Vector2Int cell;
    public int rotationStep = 0;

    public Color rippleColor = new Color(1f, 0.25f, 0.85f, 0.55f);

    public event Action<MirrorElement> OnRotated;

    private float _visualRotationDeg;

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
        if (diagonalLine != null) diagonalLine.rectTransform.DOKill();
        if (rectTransform != null) rectTransform.DOKill();
        if (ripple != null) ripple.rectTransform.DOKill();
    }

    public void PlaceOnGrid(GridSystem grid)
    {
        Debug.Assert(rectTransform != null, "MirrorElement: rectTransform is null");
        rectTransform.anchoredPosition = grid.GetCellLocalPos(cell);
        float size = grid.CellSize * 0.78f;
        rectTransform.sizeDelta = new Vector2(size, size);
        if (diagonalLine != null)
        {
            float diag = size * Mathf.Sqrt(2f) * 0.95f;
            diagonalLine.rectTransform.sizeDelta = new Vector2(diag, size * 0.13f);
        }
        ApplyRotationImmediate();
    }

    public void ApplyRotationImmediate()
    {
        _visualRotationDeg = rotationStep == 0 ? 45f : -45f;
        if (diagonalLine != null)
        {
            diagonalLine.rectTransform.localEulerAngles = new Vector3(0, 0, _visualRotationDeg);
        }
    }

    public void ResetRotation(int step)
    {
        rotationStep = step;
        ApplyRotationImmediate();
    }

    private void HandleClick()
    {
        rotationStep = (rotationStep + 1) % 2;
        _visualRotationDeg += 90f;

        if (diagonalLine != null)
        {
            diagonalLine.rectTransform.DOKill();
            diagonalLine.rectTransform.DOLocalRotate(new Vector3(0, 0, _visualRotationDeg), 0.2f, RotateMode.FastBeyond360).SetEase(Ease.OutBack);
        }
        if (rectTransform != null)
        {
            rectTransform.DOKill(false);
            rectTransform.localScale = Vector3.one;
            rectTransform.DOPunchScale(Vector3.one * 0.12f, 0.3f, 6, 0.5f);
        }
        TriggerRipple();
        HapticManager.Trigger(HapticManager.HapticType.Light);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        OnRotated?.Invoke(this);
    }

    private void TriggerRipple()
    {
        if (ripple == null) return;
        var rt = ripple.rectTransform;
        rt.DOKill();
        rt.localScale = Vector3.one * 0.6f;
        ripple.color = rippleColor;
        Sequence s = DOTween.Sequence();
        s.Append(rt.DOScale(2.0f, 0.5f).SetEase(Ease.OutQuad));
        s.Join(ripple.DOFade(0f, 0.5f).SetEase(Ease.OutQuad));
    }
}
