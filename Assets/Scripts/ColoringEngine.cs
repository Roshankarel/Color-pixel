using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;


public class ColoringEngine : MonoBehaviour
{
    public static ColoringEngine Instance;

    [Header("Settings")]
    [Range(0, 30)]
    public int tolerance = 15;

    [Header("Paint By Number")]
    public TextAsset paintPaletteJson;
    public Texture2D regionIdMap;
    public bool allowRecolorCorrectRegions = false;

    [Header("References")]
    public Camera mainCamera;
    public SpriteRenderer DrawingRenderer;
    
    public event Action DrawingCompleted;

    private Texture2D _texture;
    private Texture2D _workingTexture;
    private PaintByNumberPaletteData _paletteData;
    private int _selectedNumber = -1;
    private Color _selectedColor = Color.red;
    private bool _regionIdMapReadable = true;
    
    private HashSet<int> paintedRegions = new HashSet<int>();

    public DrawingData CurrentDrawing { get; private set; }

    private int totalRegions;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadDrawingData();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (DrawingRenderer == null)
        {
            Debug.LogError("ColoringEngine: No SpriteRenderer assigned!");
            return;
        }

        Sprite sourceSprite = DrawingRenderer.sprite;
        Texture2D original = sourceSprite.texture;
        _workingTexture = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
        _workingTexture.SetPixels(original.GetPixels());
        _workingTexture.Apply();

        DrawingRenderer.sprite = Sprite.Create(
            _workingTexture,
            sourceSprite.rect,
            new Vector2(sourceSprite.pivot.x / sourceSprite.rect.width, sourceSprite.pivot.y / sourceSprite.rect.height),
            sourceSprite.pixelsPerUnit
        );

        _texture = _workingTexture;
        LoadPaintByNumberData();
        ValidateRegionIdMap();
        
        ColorPaletteManager paletteManager = FindFirstObjectByType<ColorPaletteManager>();
        if (paletteManager != null)
            paletteManager.RefreshPalette();
    }

    void Update()
    {
        if (CameraPanZoom.Instance == null)
            return;

        if (CameraPanZoom.Instance.WasTapThisFrame)
        {
            HandleInput(CameraPanZoom.Instance.TapScreenPosition);
        }
    }

    void HandleInput(Vector2 screenPos, int pointerId = -1)
    {
        if (_texture == null || mainCamera == null)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId))
            return;

        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        Vector2Int pixelPos = WorldToPixel(worldPos);

        if (!IsInsideTexture(pixelPos))
            return;

        Color tappedColor = _texture.GetPixel(pixelPos.x, pixelPos.y);
        if (IsOutlinePixel(tappedColor))
            return;

        if (!TryGetRequiredPaint(pixelPos, out int requiredNumber, out Color requiredColor))
            return;

        if (_selectedNumber != requiredNumber)
        {
            Debug.Log("Wrong paint number. Needed: " + requiredNumber + ", selected: " + _selectedNumber);
            return;
        }

        if (!allowRecolorCorrectRegions && ColorsMatch(tappedColor, requiredColor, 2))
            return;

        int regionId = GetRegionId(pixelPos);

        FloodFill(pixelPos, tappedColor, requiredColor);

        MarkRegionCompleted(regionId);
    }

    void LoadPaintByNumberData()
    {
        _paletteData = PaintByNumberPaletteData.FromJson(paintPaletteJson);

        paintedRegions.Clear();

        totalRegions = _paletteData.regions.Length;

        Debug.Log($"Total Regions: {totalRegions}");

        if (_paletteData == null)
            Debug.LogError("ColoringEngine: Assign a paint_palette JSON TextAsset.");
    }

    void ValidateRegionIdMap()
    {
        if (regionIdMap == null)
        {
            Debug.LogError("ColoringEngine: Assign a region_id_map texture.");
            return;
        }

        try
        {
            regionIdMap.GetPixel(0, 0);
            _regionIdMapReadable = true;
        }
        catch (UnityException)
        {
            _regionIdMapReadable = false;
            Debug.LogError("ColoringEngine: Region ID map must have Read/Write enabled in import settings.");
        }

        if (_texture != null && (regionIdMap.width != _texture.width || regionIdMap.height != _texture.height))
        {
            Debug.LogWarning("ColoringEngine: Region ID map size does not match the visible texture. IDs will be sampled by proportional coordinates.");
        }
    }

    bool TryGetRequiredPaint(Vector2Int workingPixel, out int requiredNumber, out Color requiredColor)
    {
        requiredNumber = -1;
        requiredColor = Color.clear;

        if (_paletteData == null || regionIdMap == null || !_regionIdMapReadable)
            return false;

        int regionId = GetRegionId(workingPixel);
        if (regionId <= 0)
            return false;

        if (!_paletteData.TryGetRegion(regionId, out PaintByNumberRegionEntry regionEntry))
        {
            Debug.LogWarning("ColoringEngine: Region ID " + regionId + " is missing from paint_palette JSON.");
            return false;
        }

        if (!_paletteData.TryGetUnityColor(regionEntry.colorNumber, out requiredColor))
        {
            Debug.LogWarning("ColoringEngine: Color number " + regionEntry.colorNumber + " is missing or invalid in paint_palette JSON.");
            return false;
        }

        requiredNumber = regionEntry.colorNumber;
        return true;
    }

    int GetRegionId(Vector2Int workingPixel)
    {
        int x = workingPixel.x;
        int y = workingPixel.y;

        if (regionIdMap.width != _texture.width || regionIdMap.height != _texture.height)
        {
            x = Mathf.Clamp(Mathf.RoundToInt((workingPixel.x / (float)(_texture.width - 1)) * (regionIdMap.width - 1)), 0, regionIdMap.width - 1);
            y = Mathf.Clamp(Mathf.RoundToInt((workingPixel.y / (float)(_texture.height - 1)) * (regionIdMap.height - 1)), 0, regionIdMap.height - 1);
        }

        Color32 encoded = regionIdMap.GetPixel(x, y);
        return encoded.r + (encoded.g << 8) + (encoded.b << 16);
    }
    

    void FloodFill(Vector2Int startPos, Color targetColor, Color fillColor)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(startPos);
        visited.Add(startPos);

        List<Vector2Int> pixelsToFill = new List<Vector2Int>();

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            pixelsToFill.Add(current);

            Vector2Int[] neighbors = {
                new Vector2Int(current.x + 1, current.y),
                new Vector2Int(current.x - 1, current.y),
                new Vector2Int(current.x, current.y + 1),
                new Vector2Int(current.x, current.y - 1),
            };

            foreach (var neighbor in neighbors)
            {
                if (!IsInsideTexture(neighbor)) continue;
                if (visited.Contains(neighbor)) continue;

                Color neighborColor = _texture.GetPixel(neighbor.x, neighbor.y);

                if (IsOutlinePixel(neighborColor)) continue;
                if (!ColorsMatch(neighborColor, targetColor, tolerance)) continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        foreach (var pixel in pixelsToFill)
            _texture.SetPixel(pixel.x, pixel.y, fillColor);

        _texture.Apply();
    }

    private void MarkRegionCompleted(int regionId)
    {
        if (paintedRegions.Contains(regionId))
            return;

        paintedRegions.Add(regionId);

        Debug.Log($"Completed {paintedRegions.Count}/{totalRegions}");

        if (paintedRegions.Count >= totalRegions)
        {
            CompleteDrawing();
        }
    }

    private void CompleteDrawing()
    {
        Debug.Log("🎉 DRAWING COMPLETED!");

        DrawingCompleted?.Invoke();
    }

    Vector2Int WorldToPixel(Vector2 worldPos)
    {
        Vector2 localPos = DrawingRenderer.transform.InverseTransformPoint(worldPos);
        Sprite sprite = DrawingRenderer.sprite;
        float pixelsPerUnit = sprite.pixelsPerUnit;

        int px = Mathf.FloorToInt((localPos.x * pixelsPerUnit) + sprite.pivot.x + sprite.rect.x);
        int py = Mathf.FloorToInt((localPos.y * pixelsPerUnit) + sprite.pivot.y + sprite.rect.y);

        return new Vector2Int(px, py);
    }

    bool IsInsideTexture(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _texture.width &&
               pos.y >= 0 && pos.y < _texture.height;
    }

    bool ColorsMatch(Color a, Color b, int tol)
    {
        float toleranceValue = tol / 255f;
        return Mathf.Abs(a.r - b.r) <= toleranceValue &&
               Mathf.Abs(a.g - b.g) <= toleranceValue &&
               Mathf.Abs(a.b - b.b) <= toleranceValue;
    }

    bool IsOutlinePixel(Color c)
    {
        float brightness = (c.r + c.g + c.b) / 3f;
        return brightness < 0.3f;
    }

    public void SetPaint(int number, Color color)
    {
        color.a = 1f;
        _selectedNumber = number;
        _selectedColor = color;
        Debug.Log("Active paint: " + number + " / " + ColorUtility.ToHtmlStringRGB(color));
    }

    public void SetColor(Color color)
    {
        SetPaint(-1, color);
    }

    public int GetSelectedNumber()
    {
        return _selectedNumber;
    }

    public Color GetSelectedColor()
    {
        return _selectedColor;
    }

    private void LoadDrawingData()
    {
        DrawingData drawing = GameSceneLoader.SelectedDrawing;

        // Fallback for testing directly in SampleScene
        if (drawing == null)
        {
            Debug.Log("ColoringEngine: No SelectedDrawing found. Using Inspector references.");
            return;
        }

        CurrentDrawing = drawing;

        Debug.Log($"Loading drawing: {drawing.name}");

        paintPaletteJson = drawing.paletteJson;
        regionIdMap = drawing.regionIdMap;

        DrawingRenderer.sprite = Sprite.Create(
            drawing.outlineTexture,
            new Rect(0, 0, drawing.outlineTexture.width, drawing.outlineTexture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
        GameSceneLoader.ClearSelectedDrawing();
    }
}
