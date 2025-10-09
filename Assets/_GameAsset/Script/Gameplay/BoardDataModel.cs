using System.Collections.Generic;
[System.Serializable]
public class BoardDataModel
{
    public readonly int rows;
    public readonly int cols;
    public PieceModel[,] cells;
    [Sirenix.OdinInspector.ShowInInspector]
    public readonly Dictionary<int, PieceModel> byId = new();

    public BoardDataModel(int rows = 10, int cols = 9)
    {
        this.rows = rows;
        this.cols = cols;
        cells = new PieceModel[rows, cols];
    }

    public void Clear()
    {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                cells[r, c] = null;
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
        if (target != null) byId.Remove(target.id); // capture
        cells[newR, newC] = p;
        p.row = newR; p.col = newC;
    }

    public PieceModel GetAt(int r, int c) => cells[r, c];
    public PieceModel GetById(int id) => byId.TryGetValue(id, out var p) ? p : null;
}
