using UnityEngine;

public class PopupTester : MonoBehaviour
{
    [SerializeField] private UnlockPopupUI popup;
    [SerializeField] private DrawingData testDrawing;

    public void OpenPopup()
    {
        popup.Show(testDrawing);
    }
}