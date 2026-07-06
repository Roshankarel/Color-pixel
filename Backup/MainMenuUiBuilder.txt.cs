using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public static class MainMenuUiBuilder
{
    private const string CatalogPath = "Assets/Resources/DrawingCatalog.asset";
    private const string PackTilePrefabPath = "Assets/Prefabs/PackTile.prefab";

    [MenuItem("Tools/Color Pixel/Build Main Menu UI")]
    public static void BuildMainMenuUi()
    {
        EnsureFolder("Assets/Prefabs");

        GameObject existingCanvas = GameObject.Find("MainMenuCanvas");
        if (existingCanvas != null)
            Undo.DestroyObjectImmediate(existingCanvas);

        GameObject existingEventSystem = GameObject.Find("EventSystem");
        if (existingEventSystem == null)
            CreateEventSystem();

        GameObject packTilePrefab = CreateOrUpdatePackTilePrefab();

        Canvas canvas = CreateCanvas();
        MainMenuBootstrap controller = canvas.gameObject.AddComponent<MainMenuBootstrap>();
        controller.catalog = AssetDatabase.LoadAssetAtPath<DrawingCatalog>(CatalogPath);
        controller.gameSceneName = "SampleScene";
        controller.coinsKey = "coins";

        GameObject mainPanel = CreatePanel("MainMenuPanel", canvas.transform, new Color(0.96f, 0.98f, 1f, 1f));

        controller.titleText = CreateText("TitleText", mainPanel.transform, "COLOR PIXEL", 46, FontStyle.Bold, TextAnchor.MiddleCenter) ;
        SetRect(controller.titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -88), new Vector2(520, 80));

        controller.coinsText = CreateText("CoinsText", mainPanel.transform, "Coins: 0", 24, FontStyle.Bold, TextAnchor.MiddleLeft) ;
        SetRect(controller.coinsText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(116, -62), new Vector2(190, 48));

        controller.settingsButton = CreateButton("SettingsButton", mainPanel.transform, "Settings", new Color(0.9f, 0.93f, 0.97f, 1f));
        SetRect(controller.settingsButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-92, -62), new Vector2(132, 48));

        controller.playButton = CreateButton("PlayButton", mainPanel.transform, "Play", new Color(0.1f, 0.58f, 0.45f, 1f));
        SetRect(controller.playButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 70), new Vector2(360, 86));

        controller.libraryButton = CreateButton("LibraryButton", mainPanel.transform, "Library", new Color(0.18f, 0.35f, 0.74f, 1f));
        SetRect(controller.libraryButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -36), new Vector2(360, 72));

        GameObject banner = CreatePanel("BannerAdAnchor", mainPanel.transform, new Color(0.9f, 0.92f, 0.95f, 1f));
        controller.bannerAdAnchor = banner.GetComponent<Image>();
        SetRect(banner.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0, 32), new Vector2(-32, 52));
        TMP_Text bannerText = CreateText("Label", banner.transform, "Banner Ad Anchor", 18, FontStyle.Normal, TextAnchor.MiddleCenter) ;
        bannerText.color = new Color(0.28f, 0.32f, 0.38f, 1f);
        StretchToParent(bannerText.rectTransform);

        BuildLibraryOverlay(canvas.transform, controller, packTilePrefab);
        BuildLockedPackModal(canvas.transform, controller);

        Selection.activeGameObject = canvas.gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Color Pixel Main Menu UI built. The Canvas hierarchy is now editable in the scene, and the pack tile prefab is saved at " + PackTilePrefabPath + ".");
    }

    private static void BuildLibraryOverlay(Transform parent, MainMenuBootstrap controller, GameObject packTilePrefab)
    {
        GameObject overlay = CreatePanel("LibraryOverlay", parent, new Color(0.96f, 0.98f, 1f, 1f));
        controller.libraryOverlay = overlay;

        TMP_Text title = CreateText("LibraryTitle", overlay.transform, "Library", 40, FontStyle.Bold, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -72), new Vector2(420, 70));

        controller.libraryCloseButton = CreateButton("CloseLibraryButton", overlay.transform, "Close", new Color(0.9f, 0.93f, 0.97f, 1f));
        SetRect(controller.libraryCloseButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-94, -64), new Vector2(134, 50));

        GameObject scrollView = CreateScrollGrid("PackGrid", overlay.transform, 2, new Vector2(28, 28), new Vector2(430, 300));
        Transform content = scrollView.transform.Find("Viewport/Content");

        controller.packTileContainer = content;
        controller.packTilePrefab = packTilePrefab.GetComponent<PackTileView>();

        // Optional: create preview tiles in the editor
        for (int i = 0; i < DrawingCatalog.PackOrder.Length; i++)
        {
            GameObject tileObject = PrefabUtility.InstantiatePrefab(packTilePrefab, content) as GameObject;
            tileObject.name = "PackTile_" + DrawingCatalog.PackOrder[i];

            PackTileView tile = tileObject.GetComponent<PackTileView>();
            tile.Pack = DrawingCatalog.PackOrder[i];
            SetPackTileEditorLabel(tile);
        }
        overlay.SetActive(false);
    }
    private static GameObject CreateOrUpdatePackTilePrefab()
    {
        GameObject tile = CreatePackTileObject();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tile, PackTilePrefabPath);
        Object.DestroyImmediate(tile);
        AssetDatabase.SaveAssets();
        return prefab;
    }

    private static GameObject CreatePackTileObject()
    {
        GameObject tileObject = new GameObject("PackTile", typeof(RectTransform), typeof(Image), typeof(Button), typeof(PackTileView));
        RectTransform tileRect = tileObject.GetComponent<RectTransform>();
        tileRect.sizeDelta = new Vector2(430, 300);
        tileObject.GetComponent<Image>().color = Color.white;

        PackTileView tile = tileObject.GetComponent<PackTileView>();
        tile.button = tileObject.GetComponent<Button>();

        tile.nameText = CreateText("NameText", tileObject.transform, "Pack", 28, FontStyle.Bold, TextAnchor.MiddleCenter);
        SetRect(tile.nameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -44), new Vector2(360, 52));

        GameObject thumbnail = new GameObject("Thumbnail", typeof(RectTransform), typeof(Image));
        thumbnail.transform.SetParent(tileObject.transform, false);
        tile.thumbnailImage = thumbnail.GetComponent<Image>();
        tile.thumbnailImage.preserveAspect = true;
        tile.thumbnailImage.color = Color.white;
        SetRect(thumbnail.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 24), new Vector2(260, 140));

        tile.stateText = CreateText("StateText", tileObject.transform, "0/0 complete", 22, FontStyle.Normal, TextAnchor.MiddleCenter);
        tile.stateText.color = new Color(0.16f, 0.2f, 0.28f, 1f);
        SetRect(tile.stateText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 78), new Vector2(360, 68));

        tile.lockIcon = CreateText("LockIcon", tileObject.transform, "LOCKED", 24, FontStyle.Bold, TextAnchor.MiddleCenter).gameObject;
        SetRect(tile.lockIcon.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 12), new Vector2(220, 48));

        tile.completeBadge = CreateText("CompleteBadge", tileObject.transform, "COMPLETE", 20, FontStyle.Bold, TextAnchor.MiddleCenter).gameObject;
        SetRect(tile.completeBadge.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-74, -42), new Vector2(130, 38));
        tile.completeBadge.SetActive(false);

        GameObject progressBar = CreatePanel("ProgressBar", tileObject.transform, new Color(0.82f, 0.86f, 0.9f, 1f));
        SetRect(progressBar.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 46), new Vector2(300, 24));

        GameObject fill = CreatePanel("Fill", progressBar.transform, new Color(0.1f, 0.58f, 0.45f, 1f));
        tile.progressFill = fill.GetComponent<Image>();
        tile.progressFill.type = Image.Type.Filled;
        tile.progressFill.fillMethod = Image.FillMethod.Horizontal;
        tile.progressFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        tile.progressFill.fillAmount = 0f;
        StretchToParent(fill.GetComponent<RectTransform>());

        return tileObject;
    }

    private static void SetPackTileEditorLabel(PackTileView tile)
    {
        if (tile == null || tile.nameText == null)
            return;

        tile.nameText.text = tile.pack.ToString();
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObject, "Build Main Menu UI");

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(eventSystem, "Build Main Menu UI");
    }

    private static GameObject CreateScrollGrid(string name, Transform parent, int columns, Vector2 spacing, Vector2 cellSize)
    {
        GameObject scrollObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollObject.transform.SetParent(parent, false);
        scrollObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        SetRect(scrollObject.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0, -120), new Vector2(-80, -220));

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollObject.transform, false);
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        StretchToParent(viewport.GetComponent<RectTransform>());

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 1f);
        contentRect.anchorMax = new Vector2(0.5f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;

        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        grid.cellSize = cellSize;
        grid.spacing = spacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;

        return scrollObject;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        panel.GetComponent<Image>().color = color;
        StretchToParent(panel.GetComponent<RectTransform>());
        return panel;
    }

    private static Button CreateButton(string name, Transform parent, string label, Color color)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<Image>().color = color;

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.12f);
        button.colors = colors;

        TMP_Text text = CreateText("Label", buttonObject.transform, label, 28, FontStyle.Bold, TextAnchor.MiddleCenter);
        text.color = Color.white;
        StretchToParent(text.rectTransform);
        return button;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, int size, FontStyle style, TextAnchor anchor)
{
    GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
    textObject.transform.SetParent(parent, false);

    TextMeshProUGUI tmpText = textObject.GetComponent<TextMeshProUGUI>();
    tmpText.text = text;
    tmpText.fontSize = size;
    tmpText.fontStyle = style == FontStyle.Bold ? FontStyles.Bold : FontStyles.Normal;
    tmpText.alignment = ConvertAnchor(anchor);
    tmpText.color = new Color(0.08f, 0.12f, 0.18f, 1f);
    tmpText.enableAutoSizing = true;
    tmpText.fontSizeMin = 12;
    tmpText.fontSizeMax = size;
    return tmpText;
}

private static TextAlignmentOptions ConvertAnchor(TextAnchor anchor)
{
    switch (anchor)
    {
        case TextAnchor.MiddleLeft: return TextAlignmentOptions.MidlineLeft;
        case TextAnchor.MiddleRight: return TextAlignmentOptions.MidlineRight;
        case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
        case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
        default: return TextAlignmentOptions.Center;
    }
}


    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string folder = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folder);
    }
}
