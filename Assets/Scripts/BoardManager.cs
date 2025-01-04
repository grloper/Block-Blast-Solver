using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject board; // Parent object of all tiles

   public void ResetBoard()
{
    Debug.Log("Reset function triggered.");
    foreach (Transform tile in transform)
    {
        var tileController = tile.GetComponent<TileController>();
        if (tileController != null)
        {
            tileController.RevertTile();
        }
    }
}
public bool[,] GetBoardState()
{
    bool[,] boardState = new bool[8, 8];
    int index = 0;

    foreach (Transform tile in board.transform)
    {
        TileController tileController = tile.GetComponent<TileController>();
        if (tileController != null)
        {
            int row = index / 8;
            int col = index % 8;
            boardState[row, col] = tileController.IsFilled;
            index++;
        }
    }

    return boardState;
}

}
