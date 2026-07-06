using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuBootstrap : MonoBehaviour
{
    [Header("Content")]
    public DrawingCatalog catalog;
    public string gameSceneName = "SampleScene";

    [Header("PlayerPrefs")]
    public string coinsKey = "coins";

    [Header("Main Menu References")]
    public TMP_Text titleText;
    public TMP_Text coinsText;
    public Button playButton;
    public Button libraryButton;
    public Button settingsButton;
    public Image bannerAdAnchor;

    [Header("Library References")]
    public GameObject libraryOverlay;
    public Button libraryCloseButton;
    public Transform packTileContainer;
    public PackTileView packTilePrefab;
    

    [Header("Locked Pack Modal")]
    public GameObject lockedPackModal;
    public TMP_Text lockedPackMessageText;
    public Button lockedPackCloseButton;

    private ProgressManager progressManager;
    private GameSceneLoader gameSceneLoader;
    private int lastDisplayedCoins = int.MinValue;

    void Awake()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (catalog == null)
            catalog = Resources.Load<DrawingCatalog>("DrawingCatalog");

        progressManager = ProgressManager.EnsureExists();
        progressManager.InitializeFromCatalog(catalog);
        progressManager.OnProgressChanged += RefreshPackTiles;

        GameSceneLoader.EnsureExists();
        gameSceneLoader = GameSceneLoader.Instance;
        gameSceneLoader.SetGameSceneName(gameSceneName);

        WireButtons();
        HideLibrary();
        HideLockedPackModal();
        RefreshCoins(force: true);
        RefreshPackTiles();
    }

    void OnDestroy()
    {
        if (progressManager != null)
            progressManager.OnProgressChanged -= RefreshPackTiles;
    }

    void Update()
    {
        RefreshCoins(force: false);
    }

    public void PlayCurrentLevel()
    {
        DrawingData drawing = progressManager.GetCurrentPlayableDrawing(catalog);
        if (drawing == null)
        {
            Debug.LogError("No playable drawing found.");
            return;
        }
        Debug.Log($"Playable drawing = {drawing.name}");
        gameSceneLoader.LoadDrawing(drawing);
       //gameSceneLoader.LoadDrawing(testDrawing);
    }

    public void ShowLibrary()
    {
        if (libraryOverlay != null)
            libraryOverlay.SetActive(true);

        RefreshPackTiles();
    }

    public void HideLibrary()
    {
        if (libraryOverlay != null)
            libraryOverlay.SetActive(false);
    }

    public void OnPackTileClicked(PackTileView tile)
    {
        Debug.Log("Clicked " + tile.Pack);
        if (tile == null)
            return;

        DrawingPack pack = tile.Pack;
        if (progressManager.IsPackUnlocked(pack))
            return;

        ShowLockedPackModal(progressManager.IsPackPurchasable(catalog, pack)
            ? "Unlock available later:\n100 coins or 2 ads"
            : "Complete previous pack to unlock.");
    }

    private void WireButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(PlayCurrentLevel);
            playButton.onClick.AddListener(PlayCurrentLevel);
        }

        if (libraryButton != null)
        {
            libraryButton.onClick.RemoveListener(ShowLibrary);
            libraryButton.onClick.AddListener(ShowLibrary);
        }

        if (libraryCloseButton != null)
        {
            libraryCloseButton.onClick.RemoveListener(HideLibrary);
            libraryCloseButton.onClick.AddListener(HideLibrary);
        }

        if (lockedPackCloseButton != null)
        {
            lockedPackCloseButton.onClick.RemoveListener(HideLockedPackModal);
            lockedPackCloseButton.onClick.AddListener(HideLockedPackModal);
        }

    }

    private void RefreshCoins(bool force)
    {
        int coins = progressManager.GetCoins();
        if (!force && coins == lastDisplayedCoins)
            return;

        lastDisplayedCoins = coins;
        if (coinsText != null)
            coinsText.text = $"Coins: {progressManager.GetCoins()}";
    }

    private void RefreshPackTiles()
    {
        if (packTileContainer == null || packTilePrefab == null || catalog == null)
        return;

    // Remove old tiles
    foreach (Transform child in packTileContainer)
        Destroy(child.gameObject);

    // Create one tile for each pack
    foreach (DrawingPack pack in System.Enum.GetValues(typeof(DrawingPack)))
    {
        PackTileView tile = Instantiate(packTilePrefab, packTileContainer);

        tile.Pack = pack;
        tile.SetController(this);

        bool unlocked = progressManager.IsPackUnlocked(pack);
        bool complete = progressManager.IsPackFinished(catalog, pack);
        bool purchasable = progressManager.IsPackPurchasable(catalog, pack);

        int completedCount = progressManager.GetCompletedOrSkippedCount(pack);
        int totalCount = catalog.GetLevelCount(pack);

        DrawingData thumbnail = catalog.GetFirstDrawing(pack);

        tile.Refresh(
            unlocked,
            complete,
            purchasable,
            completedCount,
            totalCount,
            thumbnail == null ? null : thumbnail.outlineTexture
        );
    }
    }

    private void ShowLockedPackModal(string message)
    {
        if (lockedPackMessageText != null)
            lockedPackMessageText.text = message;

        if (lockedPackModal != null)
            lockedPackModal.SetActive(true);
    }

    private void HideLockedPackModal()
    {
        if (lockedPackModal != null)
            lockedPackModal.SetActive(false);
    }
}
