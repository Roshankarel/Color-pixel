using UnityEngine;
using System.Linq;

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

    public GameObject packPanel;
    public GameObject drawingPanel; 

    private void Awake()
    {
        if(progressManager == null)
            progressManager = ProgressManager.EnsureExists();
            
        progressManager.InitializeFromCatalog(catalog);
    }
    private void OnEnable()
    {
        Debug.Log("LibraryUI onEnable");
        BuildLibrary();
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

            tile.Pack = pack;
            tile.SetController(this);

            int total = catalog.drawings.Count(d => d.pack == pack);
            int completed = progressManager.GetCompletedOrSkippedCount(pack);

            bool unlocked = progressManager.IsPackUnlocked(pack);
            bool complete = completed == total;
            bool purchasable = false;

            Texture2D thumbnail =
                catalog.drawings.First(d => d.pack == pack).outlineTexture;

            tile.Refresh(
                unlocked,
                complete,
                purchasable,
                completed,
                total,
                thumbnail);
        }
    }
    public void OnDrawingTileClicked(DrawingTileView tile)
    {
        if (tile == null)
            return;

        Debug.Log("Loading drawing : " + tile.Drawing.name);

        GameSceneLoader.Instance.LoadDrawing(tile.Drawing);
    }
    public void ShowPack(DrawingPack pack)
    {
        foreach (Transform child in drawingTileContainer)
            Destroy(child.gameObject);

        var drawings = catalog.drawings
            .Where(d => d.pack == pack)
            .OrderBy(d => d.levelNumber);

        foreach (DrawingData drawing in drawings)
        {
            DrawingTileView tile =
                Instantiate(drawingTilePrefab, drawingTileContainer);

            tile.Drawing = drawing;
            tile.SetController(this);

            Debug.Log("ProgressManager = " + progressManager);
            Debug.Log("Drawing = " + drawing);

            bool completed = progressManager.IsLevelCompleted(
            drawing.pack,
            drawing.levelNumber - 1);

            bool unlocked =
            completed ||
            progressManager.GetCurrentPlayableDrawing(catalog) == drawing;

            tile.Refresh(unlocked, completed);

            Debug.Log("Created drawing tile : " + drawing.name);
        }

        if (packPanel != null)
            packPanel.SetActive(false);

        if (drawingPanel != null)
            drawingPanel.SetActive(true);
        
    }

    public void OnPackTileClicked(PackTileView tile)
    {
        if (tile == null)
            return;

        ShowPack(tile.Pack);
    }
    public void BackToPackList()
    {
        drawingPanel.SetActive(false);
        packPanel.SetActive(true);
    }
}