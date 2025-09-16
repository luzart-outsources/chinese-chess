using System.Collections.Generic;
using UnityEngine;

public sealed class XiangqiRuleSet : IRuleSet
{
    public GameRules Code => _up ? GameRules.XiangqiUpsideDown : GameRules.Xiangqi;
    public int Rows => 10;
    public int Cols => 9;

    private readonly bool _up;
    public XiangqiRuleSet(bool upsideDown) { _up = upsideDown; }

    public IEnumerable<Vector2Int> GetValidMoves(BoardDataModel board, PieceModel p)
    {
        var cfg = _up ? RulesConfig.UpsideDown : RulesConfig.Standard;
        return XiangqiRules.GetValidMoves(board, p, cfg);
    }

    public bool IsMoveLegal(BoardDataModel board, PieceModel p, Vector2Int to)
    {
        // (giữ đơn giản) đã sinh pseudo-legal đủ dùng trong client
        return true;
    }

    public void ApplyPostServer(BoardDataModel board, ServerMoveResult ack)
    {
        // Xiangqi không cần hậu xử lý thêm ở client (bạn đã làm trong BoardController)
    }
}

public static class XiangqiRules
{
    public static List<Vector2Int> GetValidMoves(BoardDataModel board, PieceModel p, RulesConfig cfg)
    {
        var res = new List<Vector2Int>();
        int r = p.row, c = p.col;
        bool red = p.isRed;

        // Loại dùng để tính (Cờ úp: nếu chưa ngửa → xác định theo init pos)
        PieceType typeForMove = p.type;
        if (cfg.mode == GameMode.UpsideDown && !p.isShow)
            typeForMove = InitialTypeResolver.ResolveFromInitial(p.isRed, p.initRow, p.initCol);

        switch (typeForMove)
        {
            case PieceType.Rook: AddRookMoves(board, r, c, red, res); break;
            case PieceType.Cannon: AddCannonMoves(board, r, c, red, res); break;
            case PieceType.Knight: AddKnightMoves(board, r, c, red, res); break;
            case PieceType.Elephant: AddElephantMoves(board, r, c, red, res, cfg); break;
            case PieceType.Advisor: AddAdvisorMoves(board, r, c, red, res, cfg); break;
            case PieceType.General: AddGeneralMoves(board, r, c, red, res); break;
            case PieceType.Soldier: AddSoldierMoves(board, r, c, red, res); break;
        }
        return res;
    }

    static bool InBoard(BoardDataModel b, int r, int c)
        => r >= 0 && r < b.rows && c >= 0 && c < b.cols;

    static void TryAdd(BoardDataModel b, int r, int c, bool myRed, List<Vector2Int> res)
    {
        if (!InBoard(b, r, c)) return;
        var t = b.cells[r, c];
        if (t == null || t.isRed != myRed) res.Add(new Vector2Int(r, c));
    }

    static void AddRookMoves(BoardDataModel b, int r, int c, bool red, List<Vector2Int> res)
    {
        int[] dr = { -1, 1, 0, 0 };
        int[] dc = { 0, 0, -1, 1 };
        for (int k = 0; k < 4; k++)
        {
            int nr = r, nc = c;
            while (true)
            {
                nr += dr[k]; nc += dc[k];
                if (!InBoard(b, nr, nc)) break;
                var t = b.cells[nr, nc];
                if (t != null)
                {
                    if (t.isRed != red) res.Add(new Vector2Int(nr, nc));
                    break;
                }
                res.Add(new Vector2Int(nr, nc));
            }
        }
    }

    static void AddCannonMoves(BoardDataModel b, int r, int c, bool red, List<Vector2Int> res)
    {
        int[] dr = { -1, 1, 0, 0 };
        int[] dc = { 0, 0, -1, 1 };
        for (int k = 0; k < 4; k++)
        {
            int nr = r, nc = c;
            while (true)
            {
                nr += dr[k]; nc += dc[k];
                if (!InBoard(b, nr, nc)) break;
                var t = b.cells[nr, nc];
                if (t != null)
                {
                    int jr = nr + dr[k], jc = nc + dc[k];
                    while (InBoard(b, jr, jc))
                    {
                        var tt = b.cells[jr, jc];
                        if (tt != null)
                        {
                            if (tt.isRed != red) res.Add(new Vector2Int(jr, jc));
                            break;
                        }
                        jr += dr[k]; jc += dc[k];
                    }
                    break;
                }
                res.Add(new Vector2Int(nr, nc));
            }
        }
    }

    static void AddKnightMoves(BoardDataModel b, int r, int c, bool red, List<Vector2Int> res)
    {
        int[,] L = { { -2, -1 }, { -2, 1 }, { 2, -1 }, { 2, 1 }, { -1, -2 }, { 1, -2 }, { -1, 2 }, { 1, 2 } };
        for (int i = 0; i < L.GetLength(0); i++)
        {
            int nr = r + L[i, 0], nc = c + L[i, 1];
            if (!InBoard(b, nr, nc)) continue;
            int br = r + (L[i, 0] / 2), bc = c + (L[i, 1] / 2);
            if (b.cells[br, bc] != null) continue;
            var t = b.cells[nr, nc];
            if (t == null || t.isRed != red) res.Add(new Vector2Int(nr, nc));
        }
    }

    static void AddElephantMoves(BoardDataModel b, int r, int c, bool red, List<Vector2Int> res, RulesConfig cfg)
    {
        int[,] D = { { -2, -2 }, { -2, 2 }, { 2, -2 }, { 2, 2 } };
        for (int i = 0; i < D.GetLength(0); i++)
        {
            int nr = r + D[i, 0], nc = c + D[i, 1];
            if (!InBoard(b, nr, nc)) continue;

            // Cờ thường: không qua sông; Cờ úp: bỏ giới hạn này
            if (cfg.elephantBlockedByRiver)
            {
                if (red && nr < 5) continue;
                if (!red && nr > 4) continue;
            }

            int mr = r + (D[i, 0] / 2), mc = c + (D[i, 1] / 2); // mắt tượng
            if (b.cells[mr, mc] != null) continue;

            var t = b.cells[nr, nc];
            if (t == null || t.isRed != red) res.Add(new Vector2Int(nr, nc));
        }
    }

    static void AddAdvisorMoves(BoardDataModel b, int r, int c, bool red, List<Vector2Int> res, RulesConfig cfg)
    {
        int[,] D = { { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 } };
        if (cfg.advisorRestrictedToPalace)
        {
            int minRow = red ? 7 : 0, maxRow = red ? 9 : 2, minCol = 3, maxCol = 5;
            for (int i = 0; i < D.GetLength(0); i++)
            {
                int nr = r + D[i, 0], nc = c + D[i, 1];
                if (nr < minRow || nr > maxRow || nc < minCol || nc > maxCol) continue;
                var t = b.cells[nr, nc];
                if (t == null || t.isRed != red) res.Add(new Vector2Int(nr, nc));
            }
        }
        else
        {
            for (int i = 0; i < D.GetLength(0); i++)
            {
                int nr = r + D[i, 0], nc = c + D[i, 1];
                TryAdd(b, nr, nc, red, res); // không giới hạn cung
            }
        }
    }

    static void AddGeneralMoves(BoardDataModel b, int r, int c, bool red, List<Vector2Int> res)
    {
        int[] dr = { -1, 1, 0, 0 };
        int[] dc = { 0, 0, -1, 1 };
        int minRow = red ? 7 : 0, maxRow = red ? 9 : 2, minCol = 3, maxCol = 5;

        // tìm tướng đối phương
        PieceModel enemyGen = null;
        foreach (var kv in b.byId)
        {
            var pm = kv.Value;
            if (pm.type == PieceType.General && pm.isRed != red) { enemyGen = pm; break; }
        }

        for (int k = 0; k < 4; k++)
        {
            int nr = r + dr[k], nc = c + dc[k];
            if (nr < minRow || nr > maxRow || nc < minCol || nc > maxCol) continue;
            var t = b.cells[nr, nc];
            if (t != null && t.isRed == red) continue;

            // cấm “tướng đối mặt”
            if (enemyGen != null && nc == enemyGen.col)
            {
                bool blocked = false;
                int step = enemyGen.row > nr ? 1 : -1;
                for (int tr = nr + step; tr != enemyGen.row; tr += step)
                {
                    if (b.cells[tr, nc] != null) { blocked = true; break; }
                }
                if (!blocked) continue;
            }
            res.Add(new Vector2Int(nr, nc));
        }
    }

    static void AddSoldierMoves(BoardDataModel b, int r, int c, bool red, List<Vector2Int> res)
    {
        // Chuẩn: row=0 phía TRÊN → ĐỎ đi xuống (tăng row), ĐEN đi lên (giảm row)
        int forward = red ? +1 : -1;
        TryAdd(b, r + forward, c, red, res);

        bool crossedRiver = red ? (r >= 5) : (r <= 4);
        if (crossedRiver)
        {
            TryAdd(b, r, c - 1, red, res);
            TryAdd(b, r, c + 1, red, res);
        }
    }
}
