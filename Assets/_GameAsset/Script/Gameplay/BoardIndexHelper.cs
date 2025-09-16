using System.Collections.Generic;
using UnityEngine;

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
    Rook = 0,       // Xe
    Knight = 1,     // Mã
    Cannon = 2,     // Pháo
    Elephant = 3,   // Tượng
    Advisor = 4,    // Sĩ
    General = 5,    // Tướng/Soái
    Soldier = 6    // Tốt/Binh
}

public class PieceDTO
{
    public int id;
    public PieceType type;  // Với Chess: map tạm (Soldier->Pawn, Knight->Knight, Elephant->Bishop, Rook->Rook, Advisor->Queen, General->King)
    public bool isRed;      // Dùng như White=true khi chơi Chess
    public bool isShow;
}

public struct InitPayload
{
    public PieceDTO[][] grid;  // <--- răng cưa
    public bool iAmRed;
    public bool myTurn;
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


