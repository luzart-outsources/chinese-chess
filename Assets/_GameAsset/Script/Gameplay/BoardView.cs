using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditorInternal;

/// <summary>
/// View chính của bàn cờ, quản lý hiển thị và animation của các quân cờ
/// Xử lý perspective, spawning, moving và các effect visual
/// </summary>
public class BoardView : MonoBehaviour
{
    [Header("Cells (kéo: dưới→trên, trái→phải)")]
    public List<Transform> cells; // 90 cells cho bàn cờ 10x9

    [Header("Prefabs")]
    public ChessPieceView piecePrefab;      // Prefab cho quân cờ
    public MoveIndicatorView indicatorPrefab;   // Prefab cho chấm chỉ nước đi

    [Header("Perspective")]
    public bool serverRowZeroAtTop = true;  // Row 0 từ server có ở trên không
    public bool mySideAtBottom = true;      // Quân của tôi có ở dưới không
    public bool iAmRed = true;             // Tôi có phải quân đỏ không

    // Dictionaries quản lý các view
    private readonly Dictionary<int, ChessPieceView> viewsById = new();
    private readonly List<MoveIndicatorView> activeIndicators = new();

    [SerializeField] private bool useTween = true;
    [SerializeField] private float moveDuration = 0.15f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;

    /// <summary>
    /// Cấu hình perspective xem bàn cờ
    /// Quyết định hướng nhìn và cách hiển thị bàn cờ cho người chơi
    /// </summary>
    /// <param name="iAmRed">True nếu người chơi là quân đỏ</param>
    /// <param name="mySideAtBottom">True nếu muốn quân của mình hiển thị ở dưới</param>
    public void ConfigurePerspective(bool iAmRed, bool mySideAtBottom = true)
    {
        this.iAmRed = iAmRed;
        this.mySideAtBottom = mySideAtBottom;
    }

    /// <summary>
    /// Xóa tất cả quân cờ trên bàn
    /// Destroy các GameObject và clear dictionary
    /// </summary>
    public void ClearAll()
    {
        foreach (var kv in viewsById) if (kv.Value) Destroy(kv.Value.gameObject);
        viewsById.Clear();
    }

    /// <summary>
    /// Tạo một quân cờ mới trên bàn cờ
    /// </summary>
    /// <param name="id">ID duy nhất của quân</param>
    /// <param name="type">Loại quân cờ</param>
    /// <param name="isRed">Màu quân (đỏ/đen)</param>
    /// <param name="isShow">Trạng thái úp/ngửa</param>
    /// <param name="serverRow">Vị trí hàng theo server (0-9)</param>
    /// <param name="serverCol">Vị trí cột theo server (0-8)</param>
    /// <returns>ChessPieceView đã được tạo</returns>
    public ChessPieceView SpawnPiece(int id, PieceType type, bool isRed, bool isShow, int serverRow, int serverCol)
    {
        var go = Instantiate(piecePrefab, transform);
        var view = go.GetComponent<ChessPieceView>();

        // Khởi tạo data và render trong ChessPieceView
        view.InitData(id, type, isRed, isShow);

        // Đặt vị trí theo cell tương ứng (có tính perspective)
        var t = TargetCell(serverRow, serverCol);
        go.transform.position = t.position;

        // Lưu vào dictionary để quản lý
        viewsById[id] = view;
        return view;
    }

    /// <summary>
    /// Liên kết một quân cờ với BoardController
    /// </summary>
    /// <param name="v">ChessPieceView cần bind</param>
    /// <param name="c">BoardController instance</param>
    public void SetControllerFor(ChessPieceView v, BoardController c)
    {
        if (v != null) v.BindController(c);
    }

    /// <summary>
    /// Liên kết tất cả quân cờ với BoardController
    /// </summary>
    /// <param name="c">BoardController instance</param>
    public void SetControllerForAll(BoardController c)
    {
        foreach (var kv in viewsById) if (kv.Value) kv.Value.BindController(c);
    }

    /// <summary>
    /// Lấy ChessPieceView theo ID
    /// </summary>
    /// <param name="id">ID của quân cờ</param>
    /// <returns>ChessPieceView hoặc null nếu không tìm thấy</returns>
    public ChessPieceView GetViewById(int id)
    {
        return viewsById.TryGetValue(id, out var v) ? v : null;
    }

    /// <summary>
    /// Di chuyển một quân cờ đến vị trí mới với animation
    /// </summary>
    /// <param name="id">ID của quân cờ</param>
    /// <param name="serverRow">Hàng đích theo server</param>
    /// <param name="serverCol">Cột đích theo server</param>
    public void MovePieceTo(int id, int serverRow, int serverCol)
    {
        if (!viewsById.TryGetValue(id, out var v) || v == null) return;
        var t = TargetCell(serverRow, serverCol);

        var tr = v.transform;
        tr.DOKill(); // Dừng mọi tween đang chạy trên transform này
        
        if (useTween)
            tr.DOMove(t.position, moveDuration).SetEase(moveEase);
        else
            tr.position = t.position;
    }

    /// <summary>
    /// Xóa một quân cờ khỏi bàn (khi bị bắt)
    /// </summary>
    /// <param name="id">ID của quân cờ cần xóa</param>
    public void RemoveById(int id)
    {
        if (viewsById.TryGetValue(id, out var v) && v)
        {
            Destroy(v.gameObject);
            viewsById.Remove(id);
        }
    }

    /// <summary>
    /// Cập nhật trạng thái hiển thị (úp/ngửa) của một quân
    /// </summary>
    /// <param name="id">ID của quân cờ</param>
    /// <param name="isShow">True nếu ngửa, false nếu úp</param>
    public void SetShowState(int id, bool isShow)
    {
        var v = GetViewById(id);
        if (v == null) return;
        v.SetShow(isShow); // ChessPieceView tự động ẩn/hiện pieceImage
    }

    /// <summary>
    /// Cập nhật loại quân (dùng cho cờ úp khi lật lần đầu)
    /// </summary>
    /// <param name="id">ID của quân cờ</param>
    /// <param name="newType">Loại quân mới</param>
    public void SetType(int id, PieceType newType)
    {
        var v = GetViewById(id);
        if (v == null) return;
        v.SetType(newType); // ChessPieceView tự động đổi sprite
    }

    /// <summary>
    /// Cập nhật màu của quân cờ
    /// </summary>
    /// <param name="id">ID của quân cờ</param>
    /// <param name="isRed">True nếu đỏ, false nếu đen</param>
    public void SetColor(int id, bool isRed)
    {
        var v = GetViewById(id);
        if (v == null) return;
        v.SetColor(isRed); // ChessPieceView tự động đổi sprite theo màu
    }

    /// <summary>
    /// Thiết lập sprite đế cho một quân cờ
    /// </summary>
    /// <param name="id">ID của quân cờ</param>
    /// <param name="baseSprite">Sprite cho đế</param>
    public void SetBaseSpriteFor(int id, Sprite baseSprite)
    {
        var v = GetViewById(id);
        if (v == null) return;
        v.SetBaseSprite(baseSprite);
    }

    /// <summary>
    /// Chuyển đổi tọa độ server (row,col) thành Transform cell tương ứng
    /// Có tính đến perspective để hiển thị đúng hướng cho người chơi
    /// </summary>
    /// <param name="serverRow">Hàng theo server (0-9)</param>
    /// <param name="serverCol">Cột theo server (0-8)</param>
    /// <returns>Transform của cell tương ứng</returns>
    private Transform TargetCell(int serverRow, int serverCol)
    {
        int rows = 10, cols = 9;
        int viewRow = serverRow, viewCol = serverCol;

        // Nếu tôi là ĐEN và muốn quân mình ở dưới → xoay 180°
        if (mySideAtBottom && !iAmRed)
        {
            viewRow = rows - 1 - serverRow;
            viewCol = cols - 1 - serverCol;
        }

        // cells List được xếp từ DƯỚI→TRÊN, TRÁI→PHẢI
        int rowFromBottom = serverRowZeroAtTop ? (rows - 1 - viewRow) : viewRow;
        int idx = rowFromBottom * cols + viewCol;

        return cells[idx];
    }

    /// <summary>
    /// Hiển thị các indicator nước đi với phân biệt di chuyển thường và bắt quân
    /// </summary>
    /// <param name="allMoves">Tất cả nước đi hợp lệ</param>
    /// <param name="captureMoves">Các nước đi bắt quân</param>
    /// <param name="onClick">Callback khi click vào indicator</param>
    public void ShowIndicators(IEnumerable<Vector2Int> allMoves,
                               IEnumerable<Vector2Int> captureMoves,
                               Action<int, int> onClick)
    {
        ClearIndicators();

        // Tạo HashSet để check nhanh ô bắt quân
        var capSet = new HashSet<(int r, int c)>();
        foreach (var m in captureMoves) capSet.Add((m.x, m.y));

        // Tạo indicator cho mỗi nước đi
        foreach (var m in allMoves)
        {
            var cell = TargetCell(m.x, m.y);
            var go = Instantiate(indicatorPrefab, transform);
            go.transform.position = cell.position;
            go.transform.SetAsLastSibling(); // Hiển thị trên cùng
            
            var iv = go;
            iv.Bind(m.x, m.y, onClick);
            
            // Thiết lập mode dựa trên có phải ô bắt quân không
            iv.SetMode(capSet.Contains((m.x, m.y)) ? IndicatorMode.Capture : IndicatorMode.Move);

            activeIndicators.Add(go);
        }
    }

    /// <summary>
    /// Hiển thị các indicator nước đi (chỉ di chuyển thường, không phân biệt bắt quân)
    /// </summary>
    /// <param name="allMoves">Tất cả nước đi hợp lệ</param>
    /// <param name="onClick">Callback khi click vào indicator</param>
    public void ShowIndicators(IEnumerable<Vector2Int> allMoves, Action<int, int> onClick)
    {
        ClearIndicators();
        foreach (var m in allMoves)
        {
            var cell = TargetCell(m.x, m.y);
            var go = Instantiate(indicatorPrefab, transform);
            go.transform.position = cell.position;
            go.transform.SetAsLastSibling();
            
            var iv = go;
            iv.Bind(m.x, m.y, onClick);
            iv.SetMode(IndicatorMode.Move); // Mặc định là di chuyển thường

            activeIndicators.Add(iv);
        }
    }

    /// <summary>
    /// Xóa tất cả indicator đang hiển thị
    /// </summary>
    public void ClearIndicators()
    {
        for (int i = 0; i < activeIndicators.Count; i++)
            if (activeIndicators[i]) Destroy(activeIndicators[i].gameObject);
        activeIndicators.Clear();
    }
    
    /// <summary>
    /// Thiết lập trạng thái chọn cho một quân cờ
    /// </summary>
    /// <param name="id">ID của quân cờ</param>
    /// <param name="selected">True nếu chọn, false nếu bỏ chọn</param>
    /// <param name="animate">True nếu có animation, false nếu không</param>
    public void SetSelected(int id, bool selected, bool animate = true)
    {
        var v = GetViewById(id);
        if (v != null) v.SetSelected(selected, animate);
    }

    /// <summary>
    /// Bỏ chọn tất cả quân cờ trên bàn
    /// </summary>
    /// <param name="animate">True nếu có animation, false nếu không</param>
    public void DeselectAll(bool animate = true)
    {
        foreach (var kv in viewsById)
            if (kv.Value != null) kv.Value.SetSelected(false, animate);
    }
}
