using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockPopupUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private Image previewImage;
    [SerializeField] private TMP_Text drawingNameText;
    [SerializeField] private TMP_Text priceText;

    [SerializeField] private Button unlockButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private ProgressManager progressManager;
    [SerializeField] private LibraryUI libraryUI;

    private DrawingData currentDrawing;

    private void Awake()
    {
        if (unlockButton != null)
            unlockButton.onClick.AddListener(OnUnlockPressed);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(Hide);

        if (progressManager == null)
            progressManager = ProgressManager.Instance;

        Hide();
    }

    public void Show(DrawingData drawing)
    {
        currentDrawing = drawing;

        if (drawingNameText != null)
            drawingNameText.text = drawing.name;

        if (priceText != null)
            priceText.text = drawing.unlockCost.ToString();

        if (previewImage != null)
        {
            previewImage.sprite = Sprite.Create(
                drawing.outlineTexture,
                new Rect(0, 0, drawing.outlineTexture.width, drawing.outlineTexture.height),
                new Vector2(0.5f, 0.5f),
                100f);

            previewImage.preserveAspect = true;
        }

        root.SetActive(true);
    }

    public void TestPopup(DrawingData drawing)
    {
        Show(drawing);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

   private void OnUnlockPressed()
    {
        Debug.Log("Step 1");

        if (currentDrawing == null)
            return;

        Debug.Log("Step 2");

        bool purchased = progressManager.PurchaseDrawing(currentDrawing);

        Debug.Log("Purchased = " + purchased);

        if (!purchased)
        {
            Debug.Log("Not enough coins.");
            return;
        }

        Debug.Log("Step 3");

        Hide();

        Debug.Log("Step 4");

        libraryUI.RefreshDrawing(currentDrawing);

        Debug.Log("Step 5");

        GameSceneLoader.Instance.LoadDrawing(currentDrawing);

        Debug.Log("Step 6");
    }
    
}