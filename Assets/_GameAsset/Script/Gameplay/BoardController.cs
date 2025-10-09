using Assets._GameAsset.Script.Session;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public BoardView chineseChessView;
    public BoardView chessView;
    private BoardView view;

    [Header("Game Mode (Xiangqi legacy)")]
    public GameMode mode = GameMode.Standard; // vẫn giữ để tương thích Xiangqi up/standard

    [Header("RuleSet")]
    public GameRules rulesCode = GameRules.Xiangqi; // chọn Xiangqi / Chess / biến thể up
    private IRuleSet rules;
    private RulesContext ctx;

    private BoardDataModel data;
    private bool iAmRed = true;
    private bool myTurn = false;
    private bool inputLocked = false;

    private PieceModel lastSelectedModel;
    private PieceView lastSelectedView;

    private void Awake()
    {
        rules = RulesFactory.Create(rulesCode);
        ctx = new RulesContext { code = rulesCode, showOnlyLegal = false };
        // Khởi tạo tạm với kích thước theo RuleSet (sẽ được tạo lại khi nhận payload)
        data = new BoardDataModel(rules.Rows, rules.Cols);
    }
    public void InitializeRuleCode(EChessType eChessType)
    {
        rulesCode = (GameRules)(int)eChessType;
        rules = RulesFactory.Create(rulesCode);
        ctx = new RulesContext { code = rulesCode, showOnlyLegal = false };
        // Khởi tạo tạm với kích thước theo RuleSet (sẽ được tạo lại khi nhận payload)
        data = new BoardDataModel(rules.Rows, rules.Cols);
        if (eChessType == EChessType.ChinaChess || eChessType == EChessType.ChinaChessVisible)
        {
            chineseChessView.gameObject.SetActive(true);
            chessView.gameObject.SetActive(false);
            view = chineseChessView;
        }
        else
        {
            chineseChessView.gameObject.SetActive(false);
            chessView.gameObject.SetActive(true);
            view = chessView;
        }
        if (eChessType == EChessType.ChinaChessVisible || eChessType == EChessType.ChessVisible)
        {
            mode = GameMode.UpsideDown;
        }
        else
        {
            mode = GameMode.Standard;
        }
    }

    /// <summary>
    /// Khởi tạo từ payload mảng răng cưa.
    /// </summary>
    public void InitializeFromServer(InitPayload init)
    {
        iAmRed = init.iAmRed;
        myTurn = false;
        inputLocked = false;

        // Suy ra rows/cols từ mảng răng cưa
        int rows = (init.grid != null) ? init.grid.Length : rules.Rows;
        int cols = 0;
        if (rows > 0)
        {
            for (int r = 0; r < rows; r++)
                if (init.grid[r] != null && init.grid[r].Length > cols)
                    cols = init.grid[r].Length;
        }
        if (rows <= 0 || cols <= 0) { rows = rules.Rows; cols = rules.Cols; }

        // Tạo model theo kích thước payload
        data = new BoardDataModel(rows, cols);
        data.Clear();

        // View
        view.ClearAll();
        view.ConfigurePerspective(iAmRed, mySideAtBottom: true);
        view.ConfigureGrid(rows, cols);

        // Đặt quân theo payload răng cưa
        for (int r = 0; r < rows; r++)
        {
            var rowArr = (init.grid != null && r < init.grid.Length) ? init.grid[r] : null;
            if (rowArr == null) continue;
            for (int c = 0; c < rowArr.Length; c++)
            {
                var dto = rowArr[c];
                if (dto == null) continue;
                var p = new PieceModel(dto.id, dto.type, dto.isRed, dto.isShow, r, c);
                data.Place(p, r, c);
                var v = view.SpawnPiece(p.id, p.type, p.isRed, p.isShow, r, c);
                view.SetControllerFor(v, this);
            }
        }

        lastSelectedModel = null;
        lastSelectedView = null;
        view.ClearIndicators();
        view.DeselectAll(false);
    }

    public void SetMyTurn(bool isMyTurn)
    {
        this.myTurn = isMyTurn;
        this.inputLocked = false;
    }

    public void OnPieceClickedView(PieceView v)
    {
        if (inputLocked || !myTurn) return;
        var p = data.GetById(v.id);
        if (p == null) return;

        // Chỉ cho chọn quân của tôi (Xiangqi: Đỏ/Đen, Chess: White/Black)
        if (p.isRed != iAmRed) return;

        // Toggle
        if (lastSelectedView != null && lastSelectedView.id == v.id)
        {
            view.ClearIndicators();
            view.SetSelected(v.id, false, true);
            lastSelectedModel = null;
            lastSelectedView = null;
            return;
        }

        // Chọn mới
        if (lastSelectedView != null) view.SetSelected(lastSelectedView.id, false, true);
        view.ClearIndicators();

        var moves = new List<Vector2Int>(rules.GetValidMoves(data, p));
        if (moves.Count == 0)
        {
            lastSelectedModel = null;
            lastSelectedView = null;
            return;
        }

        view.SetSelected(v.id, true, true);
        lastSelectedModel = p;
        lastSelectedView = v;

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

        // Guard theo RuleSet nếu muốn lọc thêm
        if (!rules.IsMoveLegal(data, lastSelectedModel, new Vector2Int(targetRow, targetCol)))
            return;

        //inputLocked = true;
        view.ClearIndicators();
        if (lastSelectedView != null) view.SetSelected(lastSelectedView.id, false, true);


        GlobalServices.Instance.RequestMove(idPiece: (short)lastSelectedModel.id, toRow: (short)targetRow, toCol: (short)targetCol);

    }

    public void OnServerMoveResult(ServerMoveResult r)
    {
        inputLocked = false;
        var p = data.GetById(r.pieceId);
        if (p == null) return;

        // Lưu from trước khi di chuyển để suy luận đặc quyền
        int fromRow = p.row, fromCol = p.col;

        // --- En Passant (chỉ áp dụng cho Chess) ---
        bool isChess = rulesCode == GameRules.Chess || rulesCode == GameRules.ChessUpsideDown;
        bool maybeEnPassant = false;
        if (isChess)
        {
            // Ô đích trống + tốt đi chéo 1 cột => nghi ngờ en passant
            var targetCell = data.GetAt(r.newRow, r.newCol);
            bool targetEmpty = (targetCell == null);

            // Tự kiểm tra xem quân đang đi có phải là Tốt chess không
            bool isPawn = false;
            // Nếu enum PieceType của bạn chắc chắn “tốt chess” thì check trực tiếp:
            // isPawn = (p.type == PieceType.YourChessPawnEnum);
            // Nếu dùng chung enum cờ tướng/cờ vua, có thể nhờ map của ChessRuleSet (public static):
            isPawn = (ChessRuleSet.MapFromPieceType(p.type) == ChessPieceType.Tot);

            if (isPawn && targetEmpty && Math.Abs(r.newCol - fromCol) == 1 && Math.Abs(r.newRow - fromRow) == 1)
            {
                // Vị trí nạn nhân nằm ngay cùng hàng with fromRow, cột = newCol
                var victim = data.GetAt(fromRow, r.newCol);
                if (victim != null && victim.isRed != p.isRed &&
                    ChessRuleSet.MapFromPieceType(victim.type) == ChessPieceType.Tot)
                {
                    // Xóa nạn nhân en passant
                    data.RemoveAt(fromRow, r.newCol);
                    view.RemoveById(victim.id);
                    maybeEnPassant = true;
                }
            }
        }

        // Capture thường (ô đích có quân khác màu)
        var tgt = data.GetAt(r.newRow, r.newCol);
        if (tgt != null && tgt.isRed != p.isRed)
        {
            data.RemoveAt(r.newRow, r.newCol);
            view.RemoveById(tgt.id);
        }

        // Lật/ngửa & type (úp) – với Chess thì thường luôn show
        p.isShow = r.newType != PieceType.None;
        p.type = r.newType;

        // Di chuyển quân chính
        data.MoveTo(p, r.newRow, r.newCol);
        view.MovePieceTo(p.id, r.newRow, r.newCol);

        // Đồng bộ hiển thị
        view.SetShowState(p.id, p.isShow);
        view.SetType(p.id, p.type);

        // --- Castling (nếu là Chess): di chuyển xe luôn ---
        if (isChess)
        {
            bool isKing = (ChessRuleSet.MapFromPieceType(p.type) == ChessPieceType.Vua);
            int dx = r.newCol - fromCol;
            if (isKing && Math.Abs(dx) == 2 && fromRow == r.newRow)
            {
                // King-side: dx>0 ; Queen-side: dx<0
                int rookFromCol = (dx > 0) ? (data.cols - 1) : 0;
                int rookToCol = (dx > 0) ? (r.newCol - 1) : (r.newCol + 1);
                var rook = data.GetAt(fromRow, rookFromCol);
                if (rook != null && rook.isRed == p.isRed &&
                    ChessRuleSet.MapFromPieceType(rook.type) == ChessPieceType.Xe)
                {
                    data.MoveTo(rook, fromRow, rookToCol);
                    view.MovePieceTo(rook.id, fromRow, rookToCol);
                }
            }
        }

        // Hậu xử lý RuleSet (promotion…)
        rules.ApplyPostServer(data, r);

        lastSelectedModel = null;
        lastSelectedView = null;
        myTurn = false;
    }


    /// <summary>Soft reset: xoá quân + indicator trên view, reset data/model & input.</summary>
    public void ResetBoardKeepGrid()
    {
        // 1) Khoá input và clear chọn
        inputLocked = true;
        myTurn = false;
        lastSelectedModel = null;
        lastSelectedView = null;

        // 2) Xoá visual (quân + indicator) và dừng tween
        if (view) view.TeardownVisuals();

        // 3) Reset data (giữ size hiện tại để sẵn sàng init ván mới)
        int rows = data?.rows ?? rules.Rows;
        int cols = data?.cols ?? rules.Cols;
        data = new BoardDataModel(rows, cols);
        data.Clear();

        // 4) Mở khoá nếu cần (tuỳ luồng khởi tạo lại)
        inputLocked = false;
    }

    /// <summary>Hard close: soft reset + ẩn bàn (hoặc huỷ nếu bạn muốn).</summary>
    public void CloseBoard()
    {
        ResetBoardKeepGrid();

        //// Ẩn UI bàn cờ (tuỳ nhu cầu: có thể Destroy(view.gameObject))
        //if (view) view.SetBoardActive(false);

        // Nếu có đăng ký event vào GlobalServices, nhớ huỷ ở đây
        // GlobalServices.Instance.OnMoveResult -= OnServerMoveResult;  // ví dụ (nếu có)
    }

    /// <summary>Mở lại bàn (UI), sau đó gọi InitializeFromServer để dựng ván mới.</summary>
    public void OpenBoard()
    {
        //if (view) view.SetBoardActive(true);
    }

    // Gợi ý: bảo hiểm khi object bị disable/destroy
    private void OnDisable() { SafeTeardownIfNeeded(); }
    private void OnDestroy() { SafeTeardownIfNeeded(); }

    private void SafeTeardownIfNeeded()
    {
        try
        {
            if (view) view.TeardownVisuals();
        }
        catch { /* ignore */ }
    }

}
// Đây là DataServer truyền về, server sẽ truyền về 1 mảng này
[System.Serializable]
public class DataPieceServer
{
    public short id;
    public PieceType type;
    public bool isRed;
    public int x;
    public int y;
}



