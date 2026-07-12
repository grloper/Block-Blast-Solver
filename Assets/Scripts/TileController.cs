using UnityEngine;

/// <summary>
/// A single cell of the 8x8 board. Click/drag to mark cells as blocked
/// (mirroring your in-game board). The solver paints placement previews on top.
/// </summary>
public class TileController : MonoBehaviour
{
    public Color tileBaseColor = Color.white;
    public Color filledColor = new Color(0.29f, 0.72f, 0.37f); // blocked cell (green)

    private SpriteRenderer spriteRenderer;
    private bool isFilled;
    private bool hasPreview;
    private Color previewColor;

    private static bool isPaintingMode;      // true = painting, false = erasing
    private static bool hasStartedDrawing;   // a drag gesture is in progress

    public bool IsFilled => isFilled;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Refresh();
    }

    void OnMouseDown()
    {
        if (!hasStartedDrawing)
        {
            // First tile of the gesture decides whether we paint or erase.
            isPaintingMode = !isFilled;
            hasStartedDrawing = true;
        }
        HandleTileInteraction();
    }

    void OnMouseEnter()
    {
        if (Input.GetMouseButton(0) && hasStartedDrawing)
        {
            HandleTileInteraction();
        }
    }

    void OnMouseUp()
    {
        hasStartedDrawing = false;
    }

    private void HandleTileInteraction()
    {
        SetFilled(isPaintingMode);
    }

    public void SetFilled(bool filled)
    {
        isFilled = filled;
        hasPreview = false; // editing a cell invalidates any solver preview on it
        Refresh();
    }

    public void SetPreview(Color color)
    {
        hasPreview = true;
        previewColor = color;
        Refresh();
    }

    public void ClearPreview()
    {
        hasPreview = false;
        Refresh();
    }

    /// <summary>Empties the cell and removes any preview.</summary>
    public void RevertTile()
    {
        isFilled = false;
        hasPreview = false;
        Refresh();
    }

    private void Refresh()
    {
        if (spriteRenderer == null) return;
        if (hasPreview) spriteRenderer.color = previewColor;
        else spriteRenderer.color = isFilled ? filledColor : tileBaseColor;
    }
}
