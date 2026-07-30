using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrawingTileView : MonoBehaviour
{
    public DrawingData Drawing;

    [Header("UI")]
    
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private Image badgeIcon;
    [SerializeField] private Button button;


    [SerializeField] private Sprite coinSprite;
    [SerializeField] private Sprite rewardedAdSprite;
    [SerializeField] private Sprite premiumSprite;

    private LibraryUI controller;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnClicked);
    }

    public void SetController(LibraryUI library)
    {
        controller = library;
    }

    

    public void Refresh(bool completed)
    {

        if (badgeIcon != null)
        {
            badgeIcon.gameObject.SetActive(false);

            // Completed drawings don't show an unlock badge.
            if (!completed)
            {
                switch (Drawing.unlockType)
                {
                    case UnlockType.Free:
                        // No badge.
                        break;

                    case UnlockType.Coins:
                        badgeIcon.sprite = coinSprite;
                        badgeIcon.gameObject.SetActive(true);
                        break;

                    case UnlockType.RewardedAd:
                        badgeIcon.sprite = rewardedAdSprite;
                        badgeIcon.gameObject.SetActive(true);
                        break;

                    case UnlockType.Premium:
                        badgeIcon.sprite = premiumSprite;
                        badgeIcon.gameObject.SetActive(true);
                        break;
                }
            }
        }

        if (thumbnailImage != null)
        {
            thumbnailImage.sprite = Sprite.Create(
                Drawing.outlineTexture,
                new Rect(0, 0, Drawing.outlineTexture.width, Drawing.outlineTexture.height),
                new Vector2(0.5f, 0.5f),
                100f);

            thumbnailImage.preserveAspect = true;
        }

        if (button != null)
            button.interactable = true;
    }

    private void OnClicked()
    {
        if (controller != null)
            controller.OnDrawingTileClicked(this);
    }
    
}