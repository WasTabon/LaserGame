using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LaserEmitter : MonoBehaviour
{
    public Image bodyImage;
    public Image glowImage;
    public TextMeshProUGUI arrowText;
    public RectTransform rectTransform;

    public Vector2Int cell = new Vector2Int(0, 0);
    public Vector2Int direction = new Vector2Int(1, 0);

    public Color emitterColor = new Color(0.2f, 0.95f, 1f, 1f);

    public void PlaceOnGrid(GridSystem grid)
    {
        Debug.Assert(rectTransform != null, "LaserEmitter: rectTransform is null");
        rectTransform.anchoredPosition = grid.GetCellLocalPos(cell);
        float size = grid.CellSize * 0.78f;
        rectTransform.sizeDelta = new Vector2(size, size);
        if (arrowText != null)
        {
            arrowText.fontSize = size * 0.7f;
        }
        ApplyDirectionVisual();
    }

    public void SetDirection(Vector2Int dir)
    {
        direction = dir;
        ApplyDirectionVisual();
    }

    public Vector2 GetExitLocalPos(GridSystem grid)
    {
        Vector2 center = grid.GetCellLocalPos(cell);
        Vector2 offset = new Vector2(direction.x, direction.y) * grid.CellSize * 0.4f;
        return center + offset;
    }

    private void ApplyDirectionVisual()
    {
        float angle = DirectionToAngle(direction);
        if (arrowText != null)
        {
            arrowText.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
        }
    }

    private float DirectionToAngle(Vector2Int dir)
    {
        if (dir == new Vector2Int(1, 0)) return 0f;
        if (dir == new Vector2Int(0, 1)) return 90f;
        if (dir == new Vector2Int(-1, 0)) return 180f;
        if (dir == new Vector2Int(0, -1)) return -90f;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    public void PulseAppear()
    {
        if (rectTransform == null) return;
        rectTransform.DOKill();
        Vector3 baseScale = Vector3.one;
        rectTransform.localScale = baseScale * 0.3f;
        rectTransform.DOScale(baseScale, 0.4f).SetEase(Ease.OutBack);
    }
}
