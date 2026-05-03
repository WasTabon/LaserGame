using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float pressedScale = 0.92f;
    public float pressDuration = 0.08f;
    public float releaseDuration = 0.15f;
    public bool playClickSound = true;

    private RectTransform _rect;
    private Vector3 _baseScale;
    private Button _button;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _baseScale = _rect.localScale;
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _rect.DOScale(_baseScale * pressedScale, pressDuration).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _rect.DOScale(_baseScale, releaseDuration).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _rect.DOScale(_baseScale, releaseDuration).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void OnClick()
    {
        if (playClickSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
        HapticManager.Trigger(HapticManager.HapticType.Light);
    }

    private void OnDestroy()
    {
        if (_button != null) _button.onClick.RemoveListener(OnClick);
    }
}
