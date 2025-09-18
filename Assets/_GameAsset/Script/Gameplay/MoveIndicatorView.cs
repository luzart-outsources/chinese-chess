using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum IndicatorMode { Move, Capture }

public class MoveIndicatorView : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject moveGO;     // GameObject con cho nước đi thường
    [SerializeField] private GameObject captureGO;  // GameObject con cho nước đi bắt quân

    // Tọa độ theo server coordinates
    private int row, col;
    private Action<int, int> onClick;

    /// <summary>
    /// Tự động tìm các GameObject con nếu chưa được gán (Editor helper)
    /// </summary>
    private void Reset()
    {
        // Thử tìm GameObject con theo tên chuẩn
        if (moveGO == null) moveGO = transform.Find("MoveGO")?.gameObject;
        if (captureGO == null) captureGO = transform.Find("CaptureGO")?.gameObject;
    }

    /// <summary>
    /// Liên kết indicator với tọa độ server và callback function
    /// </summary>
    /// <param name="serverRow">Hàng theo tọa độ server (0-9)</param>
    /// <param name="serverCol">Cột theo tọa độ server (0-8)</param>
    /// <param name="handler">Function được gọi khi indicator được click</param>
    public void Bind(int serverRow, int serverCol, Action<int, int> handler)
    {
        row = serverRow; 
        col = serverCol; 
        onClick = handler;
    }

    /// <summary>
    /// Thiết lập mode hiển thị của indicator
    /// Ẩn/hiện các GameObject tương ứng để tạo visual khác nhau
    /// </summary>
    /// <param name="mode">Mode hiển thị: Move (di chuyển thường) hoặc Capture (bắt quân)</param>
    public void SetMode(IndicatorMode mode)
    {
        // Hiển thị moveGO nếu là nước đi thường, ẩn đi nếu không
        if (moveGO) moveGO.SetActive(mode == IndicatorMode.Move);
        
        // Hiển thị captureGO nếu là nước đi bắt quân, ẩn đi nếu không
        if (captureGO) captureGO.SetActive(mode == IndicatorMode.Capture);
    }

    /// <summary>
    /// Xử lý sự kiện click vào indicator
    /// Gọi callback function với tọa độ server đã được bind
    /// </summary>
    /// <param name="eventData">Dữ liệu sự kiện pointer từ Unity Event System</param>
    public void OnClick()
    {
        onClick?.Invoke(row, col);
    }
}
