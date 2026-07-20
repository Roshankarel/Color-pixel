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

        GameSceneLoader.EnsureExists();
        gameSceneLoader = GameSceneLoader.Instance;
        gameSceneLoader.SetGameSceneName(gameSceneName);

        WireButtons();
        HideLibrary();
        HideLockedPackModal();
        RefreshCoins(force: true);
        //RefreshPackTiles();
    }

    void OnDestroy()
    {
        //if (progressManager != null)

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
    }

    public void HideLibrary()
    {
        if (libraryOverlay != null)
            libraryOverlay.SetActive(false);
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
