using UnityEngine;
using UnityEngine.UI;

public class GridSystem : MonoBehaviour
{
    public RectTransform fieldRect;
    public RectTransform cellsHolder;
    public Image cellTemplatePrefab;

    public int cols = 5;
    public int rows = 5;
    public float cellSpacing = 2f;

    public Color cellColor = new Color(0.07f, 0.07f, 0.13f, 1f);
    public Color fieldBgColor = new Color(0.04f, 0.04f, 0.09f, 1f);

    private float _cellSize;

    public float CellSize => _cellSize;
    public Vector2 FieldSize => fieldRect.rect.size;

    public void Build(int newCols, int newRows)
    {
        cols = Mathf.Max(1, newCols);
        rows = Mathf.Max(1, newRows);
        Build();
    }

    public void Build()
    {
        Debug.Assert(fieldRect != null, "GridSystem: fieldRect is null");
        Debug.Assert(cellsHolder != null, "GridSystem: cellsHolder is null");

        var size = fieldRect.rect.size;
        float cw = (size.x - cellSpacing * (cols + 1)) / cols;
        float ch = (size.y - cellSpacing * (rows + 1)) / rows;
        _cellSize = Mathf.Min(cw, ch);

        for (int i = cellsHolder.childCount - 1; i >= 0; i--)
        {
            var child = cellsHolder.GetChild(i);
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                var go = new GameObject("Cell_" + x + "_" + y, typeof(RectTransform));
                go.transform.SetParent(cellsHolder, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(_cellSize, _cellSize);
                rt.anchoredPosition = GetCellLocalPos(new Vector2Int(x, y));
                var img = go.AddComponent<Image>();
                img.color = cellColor;
                img.raycastTarget = false;
            }
        }
    }

    public Vector2 GetCellLocalPos(Vector2Int cell)
    {
        float xOffset = (cell.x - (cols - 1) * 0.5f) * _cellSize;
        float yOffset = (cell.y - (rows - 1) * 0.5f) * _cellSize;
        return new Vector2(xOffset, yOffset);
    }

    public bool IsInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < cols && cell.y >= 0 && cell.y < rows;
    }
}
