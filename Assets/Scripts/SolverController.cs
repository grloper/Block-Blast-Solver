using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the solver UI at runtime (piece palette, hand slots, action buttons)
/// and connects it to the board + solver. Everything is created in code so the
/// scene only needs this component on an empty GameObject.
/// </summary>
public class SolverController : MonoBehaviour
{
    public BoardManager boardManager;

    // Colors shared with the board tiles / README demo.
    public static readonly Color PanelColor = new Color(0.10f, 0.14f, 0.23f, 0.96f);
    public static readonly Color ButtonColor = new Color(0.17f, 0.24f, 0.38f);
    public static readonly Color AccentColor = new Color(0.23f, 0.51f, 0.96f);
    public static readonly Color[] PlacementColors =
    {
        new Color(0.23f, 0.51f, 0.96f), // 1st piece - blue
        new Color(0.96f, 0.62f, 0.04f), // 2nd piece - orange
        new Color(0.66f, 0.33f, 0.97f), // 3rd piece - purple
    };

    private const int MaxHand = 3;
    private const float PanelWidth = 420f;

    private readonly List<PieceDefinition> hand = new List<PieceDefinition>();
    private readonly List<Image> slotBackgrounds = new List<Image>();
    private readonly List<RectTransform> slotShapeAreas = new List<RectTransform>();
    private Text statusText;
    private Font font;

    void Start()
    {
        if (boardManager == null) boardManager = FindFirstObjectByType<BoardManager>();
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUi();
        FrameCamera();
    }

    // ------------------------------------------------------------------ actions

    public void Solve()
    {
        boardManager.ClearPreviews();
        if (hand.Count == 0)
        {
            statusText.text = "Pick up to 3 pieces first.";
            return;
        }

        var solution = BlockBlastSolver.Solve(boardManager.GetBoardState(), hand);
        if (solution == null)
        {
            statusText.text = "No valid placement found.";
            return;
        }

        for (int i = 0; i < solution.Placements.Count; i++)
        {
            var p = solution.Placements[i];
            var color = PlacementColors[i % PlacementColors.Length];
            foreach (var cell in p.Piece.Cells)
            {
                var tile = boardManager.GetTile(p.Row + cell.y, p.Col + cell.x);
                if (tile != null) tile.SetPreview(color);
            }
        }

        statusText.text = string.Format("Placed {0}/{1} pieces • {2} line{3} cleared • score {4}",
            solution.Placements.Count, hand.Count, solution.TotalLinesCleared,
            solution.TotalLinesCleared == 1 ? "" : "s", solution.Score);
    }

    public void ClearSolution()
    {
        boardManager.ClearPreviews();
        statusText.text = "Solution cleared.";
    }

    public void ResetAll()
    {
        boardManager.ResetBoard();
        hand.Clear();
        RefreshSlots();
        statusText.text = "Board reset. Draw your blocked cells.";
    }

    private void AddToHand(PieceDefinition piece)
    {
        if (hand.Count >= MaxHand) return;
        hand.Add(piece);
        RefreshSlots();
    }

    private void RemoveFromHand(int slot)
    {
        if (slot < hand.Count) hand.RemoveAt(slot);
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < MaxHand; i++)
        {
            foreach (Transform child in slotShapeAreas[i]) Destroy(child.gameObject);
            bool used = i < hand.Count;
            slotBackgrounds[i].color = used
                ? Color.Lerp(ButtonColor, PlacementColors[i], 0.25f)
                : new Color(1f, 1f, 1f, 0.06f);
            if (used) DrawPieceShape(slotShapeAreas[i], hand[i], 72f, PlacementColors[i]);
        }
    }

    // ------------------------------------------------------------------ UI build

    private void BuildUi()
    {
        var canvasGo = new GameObject("SolverCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 800);
        scaler.matchWidthOrHeight = 0.5f;

        // Right-hand panel
        var panel = CreateRect("Panel", canvasGo.transform);
        var panelImg = panel.gameObject.AddComponent<Image>();
        panelImg.color = PanelColor;
        panel.anchorMin = new Vector2(1, 0);
        panel.anchorMax = new Vector2(1, 1);
        panel.pivot = new Vector2(1, 0.5f);
        panel.sizeDelta = new Vector2(PanelWidth, 0);
        panel.anchoredPosition = Vector2.zero;

        float y = -28f;
        CreateText(panel, "BLOCK BLAST SOLVER", 26, FontStyle.Bold, new Vector2(0, y), 36, TextAnchor.MiddleCenter, Color.white);
        y -= 44f;
        CreateText(panel, "1. Paint your board   2. Pick your 3 pieces   3. Solve",
            13, FontStyle.Normal, new Vector2(0, y), 22, TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.65f));
        y -= 36f;

        // Hand slots
        float slotSize = 86f, slotGap = 14f;
        float slotsWidth = MaxHand * slotSize + (MaxHand - 1) * slotGap;
        for (int i = 0; i < MaxHand; i++)
        {
            int slot = i;
            var rt = CreateRect("Slot" + i, panel);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(slotSize, slotSize);
            rt.anchoredPosition = new Vector2(-slotsWidth / 2 + slotSize / 2 + i * (slotSize + slotGap), y);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.06f);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => RemoveFromHand(slot));
            slotBackgrounds.Add(img);

            var shape = CreateRect("Shape", rt);
            shape.anchorMin = Vector2.zero;
            shape.anchorMax = Vector2.one;
            shape.sizeDelta = Vector2.zero;
            slotShapeAreas.Add(shape);
        }
        y -= slotSize + 26f;

        CreateText(panel, "PIECES", 13, FontStyle.Bold, new Vector2(0, y), 20, TextAnchor.MiddleLeft, new Color(1, 1, 1, 0.5f), 24);
        y -= 26f;

        // Palette grid
        int perRow = 6;
        float cell = 58f, gap = 8f;
        var pieces = PieceLibrary.Pieces;
        for (int i = 0; i < pieces.Count; i++)
        {
            var piece = pieces[i];
            int row = i / perRow, col = i % perRow;
            var rt = CreateRect("Piece_" + piece.Name, panel);
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(cell, cell);
            rt.anchoredPosition = new Vector2(24 + col * (cell + gap), y - row * (cell + gap));
            var img = rt.gameObject.AddComponent<Image>();
            img.color = ButtonColor;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => AddToHand(piece));
            DrawPieceShape(rt, piece, cell - 12f, new Color(0.55f, 0.75f, 1f));
        }
        int rowsUsed = (pieces.Count + perRow - 1) / perRow;
        y -= rowsUsed * (cell + gap) + 16f;

        // Action buttons
        CreateButton(panel, "SOLVE", AccentColor, new Vector2(0, y), new Vector2(-48, 46), Solve);
        y -= 58f;
        CreateButton(panel, "CLEAR SOLUTION", ButtonColor, new Vector2(-98, y), new Vector2(180, 40), ClearSolution);
        CreateButton(panel, "RESET BOARD", ButtonColor, new Vector2(98, y), new Vector2(180, 40), ResetAll);
        y -= 52f;

        statusText = CreateText(panel, "Draw your blocked cells, then pick pieces.",
            14, FontStyle.Normal, new Vector2(0, y), 40, TextAnchor.UpperCenter, new Color(1, 1, 1, 0.8f));
    }

    /// <summary>Draws a mini version of a piece centered inside <paramref name="area"/>.</summary>
    private void DrawPieceShape(RectTransform area, PieceDefinition piece, float areaSize, Color color)
    {
        int span = Mathf.Max(piece.Width, piece.Height);
        float unit = areaSize / Mathf.Max(span, 3); // don't oversize tiny pieces
        float w = piece.Width * unit, h = piece.Height * unit;
        foreach (var cellPos in piece.Cells)
        {
            var rt = CreateRect("c", area);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(unit - 2f, unit - 2f);
            rt.anchoredPosition = new Vector2(
                -w / 2 + cellPos.x * unit + unit / 2,
                h / 2 - cellPos.y * unit - unit / 2);
            rt.gameObject.AddComponent<Image>().color = color;
        }
    }

    private RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private Text CreateText(RectTransform parent, string content, int size, FontStyle style,
        Vector2 topOffset, float height, TextAnchor anchor, Color color, float sideMargin = 12)
    {
        var rt = CreateRect("Text", parent);
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.offsetMin = new Vector2(sideMargin, 0);
        rt.offsetMax = new Vector2(-sideMargin, 0);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        rt.anchoredPosition = topOffset;
        var text = rt.gameObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = anchor;
        text.color = color;
        text.text = content;
        return text;
    }

    private void CreateButton(RectTransform parent, string label, Color color,
        Vector2 topOffset, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        var rt = CreateRect(label, parent);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        // size.x <= 0 means "stretch with margin of -size.x per side"
        if (size.x <= 0)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(-size.x / 2 + 24, 0);
            rt.offsetMax = new Vector2(size.x / 2 - 24, 0);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, size.y);
        }
        else
        {
            rt.sizeDelta = size;
        }
        rt.anchoredPosition = topOffset;
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);
        var text = CreateText(rt, label, 15, FontStyle.Bold, Vector2.zero, size.y, TextAnchor.MiddleCenter, Color.white);
        var textRt = (RectTransform)text.transform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;
    }

    // ------------------------------------------------------------------ camera

    /// <summary>Centers the board in the space left of the UI panel.</summary>
    private void FrameCamera()
    {
        var cam = Camera.main;
        var setup = FindFirstObjectByType<BoardSetup>();
        if (cam == null || setup == null) return;

        float cs = setup.cellSize;
        var origin = setup.transform.position;
        var center = new Vector2(origin.x + (setup.cols - 1) * cs / 2f,
                                 origin.y - (setup.rows - 1) * cs / 2f);

        cam.orthographic = true;
        cam.orthographicSize = setup.rows * cs / 2f + 1.1f;

        float panelFraction = Mathf.Clamp01(PanelWidth * GetCanvasScale() / Screen.width);
        float halfWidth = cam.orthographicSize * cam.aspect;
        // Shift the camera right so the board centers in the area the panel doesn't cover.
        float shift = panelFraction * halfWidth;
        cam.transform.position = new Vector3(center.x + shift, center.y, -10f);
    }

    private float GetCanvasScale()
    {
        // Mirrors CanvasScaler.ScaleWithScreenSize with matchWidthOrHeight = 0.5.
        float logWidth = Mathf.Log(Screen.width / 1280f, 2);
        float logHeight = Mathf.Log(Screen.height / 800f, 2);
        return Mathf.Pow(2, Mathf.Lerp(logWidth, logHeight, 0.5f));
    }
}
