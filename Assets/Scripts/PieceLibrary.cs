using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An immutable block shape. Cells are offsets from the piece's top-left
/// corner: x = column, y = row (row grows downward, matching the board).
/// </summary>
public class PieceDefinition
{
    public readonly string Name;
    public readonly Vector2Int[] Cells;
    public readonly int Width;
    public readonly int Height;

    public PieceDefinition(string name, Vector2Int[] cells)
    {
        Name = name;
        Cells = cells;
        foreach (var c in cells)
        {
            if (c.x + 1 > Width) Width = c.x + 1;
            if (c.y + 1 > Height) Height = c.y + 1;
        }
    }
}

/// <summary>The standard Block Blast piece set.</summary>
public static class PieceLibrary
{
    public static readonly List<PieceDefinition> Pieces = BuildPieces();

    /// <summary>
    /// Builds a piece from an ASCII picture, e.g. Make("S", "XX.", ".XX").
    /// 'X' marks a cell of the piece.
    /// </summary>
    private static PieceDefinition Make(string name, params string[] rows)
    {
        var cells = new List<Vector2Int>();
        for (int r = 0; r < rows.Length; r++)
            for (int c = 0; c < rows[r].Length; c++)
                if (rows[r][c] == 'X')
                    cells.Add(new Vector2Int(c, r));
        return new PieceDefinition(name, cells.ToArray());
    }

    private static List<PieceDefinition> BuildPieces()
    {
        return new List<PieceDefinition>
        {
            Make("1x1", "X"),

            // Lines
            Make("1x2", "XX"),
            Make("1x3", "XXX"),
            Make("1x4", "XXXX"),
            Make("1x5", "XXXXX"),
            Make("2x1", "X", "X"),
            Make("3x1", "X", "X", "X"),
            Make("4x1", "X", "X", "X", "X"),
            Make("5x1", "X", "X", "X", "X", "X"),

            // Squares & rectangles
            Make("2x2", "XX", "XX"),
            Make("3x3", "XXX", "XXX", "XXX"),
            Make("2x3", "XXX", "XXX"),
            Make("3x2", "XX", "XX", "XX"),

            // Small corners
            Make("Corner NW", "XX", "X."),
            Make("Corner NE", "XX", ".X"),
            Make("Corner SW", "X.", "XX"),
            Make("Corner SE", ".X", "XX"),

            // Big Ls
            Make("L", "X..", "X..", "XXX"),
            Make("L 90", "XXX", "X..", "X.."),
            Make("L 180", "XXX", "..X", "..X"),
            Make("L 270", "..X", "..X", "XXX"),

            // Ts
            Make("T up", ".X.", "XXX"),
            Make("T down", "XXX", ".X."),
            Make("T left", ".X", "XX", ".X"),
            Make("T right", "X.", "XX", "X."),

            // S / Z
            Make("S", ".XX", "XX."),
            Make("Z", "XX.", ".XX"),
            Make("S vert", "X.", "XX", ".X"),
            Make("Z vert", ".X", "XX", "X."),
        };
    }
}
