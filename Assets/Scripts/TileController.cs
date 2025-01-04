using UnityEngine;

public class TileController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Color tileBaseColor = Color.white;
    public Color tileHighlightColor = Color.green;
    public Color filledColor = Color.black;

    private bool isGlowing = false; // Tracks if the tile is glowing/active
    private static bool isPaintingMode; // Tracks if the user is painting (true) or erasing (false)
    private static bool hasStartedDrawing = false; // Tracks if the user has started drawing

    public bool IsFilled => spriteRenderer.color == filledColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = tileBaseColor;
    }

    void OnMouseDown()
    {
        // Determine the initial action (painting or erasing)
        if (!hasStartedDrawing)
        {
            isPaintingMode = spriteRenderer.color == tileBaseColor;
            hasStartedDrawing = true;
        }

        // Perform the action based on the initial action
        HandleTileInteraction();
    }

    void OnMouseEnter()
    {
        // Only apply the interaction if the left mouse button is held
        if (Input.GetMouseButton(0))
        {
            HandleTileInteraction();
        }
    }

    void OnMouseUp()
    {
        // Reset the drawing state when the mouse button is released
        hasStartedDrawing = false;
    }

    private void HandleTileInteraction()
    {
        if (isPaintingMode)
        {
            if (spriteRenderer.color == tileBaseColor)
            {
                isGlowing = true;
                spriteRenderer.color = tileHighlightColor;
            }
        }
        else
        {
            if (spriteRenderer.color == tileHighlightColor)
            {
                isGlowing = false;
                spriteRenderer.color = tileBaseColor;
            }
        }
    }

    public void RevertTile()
    {
        isGlowing = false;
        spriteRenderer.color = tileBaseColor;
    }
}
