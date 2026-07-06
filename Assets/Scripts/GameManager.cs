using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ColoringEngine coloringEngine;
    [SerializeField] private CompletionPanelUI completionPanel;
    [SerializeField] private DrawingCatalog catalog;

    private ProgressManager progressManager;

    private void Awake()
    {
        progressManager = ProgressManager.EnsureExists();
    }

    private void OnEnable()
    {
        if (coloringEngine != null)
            coloringEngine.DrawingCompleted += OnDrawingCompleted;
        
         if (completionPanel != null)
        completionPanel.NextButtonPressed += OnNextPressed;
    }
            

    private void OnDisable()
    {
        if (coloringEngine != null)
            coloringEngine.DrawingCompleted -= OnDrawingCompleted;
        
         if (completionPanel != null)
        completionPanel.NextButtonPressed -= OnNextPressed;
            
    }

    private void OnDrawingCompleted()
    {
        Debug.Log("GameManager : Drawing Completed");

        //Debug.Log("SelectedDrawing before read = " + GameSceneLoader.SelectedDrawing);

        DrawingData drawing = coloringEngine.CurrentDrawing;

        //Debug.Log("CompletionPanel reference = " + completionPanel);
    
       // Debug.Log($"Current Drawing: {drawing?.name}");

        if (drawing != null)
        {
           // Debug.Log($"Pack: {drawing.pack}, Level: {drawing.levelNumber}");

            progressManager.MarkLevelCompleted(
                drawing.pack,
                drawing.levelNumber - 1,
                3
            );
        }

        progressManager.AddCoins(50);

        completionPanel.Show(50);


        // Later:
        // progressManager.MarkLevelCompleted(...);
    }

    private void OnNextPressed()
    {
        Debug.Log("Next Button Pressed");

        DrawingData nextDrawing = progressManager.GetCurrentPlayableDrawing(catalog);

    if (nextDrawing == null)
    {
        Debug.Log("No more drawings available.");

        // We'll return to the Main Menu later.
        return;
    }

    Debug.Log("Loading next drawing: " + nextDrawing.name);

    GameSceneLoader.Instance.LoadDrawing(nextDrawing);
    }
}