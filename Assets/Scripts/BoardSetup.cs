using UnityEngine;

public class BoardSetup : MonoBehaviour
{
    public GameObject cellPrefab; // Prefab for each cell
    public int rows = 8;
    public int cols = 8;
    public float cellSize = 1f;

    void Start()
    {
        GenerateBoard();
    }

    // Generate the game board
    void GenerateBoard()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 position = new Vector3(col * cellSize, row * -cellSize, 0);
                Instantiate(cellPrefab, position, Quaternion.identity, transform);
            }
        }
    }

}
