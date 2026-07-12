using System.Collections.Generic;

/// <summary>
/// Pure C# search for the best way to place up to three pieces on an 8x8
/// Block Blast board. Tries every ordering and position, simulates the line
/// clears after each placement, and keeps the highest-scoring sequence.
/// </summary>
public static class BlockBlastSolver
{
    public const int Size = 8;

    public struct Placement
    {
        public PieceDefinition Piece;
        public int Row;
        public int Col;
        public int LinesCleared; // lines cleared right after this placement
    }

    public class Solution
    {
        public List<Placement> Placements = new List<Placement>();
        public int TotalLinesCleared;
        public int Score;
    }

    /// <summary>
    /// Returns the best sequence found, or null if no piece can be placed.
    /// If not all pieces fit, the solution placing the most pieces wins.
    /// </summary>
    public static Solution Solve(bool[,] board, IList<PieceDefinition> pieces)
    {
        var best = new Solution { Score = int.MinValue };
        var used = new bool[pieces.Count];
        var current = new List<Placement>();
        Search(Copy(board), pieces, used, current, 0, 0, best);
        return best.Placements.Count > 0 ? best : null;
    }

    private static void Search(bool[,] board, IList<PieceDefinition> pieces, bool[] used,
        List<Placement> current, int score, int linesSoFar, Solution best)
    {
        bool placedAny = false;

        for (int i = 0; i < pieces.Count; i++)
        {
            if (used[i]) continue;
            // Identical pieces are interchangeable; only try the first unused one.
            bool duplicate = false;
            for (int j = 0; j < i; j++)
                if (!used[j] && pieces[j] == pieces[i]) { duplicate = true; break; }
            if (duplicate) continue;

            var piece = pieces[i];
            for (int r = 0; r + piece.Height <= Size; r++)
            {
                for (int c = 0; c + piece.Width <= Size; c++)
                {
                    if (!Fits(board, piece, r, c)) continue;
                    placedAny = true;

                    var next = Copy(board);
                    Place(next, piece, r, c, true);
                    int cleared = ClearFullLines(next);

                    // Block Blast style scoring: points per cell, big bonus
                    // per line, extra combo bonus for multi-line clears.
                    int stepScore = piece.Cells.Length + cleared * 10 + (cleared > 1 ? (cleared - 1) * 20 : 0);

                    used[i] = true;
                    current.Add(new Placement { Piece = piece, Row = r, Col = c, LinesCleared = cleared });
                    Search(next, pieces, used, current, score + stepScore, linesSoFar + cleared, best);
                    current.RemoveAt(current.Count - 1);
                    used[i] = false;
                }
            }
        }

        if (!placedAny)
        {
            // Leaf: no remaining piece fits (or all pieces are placed).
            if (IsBetter(current.Count, score, best))
            {
                best.Placements = new List<Placement>(current);
                best.Score = score;
                best.TotalLinesCleared = linesSoFar;
            }
        }
    }

    private static bool IsBetter(int placedCount, int score, Solution best)
    {
        if (placedCount != best.Placements.Count) return placedCount > best.Placements.Count;
        return score > best.Score;
    }

    public static bool Fits(bool[,] board, PieceDefinition piece, int row, int col)
    {
        foreach (var cell in piece.Cells)
            if (board[row + cell.y, col + cell.x])
                return false;
        return true;
    }

    public static void Place(bool[,] board, PieceDefinition piece, int row, int col, bool value)
    {
        foreach (var cell in piece.Cells)
            board[row + cell.y, col + cell.x] = value;
    }

    /// <summary>Clears every full row and column simultaneously; returns how many lines were cleared.</summary>
    public static int ClearFullLines(bool[,] board)
    {
        var fullRows = new List<int>();
        var fullCols = new List<int>();

        for (int r = 0; r < Size; r++)
        {
            bool full = true;
            for (int c = 0; c < Size; c++) if (!board[r, c]) { full = false; break; }
            if (full) fullRows.Add(r);
        }
        for (int c = 0; c < Size; c++)
        {
            bool full = true;
            for (int r = 0; r < Size; r++) if (!board[r, c]) { full = false; break; }
            if (full) fullCols.Add(c);
        }

        foreach (int r in fullRows)
            for (int c = 0; c < Size; c++) board[r, c] = false;
        foreach (int c in fullCols)
            for (int r = 0; r < Size; r++) board[r, c] = false;

        return fullRows.Count + fullCols.Count;
    }

    private static bool[,] Copy(bool[,] board)
    {
        var copy = new bool[Size, Size];
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                copy[r, c] = board[r, c];
        return copy;
    }
}
