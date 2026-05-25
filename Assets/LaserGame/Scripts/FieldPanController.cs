using UnityEngine;
using UnityEngine.EventSystems;

public class FieldPanController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public RectTransform fieldRoot;
    public RectTransform viewport;
    public Vector2 contentSize;
    public float viewportScale = 0.85f;
    public float dragThresholdPixels = 8f;

    private Vector2 _dragStart;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragStart = fieldRoot != null ? fieldRoot.anchoredPosition : Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (fieldRoot == null || viewport == null) return;

        Vector2 newPos = fieldRoot.anchoredPosition + eventData.delta;
        ClampToBounds(ref newPos);
        fieldRoot.anchoredPosition = newPos;
    }

    public void OnEndDrag(PointerEventData eventData) { }

    public void SetContentSize(float width, float height)
    {
        contentSize = new Vector2(width, height);
        Recenter();
    }

    public void Recenter()
    {
        if (fieldRoot != null) fieldRoot.anchoredPosition = Vector2.zero;
    }

    public void SnapToContentLocalPos(Vector2 localPos)
    {
        if (fieldRoot == null || viewport == null) return;
        Vector2 newPos = -localPos;
        ClampToBounds(ref newPos);
        fieldRoot.anchoredPosition = newPos;
    }

    private void ClampToBounds(ref Vector2 pos)
    {
        Vector2 viewportSize = viewport.rect.size;
        Vector2 maxOffset = new Vector2(
            Mathf.Max(0f, (contentSize.x - viewportSize.x * viewportScale) * 0.5f),
            Mathf.Max(0f, (contentSize.y - viewportSize.y * viewportScale) * 0.5f)
        );
        pos.x = Mathf.Clamp(pos.x, -maxOffset.x, maxOffset.x);
        pos.y = Mathf.Clamp(pos.y, -maxOffset.y, maxOffset.y);
    }
}
