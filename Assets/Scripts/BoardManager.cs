using UnityEngine;

/// <summary>
/// Owns the 8x8 tile grid: exposes the board state to the solver,
/// paints solution previews, and resets the board.
/// </summary>
public class BoardManager : MonoBehaviour
{
    public GameObject board; // Parent object of all tiles

    public const int Size = BlockBlastSolver.Size;

    private TileController[,] tiles;

    public void ResetBoard()
    {
        EnsureTiles();
        if (tiles == null) return;
        foreach (var tile in tiles)
            if (tile != null) tile.RevertTile();
    }

    public void ClearPreviews()
    {
        EnsureTiles();
        if (tiles == null) return;
        foreach (var tile in tiles)
            if (tile != null) tile.ClearPreview();
    }

    public bool[,] GetBoardState()
    {
        EnsureTiles();
        var state = new bool[Size, Size];
        if (tiles == null) return state;
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                state[r, c] = tiles[r, c] != null && tiles[r, c].IsFilled;
        return state;
    }

    public TileController GetTile(int row, int col)
    {
        EnsureTiles();
        return tiles == null ? null : tiles[row, col];
    }

    /// <summary>
    /// Tiles are spawned by BoardSetup in row-major order (row 0 first),
    /// so child order maps directly onto grid coordinates.
    /// </summary>
    private void EnsureTiles()
    {
        if (tiles != null) return;
        var grid = new TileController[Size, Size];
        int index = 0;
        var parent = board != null ? board.transform : transform;
        foreach (Transform child in parent)
        {
            var tile = child.GetComponent<TileController>();
            if (tile == null) continue;
            grid[index / Size, index % Size] = tile;
            index++;
            if (index >= Size * Size) break;
        }
        // BoardSetup may not have spawned the tiles yet; retry on next call.
        if (index == Size * Size) tiles = grid;
    }
}
