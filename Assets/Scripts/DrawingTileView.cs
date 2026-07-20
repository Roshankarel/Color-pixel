using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrawingTileView : MonoBehaviour
{
    public DrawingData Drawing;

    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Button button;

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

    

    public void Refresh(bool unlocked, bool completed)
    {
        if (nameText != null)
            nameText.text = Drawing.name;

        if (statusText != null)
            statusText.text = completed ? "Completed" :
                            unlocked ? "Ready" : "Locked";

        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

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
            button.interactable = unlocked;
    }

    private void OnClicked()
    {
        if (controller != null)
            controller.OnDrawingTileClicked(this);
    }
    
}