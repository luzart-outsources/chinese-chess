using System.Collections.Generic;
using UnityEngine;

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


public enum GameMode { Standard, UpsideDown }

public struct RulesConfig
{
    public GameMode mode;
    public bool advisorRestrictedToPalace;  // Sĩ bị giới hạn cung?
    public bool elephantBlockedByRiver;     // Tượng bị giới hạn sông?

    public static RulesConfig Standard => new RulesConfig
    {
        mode = GameMode.Standard,
        advisorRestrictedToPalace = true,
        elephantBlockedByRiver = true
    };

    public static RulesConfig UpsideDown => new RulesConfig
    {
        mode = GameMode.UpsideDown,
        advisorRestrictedToPalace = false,  // Sĩ đi khắp bàn
        elephantBlockedByRiver = false      // Tượng đi khắp bàn (vẫn chặn “mắt tượng”)
    };
}

/// <summary>
/// Xác định loại quân theo VỊ TRÍ BAN ĐẦU (layout chuẩn cờ tướng)
/// </summary>
public static class InitialTypeResolver
{
    // Bản đồ chuẩn: hàng 0 (đen trên), hàng 9 (đỏ dưới)
    // R: Rook, N: Knight, E: Elephant, A: Advisor, G: General, C: Cannon, S: Soldier
    private static readonly PieceType[,] redInit = new PieceType[10, 9];
    private static readonly PieceType[,] blackInit = new PieceType[10, 9];

    static InitialTypeResolver()
    {
        // Clear
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 9; c++)
            { redInit[r, c] = PieceType.None; blackInit[r, c] = PieceType.None; }

        // BLACK (top)
        // row 0: R N E A G A E N R
        var topRow = new[] { PieceType.Rook, PieceType.Knight, PieceType.Elephant, PieceType.Advisor, PieceType.General, PieceType.Advisor, PieceType.Elephant, PieceType.Knight, PieceType.Rook };
        for (int c = 0; c < 9; c++) blackInit[0, c] = topRow[c];
        // row 2: cannons at 1,7
        blackInit[2, 1] = PieceType.Cannon; blackInit[2, 7] = PieceType.Cannon;
        // row 3: soldiers at 0,2,4,6,8
        for (int c = 0; c < 9; c += 2) blackInit[3, c] = PieceType.Soldier;

        // RED (bottom)
        // row 9: R N E A G A E N R
        for (int c = 0; c < 9; c++) redInit[9, c] = topRow[c];
        // row 7: cannons at 1,7
        redInit[7, 1] = PieceType.Cannon; redInit[7, 7] = PieceType.Cannon;
        // row 6: soldiers at 0,2,4,6,8
        for (int c = 0; c < 9; c += 2) redInit[6, c] = PieceType.Soldier;
    }

    public static PieceType ResolveFromInitial(bool isRed, int initRow, int initCol)
    {
        var t = isRed ? redInit[initRow, initCol] : blackInit[initRow, initCol];
        return t == PieceType.None ? PieceType.Soldier : t; // fallback an toàn
    }
}

public class PieceModel
{
    public int id;
    public PieceType type;       // loại “đang sở hữu” (sau khi ngửa sẽ cố định)
    public bool isRed;
    public bool isShow;          // đang úp hay đã ngửa

    public int row, col;         // tọa độ hiện tại (theo server)
    public int initRow, initCol; // vị trí ban đầu để xác định loại khi úp-ngửa lần đầu

    public PieceModel(int id, PieceType type, bool isRed, bool isShow, int row, int col)
    {
        this.id = id; this.type = type; this.isRed = isRed; this.isShow = isShow;
        this.row = row; this.col = col; this.initRow = row; this.initCol = col;
    }
}

public class BoardDataModel
{
    public readonly int rows = 10;
    public readonly int cols = 9;
    public PieceModel[,] cells;
    public readonly Dictionary<int, PieceModel> byId = new();

    public BoardDataModel() { cells = new PieceModel[rows, cols]; }

    public void Clear()
    {
        cells = new PieceModel[rows, cols];
        byId.Clear();
    }

    public void Place(PieceModel p, int r, int c)
    {
        cells[r, c] = p;
        p.row = r; p.col = c;
        byId[p.id] = p;
    }

    public void RemoveAt(int r, int c)
    {
        var p = cells[r, c];
        if (p != null) byId.Remove(p.id);
        cells[r, c] = null;
    }

    public void MoveTo(PieceModel p, int newR, int newC)
    {
        cells[p.row, p.col] = null;
        var target = cells[newR, newC];
        if (target != null) byId.Remove(target.id); // bắt quân
        cells[newR, newC] = p;
        p.row = newR; p.col = newC;
    }

    public PieceModel GetAt(int r, int c) => cells[r, c];
    public PieceModel GetById(int id) => byId.TryGetValue(id, out var p) ? p : null;
}


public static class BoardIndexHelper
{
    /// <summary>
    /// Chuyển (row, col) từ server sang index trong List<Transform> (bố cục: bottom-to-top, left-to-right).
    /// rows=10, cols=9 cho cờ tướng.
    /// </summary>
    public static int ServerRC_To_ListIndex(int serverRow, int serverCol,
                                            int rows = 10, int cols = 9,
                                            bool serverRowZeroAtTop = true)
    {
        // Hàng trong List<Transform> đang xếp "từ dưới lên" → ta cần đổi row server sang "row tính từ dưới"
        int rowFromBottom = serverRowZeroAtTop ? (rows - 1 - serverRow) : serverRow;
        return rowFromBottom * cols + serverCol; // trái → phải trong từng hàng
    }

    /// <summary>
    /// Ngược lại: lấy index trong List<Transform> → (row,col) theo tọa độ server.
    /// </summary>
    public static (int row, int col) ListIndex_To_ServerRC(int listIndex,
                                                           int rows = 10, int cols = 9,
                                                           bool serverRowZeroAtTop = true)
    {
        int rowFromBottom = listIndex / cols;
        int col = listIndex % cols;
        int serverRow = serverRowZeroAtTop ? (rows - 1 - rowFromBottom) : rowFromBottom;
        return (serverRow, col);
    }

    /// <summary>
    /// Kiểm tra hợp lệ (row,col).
    /// </summary>
    public static bool InBoard(int r, int c, int rows = 10, int cols = 9)
        => r >= 0 && r < rows && c >= 0 && c < cols;

    /// <summary>
    /// Map (row,col) SERVER → index trong List<Transform> cells (dưới→trên, trái→phải),
    /// với tùy chọn xoay bàn để "quân của tôi ở dưới".
    /// - rows=10, cols=9. 
    /// - serverRowZeroAtTop=true: row=0 là hàng trên cùng theo chuẩn cờ tướng.
    /// - mySideAtBottom=true: luôn quay bàn để quân của tôi nằm dưới.
    /// - iAmRed: màu của tôi do server trả.
    /// </summary>
    public static int ServerRC_To_ListIndex_Perspective(
        int serverRow, int serverCol,
        int rows, int cols,
        bool serverRowZeroAtTop,
        bool mySideAtBottom,
        bool iAmRed)
    {
        // B1: từ toạ độ server → toạ độ "view" (nếu tôi là Đen và muốn quân mình ở dưới → xoay 180°)
        int viewRow = serverRow;
        int viewCol = serverCol;

        if (mySideAtBottom && !iAmRed)
        {
            viewRow = rows - 1 - serverRow;
            viewCol = cols - 1 - serverCol;
        }
        // Nếu tôi là Đỏ, không cần xoay (đã ở dưới sẵn theo layout chuẩn).

        // B2: viewRow hiện đang tính theo "row 0 = TRÊN của khung nhìn".
        // List<Transform> cells lại xếp từ DƯỚI→TRÊN, TRÁI→PHẢI.
        int rowFromBottom = (serverRowZeroAtTop ? (rows - 1 - viewRow) : viewRow);
        return rowFromBottom * cols + viewCol;
    }

    public static (int row, int col) ListIndex_To_ServerRC_Perspective(
        int listIndex,
        int rows, int cols,
        bool serverRowZeroAtTop,
        bool mySideAtBottom,
        bool iAmRed)
    {
        int rowFromBottom = listIndex / cols;
        int viewCol = listIndex % cols;
        int viewRow = serverRowZeroAtTop ? (rows - 1 - rowFromBottom) : rowFromBottom;

        // Ngược xoay: nếu tôi là Đen và đang dùng mySideAtBottom, undo xoay 180° để ra toạ độ server
        int serverRow = viewRow, serverCol = viewCol;
        if (mySideAtBottom && !iAmRed)
        {
            serverRow = rows - 1 - viewRow;
            serverCol = cols - 1 - viewCol;
        }
        return (serverRow, serverCol);
    }

}
public enum PieceType
{
    None = -1,
    Rook,       // Xe
    Knight,     // Mã
    Cannon,     // Pháo
    Elephant,   // Tượng
    Advisor,    // Sĩ
    General,    // Tướng/Soái
    Soldier     // Tốt/Binh
}

public class PieceDTO
{
    public int id;
    public PieceType type;
    public bool isRed;
    public bool isShow;
}

// Khi gọi InitializeFromServer, server kèm:
public struct InitPayload
{
    public PieceDTO[,] grid10x9;
    public bool iAmRed;    // <- tôi là Đỏ (true) hay Đen (false)
    public bool myTurn;    // <- có phải tới lượt tôi không
}

