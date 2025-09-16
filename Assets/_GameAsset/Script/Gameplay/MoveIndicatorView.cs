using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum IndicatorMode { Move, Capture }

public class MoveIndicatorView : MonoBehaviour, IPointerClickHandler
{
    [Header("Refs")]
    [SerializeField] private GameObject moveGO;     // kéo object con
    [SerializeField] private GameObject captureGO;  // kéo object con

    // toạ độ THEO SERVER
    private int row, col;
    private Action<int, int> onClick;

    private void Reset()
    {
        // Thử tìm con theo tên (nếu bạn đặt đúng)
        if (moveGO == null) moveGO = transform.Find("MoveGO")?.gameObject;
        if (captureGO == null) captureGO = transform.Find("CaptureGO")?.gameObject;
    }

    public void Bind(int serverRow, int serverCol, Action<int, int> handler)
    {
        row = serverRow; col = serverCol; onClick = handler;
    }

    public void SetMode(IndicatorMode mode)
    {
        if (moveGO) moveGO.SetActive(mode == IndicatorMode.Move);
        if (captureGO) captureGO.SetActive(mode == IndicatorMode.Capture);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(row, col);
    }
}
