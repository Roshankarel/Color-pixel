using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class PackTileView : MonoBehaviour
{
    [Header("Pack")]
    public DrawingPack pack;

    [Header("References")]
    public TMP_Text nameText;
    public Image thumbnailImage;
    public TMP_Text stateText;
    public GameObject lockIcon;
    public GameObject completeBadge;
    public Image progressFill;
    public Button button;

    private LibraryUI controller;

    public DrawingPack Pack 
    {
        get => pack;
        set => pack = value;
    }

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(NotifyClicked);
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(NotifyClicked);
    }

    public void SetController(LibraryUI libraryUI)
    {
        controller = libraryUI;
    }

    public void Refresh(bool complete,int completedCount,int totalCount,Texture2D thumbnailTexture)
    {
        if (nameText != null)
            nameText.text = GetPackName(pack);

        RefreshThumbnail(thumbnailTexture);
        RefreshStateText( complete,  completedCount, totalCount);

        if (lockIcon != null)
            lockIcon.SetActive(false);

        if (completeBadge != null)
            completeBadge.SetActive(complete);

        if (progressFill != null)
            progressFill.fillAmount = totalCount <= 0 ? 0f : Mathf.Clamp01(completedCount / (float)totalCount);

        Image background = GetComponent<Image>();
        if (background != null)
            background.color = Color.white;
    }

    private void NotifyClicked()
    {
        if (controller != null)
            controller.OnPackTileClicked(this);
    }

    private void RefreshThumbnail(Texture2D thumbnailTexture)
    {
        if (thumbnailImage == null)
            return;

        thumbnailImage.color = Color.white;
        thumbnailImage.preserveAspect = true;

        if (thumbnailTexture == null)
        {
            thumbnailImage.sprite = null;
            return;
        }

        thumbnailImage.sprite = Sprite.Create(
            thumbnailTexture,
            new Rect(0, 0, thumbnailTexture.width, thumbnailTexture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private void RefreshStateText(bool complete, int completedCount, int totalCount)
    {
        if (stateText == null)
            return;

        stateText.text = complete
            ? "Complete"
            : $"{completedCount}/{totalCount} Complete";
    }


    private string GetPackName(DrawingPack drawingPack)
    {
        switch (drawingPack)
        {
            case DrawingPack.Mandala:
                return "Mandala";
            case DrawingPack.Animal:
                return "Animal";
            case DrawingPack.Nature:
                return "Nature";
            case DrawingPack.Fantasy:
                return "Fantasy";
            case DrawingPack.Seasonal:
                return "Seasonal";
            case DrawingPack.Premium:
                return "Premium";
            default:
                return drawingPack.ToString();
        }
    }
}
