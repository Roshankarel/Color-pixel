using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class LibraryUI : MonoBehaviour
{
    [Header("References")]
    public ProgressManager progressManager;
    public DrawingCatalog catalog;
    public Transform packTileContainer;
    public PackTileView packTilePrefab;
    
    public MainMenuBootstrap mainMenu;

    public Transform drawingTileContainer;
    public DrawingTileView drawingTilePrefab;

    [SerializeField] private UnlockPopupUI unlockPopup;

    private readonly List<DrawingTileView> drawingTiles = new();


    private void Awake()
    {
        if(progressManager == null)
            progressManager = ProgressManager.EnsureExists();
            
        progressManager.InitializeFromCatalog(catalog);
    }

    private void Start()
    {
        Debug.Log("LibraryUI onEnable");
        BuildLibrary();

        BuildDrawings();

        FilterCategory(DrawingPack.Mandala);
        
    }
        
    private void BuildLibrary()
    {
        Debug.Log("BuildLibrary");
        // Remove old tiles
        foreach (Transform child in packTileContainer)
            Destroy(child.gameObject);

        // Get every pack that exists in the catalog
        var packs = catalog.drawings
            .Select(d => d.pack)
            .Distinct();

        Debug.Log("Pack count = " + packs.Count());

        foreach (var pack in packs)
        {
            PackTileView tile = Instantiate(packTilePrefab, packTileContainer);

            Debug.Log("Created Pack Tile : " + pack);

            tile.Pack = pack;
            tile.SetController(this);

            int total = catalog.drawings.Count(d => d.pack == pack);
            int completed = progressManager.GetCompletedOrSkippedCount(pack);

           

            Texture2D thumbnail =
                catalog.drawings.First(d => d.pack == pack).outlineTexture;
            bool complete = completed == total;

            tile.Refresh(
                  complete,
                  completed,
                  total,
                  thumbnail);
        }
    }

    private void BuildDrawings()
    {
        drawingTiles.Clear();
        // Remove old drawing tiles
        foreach (Transform child in drawingTileContainer)
            Destroy(child.gameObject);

        // Create a tile for every drawing in the catalog
        foreach (DrawingData drawing in catalog.drawings.OrderBy(d => d.levelNumber))
        {
            DrawingTileView tile =
                Instantiate(drawingTilePrefab, drawingTileContainer);

            tile.Drawing = drawing;
            tile.SetController(this);

            drawingTiles.Add(tile);

            bool completed = progressManager.IsLevelCompleted(
            drawing.pack,
            drawing.levelNumber - 1);

            // New unlock system
            bool unlocked = progressManager.IsDrawingUnlocked(drawing);

            tile.Refresh(completed);

            Debug.Log("Created drawing tile : " + drawing.name);
        }
    }

    private void FilterCategory(DrawingPack pack)
    {
        foreach (DrawingTileView tile in drawingTiles)
        {
            if (tile == null || tile.Drawing == null)
                continue;

            tile.gameObject.SetActive(tile.Drawing.pack == pack);
        }
    }
    public void OnDrawingTileClicked(DrawingTileView tile)
    {
        if (tile == null)
            return;

        DrawingData drawing = tile.Drawing;

        if (progressManager.IsDrawingUnlocked(drawing))
        {
            Debug.Log("Loading drawing : " + drawing.name);
            GameSceneLoader.Instance.LoadDrawing(drawing);
            return;
        }

        switch (drawing.unlockType)
        {
            case UnlockType.Coins:
                unlockPopup.Show(drawing);
                break;

            case UnlockType.RewardedAd:
                progressManager.UnlockRewardedDrawing(drawing);

                GameSceneLoader.Instance.LoadDrawing(drawing);
                break;

            case UnlockType.Premium:
                PremiumManager.EnsureExists().UnlockPremium();

                GameSceneLoader.Instance.LoadDrawing(drawing);

                break;
        }
    }
    
    public void OnPackTileClicked(PackTileView tile)
    {
        if (tile == null)
            return;

        FilterCategory(tile.Pack);
    }

    public void RefreshDrawing(DrawingData drawing)
    {
        foreach (DrawingTileView tile in drawingTiles)
        {
            if (tile.Drawing != drawing)
                continue;

            bool completed = progressManager.IsLevelCompleted(
                drawing.pack,
                drawing.levelNumber - 1);

            bool unlocked = progressManager.IsDrawingUnlocked(drawing);

            tile.Refresh(completed);
            return;
        }
    }
}