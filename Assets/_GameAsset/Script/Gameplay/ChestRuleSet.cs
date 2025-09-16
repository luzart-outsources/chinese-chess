using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType { Pawn = 0, Knight = 1, Bishop = 2, Rook = 3, Queen = 4, King = 5 }

public sealed class ChessRuleSet : IRuleSet
{
    public GameRules Code => _up ? GameRules.ChessUpsideDown : GameRules.Chess;
    public int Rows => 8;
    public int Cols => 8;

    private readonly bool _up;
    public ChessRuleSet(bool upsideDown) { _up = upsideDown; }

    public IEnumerable<Vector2Int> GetValidMoves(BoardDataModel board, PieceModel p)
    {
        var res = new List<Vector2Int>();
        var ct = MapFromPieceType(p.type); // map từ PieceType hiện có

        switch (ct)
        {
            case ChessPieceType.Pawn: AddPawn(board, p.row, p.col, p.isRed, res); break;
            case ChessPieceType.Knight: AddKnight(board, p.row, p.col, p.isRed, res); break;
            case ChessPieceType.Bishop: AddSlide(board, p.row, p.col, p.isRed, res, diag: true, ortho: false); break;
            case ChessPieceType.Rook: AddSlide(board, p.row, p.col, p.isRed, res, diag: false, ortho: true); break;
            case ChessPieceType.Queen: AddSlide(board, p.row, p.col, p.isRed, res, diag: true, ortho: true); break;
            case ChessPieceType.King: AddKing(board, p.row, p.col, p.isRed, res); break;
        }
        return res;
    }

    public bool IsMoveLegal(BoardDataModel board, PieceModel p, Vector2Int to)
    {
        // TODO (nâng cao): lọc nước tự chiếu
        return true;
    }

    public void ApplyPostServer(BoardDataModel board, ServerMoveResult ack)
    {
        // TODO (nâng cao): phong cấp, en passant, castling nếu server báo
    }

    // ===== Helpers & generators =====
    static ChessPieceType MapFromPieceType(PieceType t) => t switch
    {
        PieceType.Soldier => ChessPieceType.Pawn,
        PieceType.Knight => ChessPieceType.Knight,
        PieceType.Elephant => ChessPieceType.Bishop,
        PieceType.Rook => ChessPieceType.Rook,
        PieceType.Advisor => ChessPieceType.Queen,
        PieceType.General => ChessPieceType.King,
        _ => ChessPieceType.Pawn
    };

    static bool InBoard(BoardDataModel b, int rr, int cc) => rr >= 0 && cc >= 0 && rr < b.rows && cc < b.cols;
    static void TryAdd(BoardDataModel b, int rr, int cc, bool myWhite, List<Vector2Int> res)
    {
        if (!InBoard(b, rr, cc)) return;
        var t = b.cells[rr, cc];
        if (t == null || t.isRed != myWhite) res.Add(new Vector2Int(rr, cc));
    }

    static void AddSlide(BoardDataModel b, int r, int c, bool white, List<Vector2Int> res, bool diag, bool ortho)
    {
        List<(int, int)> dirs = new();
        if (diag) { dirs.Add((-1, -1)); dirs.Add((-1, 1)); dirs.Add((1, -1)); dirs.Add((1, 1)); }
        if (ortho) { dirs.Add((-1, 0)); dirs.Add((1, 0)); dirs.Add((0, -1)); dirs.Add((0, 1)); }
        foreach (var (dr, dc) in dirs)
        {
            int rr = r + dr, cc = c + dc;
            while (InBoard(b, rr, cc))
            {
                var t = b.cells[rr, cc];
                if (t == null) res.Add(new Vector2Int(rr, cc));
                else { if (t.isRed != white) res.Add(new Vector2Int(rr, cc)); break; }
                rr += dr; cc += dc;
            }
        }
    }

    static void AddKnight(BoardDataModel b, int r, int c, bool white, List<Vector2Int> res)
    {
        int[,] L = { { -2, -1 }, { -2, 1 }, { 2, -1 }, { 2, 1 }, { -1, -2 }, { 1, -2 }, { -1, 2 }, { 1, 2 } };
        for (int i = 0; i < 8; i++) TryAdd(b, r + L[i, 0], c + L[i, 1], white, res);
    }

    static void AddKing(BoardDataModel b, int r, int c, bool white, List<Vector2Int> res)
    {
        for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                TryAdd(b, r + dr, c + dc, white, res);
            }
        // TODO: castling
    }

    static void AddPawn(BoardDataModel b, int r, int c, bool white, List<Vector2Int> res)
    {
        // Quy ước đang dùng: "Đỏ/White" đi xuống (tăng row) để thống nhất với Xiangqi
        int fwd = white ? +1 : -1;
        int rr = r + fwd, cc = c;

        // đi 1
        if (InBoard(b, rr, cc) && b.cells[rr, cc] == null)
            res.Add(new Vector2Int(rr, cc));

        // đi 2 từ hàng xuất phát
        int startRow = white ? 1 : (b.rows - 2);
        if (r == startRow && b.cells[rr, cc] == null)
        {
            int rr2 = rr + fwd;
            if (InBoard(b, rr2, cc) && b.cells[rr2, cc] == null)
                res.Add(new Vector2Int(rr2, cc));
        }

        // ăn chéo
        foreach (var dc in new int[] { -1, 1 })
        {
            int cr = r + fwd, cc2 = c + dc;
            if (!InBoard(b, cr, cc2)) continue;
            var t = b.cells[cr, cc2];
            if (t != null && t.isRed != white)
                res.Add(new Vector2Int(cr, cc2));
        }
        // TODO: en passant
    }
}
