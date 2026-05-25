using UnityEngine;

public class EmitterAutoCenter : MonoBehaviour
{
    public LaserEmitter emitter;
    public FieldPanController panController;

    private Vector2Int _lastCell;
    private Vector2Int _lastDir;
    private bool _hasFirstApplied;

    private void Awake()
    {
        if (emitter == null) emitter = GetComponent<LaserEmitter>();
    }

    private void LateUpdate()
    {
        if (emitter == null) return;
        bool needsApply = !_hasFirstApplied || emitter.cell != _lastCell || emitter.direction != _lastDir;
        if (needsApply)
        {
            ApplyRotation();
            ApplyPan();
            _lastCell = emitter.cell;
            _lastDir = emitter.direction;
            _hasFirstApplied = true;
        }
    }

    public void Recenter()
    {
        ApplyRotation();
        ApplyPan();
    }

    private void ApplyRotation()
    {
        if (emitter == null || emitter.rectTransform == null) return;
        float angle = Mathf.Atan2(emitter.direction.y, emitter.direction.x) * Mathf.Rad2Deg;
        emitter.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
    }

    private void ApplyPan()
    {
        if (panController == null || emitter == null || emitter.rectTransform == null) return;
        panController.SnapToContentLocalPos(emitter.rectTransform.anchoredPosition);
    }
}
