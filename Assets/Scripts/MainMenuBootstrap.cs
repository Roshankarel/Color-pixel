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
    
    public Button settingsButton;
    public Image bannerAdAnchor;

    [Header("Library References")]
    


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



    private void WireButtons()
    {
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
