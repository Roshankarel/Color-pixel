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
        Debug.Log("LoadDrawing called");

        if (drawing == null)
        {
            Debug.Log("Drawing is NULL");
            return;
        }

        Debug.Log("Scene = " + gameSceneName);

        SelectedDrawing = drawing;

        Debug.Log("Loading Scene...");

        SceneManager.LoadScene(gameSceneName);

        Debug.Log("LoadScene finished");
    }
    public static void ClearSelectedDrawing()
    {
        SelectedDrawing = null;
    }
}
