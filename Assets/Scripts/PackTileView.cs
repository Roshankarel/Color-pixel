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

    private MainMenuBootstrap controller;

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

    public void SetController(MainMenuBootstrap mainMenuController)
    {
        controller = mainMenuController;
    }

    public void Refresh(bool unlocked, bool complete, bool purchasable, int completedCount, int totalCount, Texture2D thumbnailTexture)
    {
        if (nameText != null)
            nameText.text = GetPackName(pack);

        RefreshThumbnail(thumbnailTexture, unlocked);
        RefreshStateText(unlocked, complete, purchasable, completedCount, totalCount);

        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        if (completeBadge != null)
            completeBadge.SetActive(unlocked && complete);

        if (progressFill != null)
            progressFill.fillAmount = totalCount <= 0 ? 0f : Mathf.Clamp01(completedCount / (float)totalCount);

        Image background = GetComponent<Image>();
        if (background != null)
            background.color = unlocked ? Color.white : new Color(0.72f, 0.74f, 0.78f, 1f);
    }

    private void NotifyClicked()
    {
        if (controller != null)
            controller.OnPackTileClicked(this);
    }

    private void RefreshThumbnail(Texture2D thumbnailTexture, bool unlocked)
    {
        if (thumbnailImage == null)
            return;

        thumbnailImage.color = unlocked ? Color.white : new Color(0.68f, 0.7f, 0.74f, 1f);
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

    private void RefreshStateText(bool unlocked, bool complete, bool purchasable, int completedCount, int totalCount)
    {
        if (stateText == null)
            return;

        if (!unlocked)
        {
            stateText.text = purchasable ? "100 coins or 2 ads to unlock" : "Complete previous pack to unlock";
            return;
        }

        stateText.text = complete ? "Complete" : completedCount + "/" + totalCount + " complete";
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
