using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RayRenderer : MonoBehaviour
{
    public RectTransform segmentsHolder;
    public CanvasGroup pulseGroup;

    public Color beamColor = new Color(0.2f, 0.95f, 1f, 1f);
    public Color glowColor = new Color(0.2f, 0.95f, 1f, 0.32f);

    public float coreThicknessRatio = 0.07f;
    public float glowThicknessRatio = 0.22f;

    public float pulseMin = 0.85f;
    public float pulseMax = 1f;
    public float pulseDuration = 1.4f;

    private List<RectTransform> _coreSegments = new List<RectTransform>();
    private List<RectTransform> _glowSegments = new List<RectTransform>();
    private Sequence _pulseSeq;
    private float _currentCellSize = 100f;

    private void OnEnable()
    {
        StartPulse();
    }

    private void OnDisable()
    {
        KillPulse();
    }

    public void Render(List<RaySegment> segments, float cellSize)
    {
        Debug.Assert(segmentsHolder != null, "RayRenderer: segmentsHolder is null");
        _currentCellSize = cellSize;

        EnsureCapacity(segments.Count);

        for (int i = 0; i < _coreSegments.Count; i++)
        {
            bool active = i < segments.Count;
            _coreSegments[i].gameObject.SetActive(active);
            _glowSegments[i].gameObject.SetActive(active);
            if (!active) continue;

            ApplySegment(_coreSegments[i], segments[i], cellSize * coreThicknessRatio);
            ApplySegment(_glowSegments[i], segments[i], cellSize * glowThicknessRatio);
        }
    }

    private void ApplySegment(RectTransform rt, RaySegment seg, float thickness)
    {
        Vector2 delta = seg.toLocal - seg.fromLocal;
        float length = delta.magnitude;
        Vector2 mid = (seg.fromLocal + seg.toLocal) * 0.5f;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        rt.anchoredPosition = mid;
        rt.sizeDelta = new Vector2(length, thickness);
        rt.localEulerAngles = new Vector3(0, 0, angle);
    }

    private void EnsureCapacity(int count)
    {
        while (_coreSegments.Count < count)
        {
            _glowSegments.Add(CreateSegment("Glow", glowColor, true));
            _coreSegments.Add(CreateSegment("Core", beamColor, false));
        }
    }

    private RectTransform CreateSegment(string name, Color color, bool sendBack)
    {
        var go = new GameObject(name + "_" + segmentsHolder.childCount, typeof(RectTransform));
        go.transform.SetParent(segmentsHolder, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        if (sendBack) rt.SetAsFirstSibling();
        return rt;
    }

    private void StartPulse()
    {
        KillPulse();
        if (pulseGroup == null) return;
        _pulseSeq = DOTween.Sequence();
        _pulseSeq.Append(pulseGroup.DOFade(pulseMin, pulseDuration * 0.5f).SetEase(Ease.InOutSine));
        _pulseSeq.Append(pulseGroup.DOFade(pulseMax, pulseDuration * 0.5f).SetEase(Ease.InOutSine));
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

    public void RevealAnimation()
    {
        if (segmentsHolder == null) return;
        segmentsHolder.localScale = new Vector3(1f, 0.4f, 1f);
        segmentsHolder.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutCubic);
    }
}
