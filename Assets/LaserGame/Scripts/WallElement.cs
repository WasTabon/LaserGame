using UnityEngine;
using UnityEngine.UI;

public class WallElement : MonoBehaviour
{
    public Image bodyImage;
    public Image borderImage;
    public RectTransform rectTransform;

    public Vector2Int cell;

    public void PlaceOnGrid(GridSystem grid)
    {
        Debug.Assert(rectTransform != null, "WallElement: rectTransform is null");
        rectTransform.anchoredPosition = grid.GetCellLocalPos(cell);
        float size = grid.CellSize * 0.92f;
        rectTransform.sizeDelta = new Vector2(size, size);
    }
}
