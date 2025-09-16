using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditorInternal;

public class BoardView : MonoBehaviour
{
    [Header("Cells (kéo: dưới→trên, trái→phải)")]
    public List<Transform> cells;

    [Header("Prefabs")]
    public ChessPieceView piecePrefab;
    public MoveIndicatorView indicatorPrefab;   // ← chỉ 1 prefab

    [Header("Perspective")]
    public bool serverRowZeroAtTop = true;
    public bool mySideAtBottom = true;
    public bool iAmRed = true;

    private readonly Dictionary<int, ChessPieceView> viewsById = new();
    private readonly List<MoveIndicatorView> activeIndicators = new();

    [SerializeField] private bool useTween = true;
    [SerializeField] private float moveDuration = 0.15f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;

    public void ConfigurePerspective(bool iAmRed, bool mySideAtBottom = true)
    {
        this.iAmRed = iAmRed;
        this.mySideAtBottom = mySideAtBottom;
    }

    public void ClearAll()
    {
        foreach (var kv in viewsById) if (kv.Value) Destroy(kv.Value.gameObject);
        viewsById.Clear();
    }

    public ChessPieceView SpawnPiece(int id, PieceType type, bool isRed, bool isShow, int serverRow, int serverCol)
    {
        var go = Instantiate(piecePrefab, transform);
        var view = go.GetComponent<ChessPieceView>();

        // Khởi tạo data + tự render trong chính ChessPieceView
        view.InitData(id, type, isRed, isShow);

        // Đặt vị trí theo cell
        var t = TargetCell(serverRow, serverCol);
        go.transform.position = t.position;

        viewsById[id] = view;
        return view;
    }

    public void SetControllerFor(ChessPieceView v, BoardController c)
    {
        if (v != null) v.BindController(c);
    }

    public void SetControllerForAll(BoardController c)
    {
        foreach (var kv in viewsById) if (kv.Value) kv.Value.BindController(c);
    }

    public ChessPieceView GetViewById(int id)
    {
        return viewsById.TryGetValue(id, out var v) ? v : null;
    }

    public void MovePieceTo(int id, int serverRow, int serverCol)
    {
        if (!viewsById.TryGetValue(id, out var v) || v == null) return;
        var t = TargetCell(serverRow, serverCol);

        var tr = v.transform;
        tr.DOKill(); // kill mọi tween trên transform này cho chắc
        if (useTween)
            tr.DOMove(t.position, moveDuration).SetEase(moveEase);
        else
            tr.position = t.position;
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
        var v = GetViewById(id);
        if (v == null) return;
        v.SetShow(isShow); // tự ẩn/hiện pieceImage theo isShow
    }

    public void SetType(int id, PieceType newType)
    {
        var v = GetViewById(id);
        if (v == null) return;
        v.SetType(newType); // tự đổi sprite theo type mới
    }

    public void SetColor(int id, bool isRed)
    {
        var v = GetViewById(id);
        if (v == null) return;
        v.SetColor(isRed); // tự đổi sprite theo màu mới (nếu cần)
    }

    public void SetBaseSpriteFor(int id, Sprite baseSprite)
    {
        var v = GetViewById(id);
        if (v == null) return;
        v.SetBaseSprite(baseSprite);
    }

    // ------ mapping (row,col) server -> cell Transform theo perspective ------
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

        // cells xếp từ DƯỚI→TRÊN, TRÁI→PHẢI
        int rowFromBottom = serverRowZeroAtTop ? (rows - 1 - viewRow) : viewRow;
        int idx = rowFromBottom * cols + viewCol;

        return cells[idx];
    }

    // --- Indicator: mọi list sử dụng toạ độ SERVER ---
    public void ShowIndicators(IEnumerable<Vector2Int> allMoves,
                               IEnumerable<Vector2Int> captureMoves,
                               Action<int, int> onClick)
    {
        ClearIndicators();

        // tạo set để check nhanh ô bắt quân
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
            iv.SetMode(IndicatorMode.Move); // mặc định là di chuyển

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
        foreach (var kv in viewsById)
            if (kv.Value != null) kv.Value.SetSelected(false, animate);
    }

}
