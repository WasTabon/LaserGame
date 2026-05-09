using DG.Tweening;
using TMPro;
using UnityEngine;

public class TutorialHint : MonoBehaviour
{
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI pointerText;

    private Sequence _pulseSeq;

    public void ShowOn(RectTransform target)
    {
        if (target == null) { Hide(); return; }
        Debug.Assert(rectTransform != null, "TutorialHint: rectTransform null");
        Debug.Assert(canvasGroup != null, "TutorialHint: canvasGroup null");

        gameObject.SetActive(true);
        Vector3 worldPos = target.position;
        worldPos.y += target.rect.height * 0.55f * target.lossyScale.y;
        transform.position = worldPos;

        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.one * 0.6f;

        canvasGroup.DOKill();
        rectTransform.DOKill();

        canvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
        rectTransform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);

        StartPulse();
    }

    public void Hide()
    {
        KillPulse();
        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() => gameObject.SetActive(false));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void StartPulse()
    {
        KillPulse();
        if (rectTransform == null) return;
        Vector2 basePos = rectTransform.anchoredPosition;
        _pulseSeq = DOTween.Sequence().SetUpdate(true);
        _pulseSeq.Append(rectTransform.DOScale(1.15f, 0.55f).SetEase(Ease.InOutSine));
        _pulseSeq.Append(rectTransform.DOScale(1f, 0.55f).SetEase(Ease.InOutSine));
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
        if (canvasGroup != null) canvasGroup.DOKill();
        if (rectTransform != null) rectTransform.DOKill();
    }
}
