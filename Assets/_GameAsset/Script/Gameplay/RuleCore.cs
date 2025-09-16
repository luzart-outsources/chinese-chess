using System.Collections.Generic;
using UnityEngine;

public enum GameRules
{
    Xiangqi,
    XiangqiUpsideDown,
    Chess,
    ChessUpsideDown
}

public struct RulesContext
{
    public GameRules code;
    public bool showOnlyLegal; // dùng cho Chess để lọc nước tự chiếu (nếu cần)
}

public interface IRuleSet
{
    GameRules Code { get; }
    int Rows { get; }
    int Cols { get; }

    IEnumerable<Vector2Int> GetValidMoves(BoardDataModel board, PieceModel p);
    bool IsMoveLegal(BoardDataModel board, PieceModel p, Vector2Int to);
    void ApplyPostServer(BoardDataModel board, ServerMoveResult ack);
}

public static class RulesFactory
{
    public static IRuleSet Create(GameRules code)
    {
        return code switch
        {
            GameRules.Xiangqi => new XiangqiRuleSet(false),
            GameRules.XiangqiUpsideDown => new XiangqiRuleSet(true),
            GameRules.Chess => new ChessRuleSet(false),
            GameRules.ChessUpsideDown => new ChessRuleSet(true),
            _ => new XiangqiRuleSet(false),
        };
    }
}
