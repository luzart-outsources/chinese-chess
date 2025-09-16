using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [Header("Refs")] public BoardView view;
    [Header("Game Mode")] public GameMode mode = GameMode.Standard;

    private BoardDataModel data;
    private bool iAmRed = true;
    private bool myTurn = false;
    private bool inputLocked = false;

    private PieceModel lastSelectedModel;    // dữ liệu quân đang chọn
    private ChessPieceView lastSelectedView; // view quân đang chọn

    private void Awake() { data = new BoardDataModel(); }

    public void InitializeFromServer(InitPayload init)
    {
        iAmRed = init.iAmRed;
        myTurn = init.myTurn;

        inputLocked = false;
        data.Clear();
        view.ClearAll();
        view.ConfigurePerspective(iAmRed, mySideAtBottom: true);

        var grid = init.grid10x9;
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 9; c++)
            {
                var dto = grid[r, c];
                if (dto == null) continue;

                var p = new PieceModel(dto.id, dto.type, dto.isRed, dto.isShow, r, c);
                data.Place(p, r, c);

                var v = view.SpawnPiece(p.id, p.type, p.isRed, p.isShow, r, c);
                view.SetControllerFor(v, this);
            }

        // đảm bảo trạng thái không bị “đang chọn”
        lastSelectedModel = null;
        lastSelectedView = null;
        view.ClearIndicators();
        view.DeselectAll(false);
    }

    public void SetMyTurn(bool isMyTurn) => myTurn = isMyTurn;

    public void OnPieceClickedView(ChessPieceView v)
    {
        if (inputLocked || !myTurn) return;

        var p = data.GetById(v.id);
        if (p == null) return;

        // Chỉ cho chọn quân của tôi
        if (p.isRed != iAmRed) return;

        // --- Toggle khi bấm lại chính quân đó ---
        if (lastSelectedView != null && lastSelectedView.id == v.id)
        {
            // đang chọn -> tắt
            view.ClearIndicators();
            view.SetSelected(v.id, false, true);
            lastSelectedModel = null;
            lastSelectedView = null;
            return;
        }

        // --- Chọn quân mới ---
        // 1) Thu nhỏ quân cũ (nếu có) + clear indicators
        if (lastSelectedView != null)
            view.SetSelected(lastSelectedView.id, false, true);
        view.ClearIndicators();

        // 2) Tính nước đi cho quân mới
        var cfg = (mode == GameMode.UpsideDown) ? RulesConfig.UpsideDown : RulesConfig.Standard;
        List<Vector2Int> moves = XiangqiRules.GetValidMoves(data, p, cfg);

        // Nếu không có nước đi (bị kẹt), không làm gì
        if (moves.Count == 0)
        {
            lastSelectedModel = null;
            lastSelectedView = null;
            return;
        }

        // 3) Đánh dấu chọn + phóng to
        view.SetSelected(v.id, true, true);
        lastSelectedModel = p;
        lastSelectedView = v;

        // 4) Phân loại capture để render chấm
        var captureMoves = new List<Vector2Int>();
        foreach (var mv in moves)
        {
            var t = data.GetAt(mv.x, mv.y);
            if (t != null && t.isRed != p.isRed) captureMoves.Add(mv);
        }

        view.ShowIndicators(moves, captureMoves, OnIndicatorClicked);
    }

    private void OnIndicatorClicked(int targetRow, int targetCol)
    {
        if (inputLocked || !myTurn || lastSelectedModel == null) return;

        inputLocked = true;

        // Ẩn indicator + bỏ chọn quân (thu nhỏ)
        view.ClearIndicators();
        if (lastSelectedView != null)
            view.SetSelected(lastSelectedView.id, false, true);

        // TODO: Gửi TCP: (lastSelectedModel.id, targetRow, targetCol)

        // Demo fake:
        StartCoroutine(CoFakeServerOk(targetRow, targetCol));
    }

    private System.Collections.IEnumerator CoFakeServerOk(int targetRow, int targetCol)
    {
        yield return new WaitForSeconds(0.15f);

        bool revealNow = (mode == GameMode.UpsideDown && lastSelectedModel != null && !lastSelectedModel.isShow);
        var revealType = InitialTypeResolver.ResolveFromInitial(
            lastSelectedModel.isRed, lastSelectedModel.initRow, lastSelectedModel.initCol);

        OnServerMoveResult(new ServerMoveResult
        {
            moveAllowed = true,
            pieceId = lastSelectedModel.id,
            newRow = targetRow,
            newCol = targetCol,
            newIsShow = revealNow ? true : lastSelectedModel.isShow,
            newType = revealNow ? revealType : lastSelectedModel.type,
            nextMyTurn = false
        });
    }

    public void OnServerMoveResult(ServerMoveResult r)
    {
        inputLocked = false;
        if (!r.moveAllowed) return;

        var p = data.GetById(r.pieceId);
        if (p == null) return;

        // Bắt quân nếu có
        var tgt = data.GetAt(r.newRow, r.newCol);
        if (tgt != null && tgt.isRed != p.isRed)
        {
            data.RemoveAt(r.newRow, r.newCol);
            view.RemoveById(tgt.id);
        }

        // Cập nhật lật/ngửa & type (cờ úp)
        p.isShow = r.newIsShow;
        p.type = r.newType;

        // Di chuyển
        data.MoveTo(p, r.newRow, r.newCol);
        view.MovePieceTo(p.id, r.newRow, r.newCol);

        // Đồng bộ hiển thị
        view.SetShowState(p.id, p.isShow);
        view.SetType(p.id, p.type);

        // Sau khi đi xong → không còn quân đang chọn
        lastSelectedModel = null;
        lastSelectedView = null;

        // Lượt kế tiếp do server quyết
        myTurn = r.nextMyTurn;
    }
}


public class ServerMoveResult
{
    public bool moveAllowed;
    public int pieceId;
    public int newRow, newCol;
    public bool newIsShow;
    public PieceType newType;
    public bool nextMyTurn; // server báo có phải lượt tôi sau khi áp dụng nước đi
}
