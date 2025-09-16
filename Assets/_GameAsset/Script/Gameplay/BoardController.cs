using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [Header("Refs")]
    public BoardView view;

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
    private ChessPieceView lastSelectedView;

    private void Awake()
    {
        rules = RulesFactory.Create(rulesCode);
        ctx = new RulesContext { code = rulesCode, showOnlyLegal = false };
        // Khởi tạo tạm với kích thước theo RuleSet (sẽ được tạo lại khi nhận payload)
        data = new BoardDataModel(rules.Rows, rules.Cols);
    }

    /// <summary>
    /// Khởi tạo từ payload mảng răng cưa.
    /// </summary>
    public void InitializeFromServer(InitPayload init)
    {
        iAmRed = init.iAmRed;
        myTurn = init.myTurn;
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

        // Nếu đang ở Xiangqi nhưng muốn “úp”, đồng bộ lại RuleSet
        if (rulesCode == GameRules.Xiangqi || rulesCode == GameRules.XiangqiUpsideDown)
        {
            rules = RulesFactory.Create(mode == GameMode.UpsideDown ? GameRules.XiangqiUpsideDown : GameRules.Xiangqi);
            ctx.code = rules.Code;
        }
    }

    public void SetMyTurn(bool isMyTurn) => myTurn = isMyTurn;

    public void OnPieceClickedView(ChessPieceView v)
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

        inputLocked = true;
        view.ClearIndicators();
        if (lastSelectedView != null) view.SetSelected(lastSelectedView.id, false, true);

        // TODO: Gửi TCP thực:
        // net.SendMove(new MoveRequest { pieceId = lastSelectedModel.id, toRow = targetRow, toCol = targetCol });

        // DEMO fake:
        StartCoroutine(CoFakeServerOk(targetRow, targetCol));
    }

    private System.Collections.IEnumerator CoFakeServerOk(int targetRow, int targetCol)
    {
        yield return new WaitForSeconds(0.15f);
        bool revealNow = (mode == GameMode.UpsideDown && lastSelectedModel != null && !lastSelectedModel.isShow);
        var revealType = InitialTypeResolver.ResolveFromInitial(lastSelectedModel.isRed, lastSelectedModel.initRow, lastSelectedModel.initCol);
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

        // Capture
        var tgt = data.GetAt(r.newRow, r.newCol);
        if (tgt != null && tgt.isRed != p.isRed)
        {
            data.RemoveAt(r.newRow, r.newCol);
            view.RemoveById(tgt.id);
        }

        // Lật/ngửa & type (úp)
        p.isShow = r.newIsShow;
        p.type = r.newType;

        // Di chuyển
        data.MoveTo(p, r.newRow, r.newCol);
        view.MovePieceTo(p.id, r.newRow, r.newCol);

        // Đồng bộ hiển thị
        view.SetShowState(p.id, p.isShow);
        view.SetType(p.id, p.type);

        // Cho RuleSet hậu xử lý nếu cần (promotion, …)
        rules.ApplyPostServer(data, r);

        lastSelectedModel = null;
        lastSelectedView = null;
        myTurn = r.nextMyTurn;
    }
}



