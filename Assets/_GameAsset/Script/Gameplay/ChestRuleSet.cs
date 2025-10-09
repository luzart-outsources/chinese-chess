using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ChessRuleSet : IRuleSet
{
    public GameRules Code => _up ? GameRules.ChessUpsideDown : GameRules.Chess;
    public int Rows => 8;
    public int Cols => 8;

    private readonly bool _up;              // lật 2 phía bàn (đổi bên trên/dưới)
    private readonly bool _redAtBottom;     // true: đỏ ở dưới (mặc định)

    // Trạng thái en passant cho lượt kế tiếp
    // target: ô mà tốt sẽ "đi tới" khi bắt qua đường; victim: vị trí con tốt bị bắt
    private Vector2Int? _enPassantTarget = null;
    private Vector2Int? _enPassantVictim = null;

    public ChessRuleSet(bool upsideDown, bool redAtBottom = true)
    {
        _up = upsideDown;
        _redAtBottom = redAtBottom;
    }

    // ================= Public API =================

    public IEnumerable<Vector2Int> GetValidMoves(BoardDataModel board, PieceModel p)
    {
        var res = new List<Vector2Int>();
        var ct = MapFromPieceType(p.type);

        switch (ct)
        {
            case ChessPieceType.Tot:
                AddPawn(board, p.row, p.col, p.isRed, res);
                AddPawnEnPassant(board, p, res);
                break;

            case ChessPieceType.Ma:
                AddKnight(board, p.row, p.col, p.isRed, res);
                break;

            case ChessPieceType.Tuong: // Bishop
                AddSlide(board, p.row, p.col, p.isRed, res, diag: true, ortho: false);
                break;

            case ChessPieceType.Xe: // Rook
                AddSlide(board, p.row, p.col, p.isRed, res, diag: false, ortho: true);
                break;

            case ChessPieceType.Hau: // Queen
                AddSlide(board, p.row, p.col, p.isRed, res, diag: true, ortho: true);
                break;

            case ChessPieceType.Vua:
                AddKing(board, p.row, p.col, p.isRed, res);
                // TODO: castling (cần cờ "đã di chuyển")
                break;
        }

        return res;
    }

    public bool IsMoveLegal(BoardDataModel board, PieceModel p, Vector2Int to)
    {
        // 1) "to" phải nằm trong danh sách nước đi sinh học thô
        bool ok = false;
        foreach (var mv in GetValidMoves(board, p))
            if (mv.x == to.x && mv.y == to.y) { ok = true; break; }
        if (!ok) return false;

        // 2) Giả lập (kể cả en passant) rồi kiểm tra tự chiếu
        var fromR = p.row; var fromC = p.col;
        var savedFrom = board.cells[fromR, fromC];
        var savedTo = board.cells[to.x, to.y];

        bool isPawn = MapFromPieceType(p.type) == ChessPieceType.Tot;
        bool isEnPassant =
            isPawn &&
            _enPassantTarget.HasValue &&
            _enPassantVictim.HasValue &&
            to == _enPassantTarget.Value &&
            savedTo == null;

        PieceModel removedVictim = null;
        Vector2Int victimPos = default;

        // Thực hiện giả lập
        board.cells[fromR, fromC] = null;
        int oldR = p.row, oldC = p.col;
        p.row = to.x; p.col = to.y;

        if (isEnPassant)
        {
            victimPos = _enPassantVictim.Value;
            removedVictim = board.cells[victimPos.x, victimPos.y];
            board.cells[victimPos.x, victimPos.y] = null; // xóa tốt bị bắt
        }

        board.cells[p.row, p.col] = p;

        bool illegal = IsKingInCheck(board, p.isRed);

        // Hoàn tác
        board.cells[oldR, oldC] = savedFrom;
        board.cells[to.x, to.y] = savedTo;
        if (isEnPassant && removedVictim != null)
            board.cells[victimPos.x, victimPos.y] = removedVictim;

        p.row = oldR; p.col = oldC;

        return !illegal;
    }

    public void ApplyPostServer(BoardDataModel board, ServerMoveResult ack)
    {
        // Clear mặc định; nếu phát sinh quyền en passant trong nước vừa rồi sẽ set lại phía dưới
        _enPassantTarget = null;
        _enPassantVictim = null;

        // --- (A) Nếu server trả trực tiếp thông tin en passant ---
        if (TryReadEnPassantFromAck(ack, out var epTarget, out var epVictim))
        {
            _enPassantTarget = epTarget;
            _enPassantVictim = epVictim;
        }
        else
        {
            // --- (B) Suy luận en passant khi tốt vừa đi 2 ô ---
            if (TryInferDoublePushFromAck(board, ack, out epTarget, out epVictim))
            {
                _enPassantTarget = epTarget;
                _enPassantVictim = epVictim;
            }
        }

        // === Promotion ===
        // Nếu server KHÔNG trả loại quân sau phong cấp, ta tự xét theo vị trí
        TryInferPromotionFromAck(board, ack);
    }

    // =============== Mapping PieceType -> ChessPieceType ===============

    public static ChessPieceType MapFromPieceType(PieceType t)
    {
        // Chỉnh theo enum thật của bạn nếu thứ tự khác
        switch ((int)t)
        {
            case 10: return ChessPieceType.Tot;   // Pawn
            case 11: return ChessPieceType.Ma;    // Knight
            case 12: return ChessPieceType.Tuong; // Bishop
            case 13: return ChessPieceType.Xe;    // Rook
            case 14: return ChessPieceType.Hau;   // Queen
            case 15: return ChessPieceType.Vua;   // King
            default: return ChessPieceType.Tot;
        }
    }

    // =================== Helpers cơ bản ===================

    static bool InBoard(BoardDataModel b, int rr, int cc)
        => rr >= 0 && cc >= 0 && rr < b.rows && cc < b.cols;

    static void TryAdd(BoardDataModel b, int rr, int cc, bool myIsRed, List<Vector2Int> res)
    {
        if (!InBoard(b, rr, cc)) return;
        var t = b.cells[rr, cc];
        if (t == null || t.isRed != myIsRed)
            res.Add(new Vector2Int(rr, cc));
    }

    // Hướng tiến của Tốt theo cấu hình (row tăng là đi xuống)
    int Forward(bool isRed, BoardDataModel b)
    {
        int dirRed = _redAtBottom ? -1 : +1;   // đỏ ở dưới -> đi lên (row -1)
        int dirBlack = -dirRed;                  // đen ngược lại
        return isRed ? dirRed : dirBlack;
    }

    int StartRow(bool isRed, BoardDataModel b)
    {
        int redStart = _redAtBottom ? (b.rows - 2) : 1;
        int blackStart = _redAtBottom ? 1 : (b.rows - 2);
        if (_up) { redStart = b.rows - 1 - redStart; blackStart = b.rows - 1 - blackStart; }
        return isRed ? redStart : blackStart;
    }

    int PromotionRow(bool isRed, BoardDataModel b)
    {
        int redPromo = _redAtBottom ? 0 : (b.rows - 1);
        int blackPromo = _redAtBottom ? (b.rows - 1) : 0;
        if (_up) { redPromo = b.rows - 1 - redPromo; blackPromo = b.rows - 1 - blackPromo; }
        return isRed ? redPromo : blackPromo;
    }

    // =================== Generators ===================

    void AddSlide(BoardDataModel b, int r, int c, bool isRed, List<Vector2Int> res, bool diag, bool ortho)
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
                else { if (t.isRed != isRed) res.Add(new Vector2Int(rr, cc)); break; }
                rr += dr; cc += dc;
            }
        }
    }

    void AddKnight(BoardDataModel b, int r, int c, bool isRed, List<Vector2Int> res)
    {
        int[,] L = { { -2, -1 }, { -2, 1 }, { 2, -1 }, { 2, 1 }, { -1, -2 }, { 1, -2 }, { -1, 2 }, { 1, 2 } };
        for (int i = 0; i < 8; i++) TryAdd(b, r + L[i, 0], c + L[i, 1], isRed, res);
    }

    void AddKing(BoardDataModel b, int r, int c, bool isRed, List<Vector2Int> res)
    {
        for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
                if (dr != 0 || dc != 0) TryAdd(b, r + dr, c + dc, isRed, res);

        // TODO: castling (cần biết "đã di chuyển", ô trống, và ô đi qua/đến không bị chiếu)
    }

    void AddPawn(BoardDataModel b, int r, int c, bool isRed, List<Vector2Int> res)
    {
        int fwd = Forward(isRed, b);

        // Đi 1
        int r1 = r + fwd, c1 = c;
        if (InBoard(b, r1, c1) && b.cells[r1, c1] == null)
            res.Add(new Vector2Int(r1, c1));

        // Đi 2 từ hàng xuất phát
        int sRow = StartRow(isRed, b);
        if (r == sRow && b.cells[r1, c1] == null)
        {
            int r2 = r1 + fwd;
            if (InBoard(b, r2, c1) && b.cells[r2, c1] == null)
                res.Add(new Vector2Int(r2, c1));
        }

        // Ăn chéo
        foreach (var dc in new int[] { -1, 1 })
        {
            int cr = r + fwd, cc = c + dc;
            if (!InBoard(b, cr, cc)) continue;
            var t = b.cells[cr, cc];
            if (t != null && t.isRed != isRed)
                res.Add(new Vector2Int(cr, cc));
        }
    }

    void AddPawnEnPassant(BoardDataModel b, PieceModel p, List<Vector2Int> res)
    {
        if (!_enPassantTarget.HasValue || !_enPassantVictim.HasValue) return;
        if (MapFromPieceType(p.type) != ChessPieceType.Tot) return;

        var target = _enPassantTarget.Value;
        var victim = _enPassantVictim.Value;

        int fwd = Forward(p.isRed, b);

        // Với en passant, ô đích phải là (p.row + fwd, p.col +/- 1) và ô đó phải trống
        if (p.row + fwd == target.x && Math.Abs(p.col - target.y) == 1 && b.cells[target.x, target.y] == null)
        {
            var victimPiece = b.cells[victim.x, victim.y];
            if (victimPiece != null &&
                victimPiece.isRed != p.isRed &&
                MapFromPieceType(victimPiece.type) == ChessPieceType.Tot)
            {
                res.Add(target);
            }
        }
    }

    // =================== Kiểm tra chiếu ===================

    bool IsKingInCheck(BoardDataModel b, bool kingIsRed)
    {
        // tìm vua
        int kr = -1, kc = -1;
        for (int r = 0; r < b.rows; r++)
        {
            for (int c = 0; c < b.cols; c++)
            {
                var t = b.cells[r, c];
                if (t != null && t.isRed == kingIsRed && MapFromPieceType(t.type) == ChessPieceType.Vua)
                { kr = r; kc = c; break; }
            }
            if (kr >= 0) break;
        }
        if (kr < 0) return false; // không thấy vua (board không hợp lệ)

        // quân địch có đánh vào (kr,kc)?
        for (int r = 0; r < b.rows; r++)
            for (int c = 0; c < b.cols; c++)
            {
                var q = b.cells[r, c];
                if (q == null || q.isRed == kingIsRed) continue;
                if (AttacksSquare(b, q, kr, kc)) return true;
            }
        return false;
    }

    bool AttacksSquare(BoardDataModel b, PieceModel q, int tr, int tc)
    {
        var ct = MapFromPieceType(q.type);
        switch (ct)
        {
            case ChessPieceType.Ma:
                {
                    int[,] L = { { -2, -1 }, { -2, 1 }, { 2, -1 }, { 2, 1 }, { -1, -2 }, { 1, -2 }, { -1, 2 }, { 1, 2 } };
                    for (int i = 0; i < 8; i++)
                        if (q.row + L[i, 0] == tr && q.col + L[i, 1] == tc) return true;
                    return false;
                }

            case ChessPieceType.Vua:
                return Mathf.Abs(q.row - tr) <= 1 && Mathf.Abs(q.col - tc) <= 1;

            case ChessPieceType.Tot:
                {
                    int fwd = Forward(q.isRed, b);
                    return (q.row + fwd == tr) && (Mathf.Abs(q.col - tc) == 1);
                }

            case ChessPieceType.Tuong: // Bishop
                return RayHits(b, q.row, q.col, tr, tc, diag: true, ortho: false);

            case ChessPieceType.Xe:    // Rook
                return RayHits(b, q.row, q.col, tr, tc, diag: false, ortho: true);

            case ChessPieceType.Hau:   // Queen
                return RayHits(b, q.row, q.col, tr, tc, diag: true, ortho: true);
        }
        return false;
    }

    bool RayHits(BoardDataModel b, int r, int c, int tr, int tc, bool diag, bool ortho)
    {
        List<(int, int)> dirs = new();
        if (diag) { dirs.Add((-1, -1)); dirs.Add((-1, 1)); dirs.Add((1, -1)); dirs.Add((1, 1)); }
        if (ortho) { dirs.Add((-1, 0)); dirs.Add((1, 0)); dirs.Add((0, -1)); dirs.Add((0, 1)); }

        foreach (var (dr, dc) in dirs)
        {
            int rr = r + dr, cc = c + dc;
            while (InBoard(b, rr, cc))
            {
                if (rr == tr && cc == tc) return true;
                if (b.cells[rr, cc] != null) break; // bị chặn
                rr += dr; cc += dc;
            }
        }
        return false;
    }

    // =================== Promotion helpers ===================

    void TryLocalPromotion(BoardDataModel b, PieceModel moved)
    {
        if (MapFromPieceType(moved.type) != ChessPieceType.Tot) return;
        int promoRow = PromotionRow(moved.isRed, b);
        if (moved.row == promoRow)
        {
            // Nâng mặc định lên Hậu (Queen)
            // CHÚ Ý: map đúng enum PieceType của bạn cho "Hau"
            moved.type = (PieceType)4; // 4 ≈ Hau trong MapFromPieceType ở trên
        }
    }

    void TryInferPromotionFromAck(BoardDataModel b, ServerMoveResult ack)
    {
        // (1) Nếu server nói rõ quân sau phong cấp -> set đúng loại (bạn map lại ở đây)
        if (TryReadPromotionFromAck(ack, out var pos, out var newType))
        {
            var p = b.cells[pos.x, pos.y];
            if (p != null && MapFromPieceType(p.type) == ChessPieceType.Tot)
                p.type = newType;
            return;
        }

        // (2) Nếu server không nói rõ: thử tìm quân vừa di chuyển
        if (TryReadMovedFromTo(ack, out var fr, out var to))
        {
            var moved = b.cells[to.x, to.y];
            if (moved != null) TryLocalPromotion(b, moved);
        }
    }

    // =================== ACK parsers (tùy server, điền field thật của bạn) ===================

    bool TryReadEnPassantFromAck(ServerMoveResult ack, out Vector2Int target, out Vector2Int victim)
    {
        target = default; victim = default;

        // Ví dụ (thay tên field đúng với server của bạn):
        // if (ack.hasEnPassant)
        // {
        //     target = new Vector2Int(ack.epTargetR, ack.epTargetC);
        //     victim = new Vector2Int(ack.epVictimR, ack.epVictimC);
        //     return true;
        // }
        return false;
    }

    bool TryInferDoublePushFromAck(BoardDataModel b, ServerMoveResult ack, out Vector2Int target, out Vector2Int victim)
    {
        target = default; victim = default;

        if (!TryReadMovedFromTo(ack, out var fr, out var to)) return false;
        if (!TryReadMovedPieceInfo(ack, out var movedType, out var moverIsRed)) return false;

        if (MapFromPieceType(movedType) != ChessPieceType.Tot) return false;
        if (Math.Abs(to.x - fr.x) != 2) return false; // không phải đi 2 ô

        int midR = (to.x + fr.x) / 2;
        target = new Vector2Int(midR, to.y); // ô mà đối thủ có thể "đi tới" khi bắt qua đường
        victim = new Vector2Int(to.x, to.y); // con tốt vừa đi 2 ô là nạn nhân
        return true;
    }

    bool TryReadPromotionFromAck(ServerMoveResult ack, out Vector2Int atPos, out PieceType newType)
    {
        atPos = default;
        newType = default;

        // Ví dụ:
        // if (ack.hasPromotion)
        // {
        //     atPos = new Vector2Int(ack.promoRow, ack.promoCol);
        //     newType = (PieceType)ack.promoPieceType; // map đúng enum
        //     return true;
        // }
        return false;
    }

    bool TryReadMovedFromTo(ServerMoveResult ack, out Vector2Int from, out Vector2Int to)
    {
        from = default; to = default;

        // Ví dụ:
        // from = new Vector2Int(ack.fromRow, ack.fromCol);
        // to   = new Vector2Int(ack.toRow, ack.toCol);
        // return true;

        return false;
    }

    bool TryReadMovedPieceInfo(ServerMoveResult ack, out PieceType movedType, out bool moverIsRed)
    {
        movedType = default; moverIsRed = default;

        // Ví dụ:
        // movedType = (PieceType)ack.movedPieceType;
        // moverIsRed = ack.moverIsRed;
        // return true;

        return false;
    }
}

