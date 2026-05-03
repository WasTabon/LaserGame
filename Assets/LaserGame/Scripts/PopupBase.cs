using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PopupBase : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public RectTransform contentRect;
    public Image backdrop;

    public float openDuration = 0.3f;
    public float closeDuration = 0.2f;

    protected virtual void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPopupOpen();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        if (contentRect != null)
        {
            contentRect.localScale = Vector3.one * 0.7f;
            contentRect.DOScale(1f, openDuration).SetEase(Ease.OutBack).SetUpdate(true);
        }

        canvasGroup.DOFade(1f, openDuration).SetUpdate(true).OnComplete(() =>
        {
            canvasGroup.interactable = true;
        });
    }

    public virtual void Close()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPopupClose();

        canvasGroup.interactable = false;

        if (contentRect != null)
        {
            contentRect.DOScale(0.7f, closeDuration).SetEase(Ease.InBack).SetUpdate(true);
        }

        canvasGroup.DOFade(0f, closeDuration).SetUpdate(true).OnComplete(() =>
        {
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            OnClosed();
        });
    }

    protected virtual void OnClosed() { }
}
