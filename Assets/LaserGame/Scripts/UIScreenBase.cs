using DG.Tweening;
using UnityEngine;

public class UIScreenBase : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float showDuration = 0.4f;

    protected virtual void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    protected virtual void OnEnable()
    {
        Show();
    }

    public virtual void Show()
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, showDuration).SetUpdate(true);
    }
}
