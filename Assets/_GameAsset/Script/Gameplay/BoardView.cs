using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BoardView : MonoBehaviour
{
    [Header("Cells (kéo: dưới→trên, trái→phải)")]
    public List<Transform> cells;

    [Header("Prefabs")]
    public PieceView piecePrefab;
    public MoveIndicatorView indicatorPrefab;

    [Header("Perspective")]
    public bool serverRowZeroAtTop = true;
    public bool mySideAtBottom = true;
    public bool iAmRed = true;

    [SerializeField] private float moveDuration = 0.15f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;

    // Kích thước bàn hiện tại
    private int gridRows = 10;
    private int gridCols = 9;

    private readonly Dictionary<int, PieceView> viewsById = new();
    private readonly List<MoveIndicatorView> activeIndicators = new();

    public void ConfigurePerspective(bool iAmRed, bool mySideAtBottom = true)
    {
        this.iAmRed = iAmRed;
        this.mySideAtBottom = mySideAtBottom;
    }

    /// <summary>Thiết lập kích thước lưới cho TargetCell (phải khớp với số lượng cells đã kéo).</summary>
    public void ConfigureGrid(int rows, int cols)
    {
        gridRows = rows; gridCols = cols;
        int need = rows * cols;
        if (cells == null || cells.Count != need)
        {
            Debug.LogWarning($"[BoardView] cells.Count={cells?.Count ?? 0} != rows*cols={need}. Hãy gán đủ cell Transform trong Inspector.");
        }
    }

    public void ClearAll()
    {
        foreach (var kv in viewsById) if (kv.Value) Destroy(kv.Value.gameObject);
        viewsById.Clear();
        ClearIndicators();
    }

    public PieceView SpawnPiece(int id, PieceType type, bool isRed, bool isShow, int serverRow, int serverCol)
    {
        var go = Instantiate(piecePrefab, transform);
        var view = go;
        view.InitData(id, type, isRed, isShow);

        var t = TargetCell(serverRow, serverCol);
        go.transform.position = t.position;

        viewsById[id] = view;
        return view;
    }

    public void SetControllerFor(PieceView v, BoardController c) { if (v != null) v.BindController(c); }
    public void SetControllerForAll(BoardController c) { foreach (var kv in viewsById) if (kv.Value) kv.Value.BindController(c); }
    public PieceView GetViewById(int id) => viewsById.TryGetValue(id, out var v) ? v : null;

    public void MovePieceTo(int id, int serverRow, int serverCol)
    {
        if (!viewsById.TryGetValue(id, out var v) || v == null) return;
        var t = TargetCell(serverRow, serverCol);
        var tr = v.transform;
        tr.DOKill();
        tr.DOMove(t.position, moveDuration).SetEase(moveEase);
    }

    public void RemoveById(int id)
    {
        if (viewsById.TryGetValue(id, out var v) && v)
        {
            Destroy(v.gameObject);
            viewsById.Remove(id);
        }
    }

    public void SetShowState(int id, bool isShow)
    {
        var v = GetViewById(id); if (v == null) return;
        v.SetShow(isShow);
    }

    public void SetType(int id, PieceType newType)
    {
        var v = GetViewById(id); if (v == null) return;
        v.SetType(newType);
    }

    public void SetColor(int id, bool isRed)
    {
        var v = GetViewById(id); if (v == null) return;
        v.SetColor(isRed);
    }

    private Transform TargetCell(int serverRow, int serverCol)
    {
        int rows = gridRows, cols = gridCols;
        int viewRow = serverRow, viewCol = serverCol;

        // Nếu tôi là Đen/Black và muốn quân mình ở dưới → xoay 180°
        if (mySideAtBottom && !iAmRed)
        {
            viewRow = rows - 1 - serverRow;
            viewCol = cols - 1 - serverCol;
        }

        int rowFromBottom = serverRowZeroAtTop ? (rows - 1 - viewRow) : viewRow;
        int idx = rowFromBottom * cols + viewCol;

        if (idx < 0 || idx >= cells.Count)
        {
            Debug.LogError($"[BoardView] TargetCell index {idx} out of range. rows={rows} cols={cols} cells={cells.Count}");
            idx = Mathf.Clamp(idx, 0, Math.Max(0, cells.Count - 1));
        }
        return cells[idx];
    }

    public void ShowIndicators(IEnumerable<Vector2Int> allMoves, IEnumerable<Vector2Int> captureMoves, System.Action<int, int> onClick)
    {
        ClearIndicators();
        var capSet = new HashSet<(int r, int c)>();
        foreach (var m in captureMoves) capSet.Add((m.x, m.y));

        foreach (var m in allMoves)
        {
            var cell = TargetCell(m.x, m.y);
            var go = Instantiate(indicatorPrefab, transform);
            go.transform.position = cell.position;
            go.transform.SetAsLastSibling();
            var iv = go;
            iv.Bind(m.x, m.y, onClick);
            iv.SetMode(capSet.Contains((m.x, m.y)) ? IndicatorMode.Capture : IndicatorMode.Move);
            activeIndicators.Add(go);
        }
    }

    public void ShowIndicators(IEnumerable<Vector2Int> allMoves, System.Action<int, int> onClick)
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
            iv.SetMode(IndicatorMode.Move);
            activeIndicators.Add(iv);
        }
    }

    public void ClearIndicators()
    {
        for (int i = 0; i < activeIndicators.Count; i++)
            if (activeIndicators[i]) Destroy(activeIndicators[i].gameObject);
        activeIndicators.Clear();
    }

    public void SetSelected(int id, bool selected, bool animate = true)
    {
        var v = GetViewById(id);
        if (v != null) v.SetSelected(selected, animate);
    }

    public void DeselectAll(bool animate = true)
    {
        foreach (var kv in viewsById) if (kv.Value != null) kv.Value.SetSelected(false, animate);
    }

    public void KillAllTweens()
    {
        foreach (var kv in viewsById)
            if (kv.Value) kv.Value.transform.DOKill();
        // Indicator cũng là child
        for (int i = 0; i < activeIndicators.Count; i++)
            if (activeIndicators[i]) activeIndicators[i].transform.DOKill();
    }

    /// <summary>Xoá toàn bộ quân (PieceView) + indicator và dừng tween.</summary>
    public void TeardownVisuals()
    {
        KillAllTweens();

        // Gỡ controller trên từng PieceView (nếu có bind)
        foreach (var kv in viewsById)
        {
            var v = kv.Value;
            if (!v) continue;
            v.BindController(null); // tránh callback rơi vào object đã bị xoá
            Destroy(v.gameObject);
        }
        viewsById.Clear();

        ClearIndicators();
    }

    /// <summary>Ẩn/hiện toàn bộ bàn (giữ nguyên state trong bộ nhớ).</summary>
    public void SetBoardActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
