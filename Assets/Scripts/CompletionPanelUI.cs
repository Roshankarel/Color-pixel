using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;


public class CompletionPanelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text coinText;

    [Header("Buttons")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button nextButton;

    public event Action NextButtonPressed;

    private void Awake()
    {

        homeButton.onClick.AddListener(OnHomeClicked);
        nextButton.onClick.AddListener(OnNextClicked);
    }

    public void Show(int coins)
    {
        gameObject.SetActive(true);
        Debug.Log("CompletionPanelUI.Show() called ");
        coinText.text = $"+{coins} Coins";
        
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnHomeClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnNextClicked()
    {
        NextButtonPressed?.Invoke();
        // We'll implement next level loading later.

    }
}