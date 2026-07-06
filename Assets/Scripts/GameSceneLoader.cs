using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneLoader : MonoBehaviour
{
    public static GameSceneLoader Instance { get; private set; }
    public static DrawingData SelectedDrawing { get; private set; }

    [SerializeField] private string gameSceneName = "SampleScene";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void EnsureExists()
    {
        if (Instance != null)
            return;

        GameObject loaderObject = new GameObject("GameSceneLoader");
        loaderObject.AddComponent<GameSceneLoader>();
    }

    public void SetGameSceneName(string sceneName)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
            gameSceneName = sceneName;
    }

    public void LoadDrawing(DrawingData drawing)
    {
        if (drawing == null)
        {
            Debug.LogWarning("GameSceneLoader: Cannot load a null drawing.");
            return;
        }

        Debug.Log($"LoadDrawing() received: {drawing.name}");

        SelectedDrawing = drawing;

        Debug.Log($"SelectedDrawing is now: {SelectedDrawing.name}");
        
        SceneManager.LoadScene(gameSceneName);
    }
    public static void ClearSelectedDrawing()
    {
        SelectedDrawing = null;
    }
}
