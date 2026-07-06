using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPaletteManager : MonoBehaviour
{
    [Header("Palette Setup")]
    public TextAsset paintPaletteJson;
    public GameObject colorSwatchPrefab;
    public Transform paletteContainer;
    public bool useFallbackPalette = false;

    [Header("Selection Ring")]
    public Color selectedRingColor = Color.white;

    private readonly List<PaletteItem> _paletteItems = new List<PaletteItem>();
    private GameObject currentSelectedSwatch;
    private Color currentSelectedColor;
    private int currentSelectedNumber = -1;

    class PaletteItem
    {
        public int number;
        public Color color;
    }

    public void RefreshPalette()
    {
        foreach (Transform child in paletteContainer)
        {
            Destroy(child.gameObject);
        }
        BuildPaletteItems();
        GenerateSwatches();
    }

    void BuildPaletteItems()
    {
        _paletteItems.Clear();

        TextAsset jsonAsset = paintPaletteJson;
        if (jsonAsset == null && ColoringEngine.Instance != null)
            jsonAsset = ColoringEngine.Instance.paintPaletteJson;

        PaintByNumberPaletteData data = PaintByNumberPaletteData.FromJson(jsonAsset);
        if (data != null && data.colors != null)
        {
            List<PaintByNumberColorEntry> sortedColors = new List<PaintByNumberColorEntry>(data.colors);
            sortedColors.Sort((a, b) => a.number.CompareTo(b.number));

            for (int i = 0; i < sortedColors.Count; i++)
            {
                if (!data.TryGetUnityColor(sortedColors[i].number, out Color color))
                {
                    Debug.LogWarning("ColorPaletteManager: Skipping invalid color number " + sortedColors[i].number + ".");
                    continue;
                }

                _paletteItems.Add(new PaletteItem
                {
                    number = sortedColors[i].number,
                    color = color
                });
            }
        }

        if (_paletteItems.Count == 0 && useFallbackPalette)
            SetFallbackPaletteItems();

        if (_paletteItems.Count == 0)
            Debug.LogError("ColorPaletteManager: No palette colors found. Assign a valid paint_palette JSON TextAsset.");
    }

    void SetFallbackPaletteItems()
    {
        AddFallbackItem(1, "F04090");
        AddFallbackItem(2, "40B0E0");
        AddFallbackItem(3, "80C030");
        AddFallbackItem(4, "F07000");
        AddFallbackItem(5, "9040C0");
        AddFallbackItem(6, "F0C000");
    }

    void AddFallbackItem(int number, string hex)
    {
        if (!ColorUtility.TryParseHtmlString("#" + hex, out Color color))
            return;

        _paletteItems.Add(new PaletteItem
        {
            number = number,
            color = color
        });
    }

    void GenerateSwatches()
    {
        if (colorSwatchPrefab == null || paletteContainer == null)
        {
            Debug.LogError("ColorPaletteManager: Swatch prefab or palette container is not assigned.");
            return;
        }

        foreach (Transform child in paletteContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < _paletteItems.Count; i++)
        {
            PaletteItem item = _paletteItems[i];
            GameObject swatch = Instantiate(colorSwatchPrefab, paletteContainer);
            swatch.name = "Swatch_" + item.number;

            Image swatchImage = swatch.GetComponent<Image>();
            if (swatchImage != null)
                swatchImage.color = item.color;

            SetSwatchNumberLabel(swatch, item.number);
            ConfigureSelectionRing(swatch);

            Button btn = swatch.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    OnSwatchSelected(swatch, item.number, item.color);
                });
            }
        }

        if (paletteContainer.childCount > 0)
        {
            PaletteItem firstItem = _paletteItems[0];
            GameObject firstSwatch = paletteContainer.GetChild(0).gameObject;
            OnSwatchSelected(firstSwatch, firstItem.number, firstItem.color);
        }
    }

    void SetSwatchNumberLabel(GameObject swatch, int number)
    {
        Text uiText = swatch.GetComponentInChildren<Text>(true);
        if (uiText != null)
            uiText.text = number.ToString();

        TMP_Text tmpText = swatch.GetComponentInChildren<TMP_Text>(true);
        if (tmpText != null)
            tmpText.text = number.ToString();
    }

    void ConfigureSelectionRing(GameObject swatch)
    {
        Transform ring = swatch.transform.Find("SelectionRing");
        if (ring == null)
            return;

        ring.gameObject.SetActive(false);

        Image ringImage = ring.GetComponent<Image>();
        if (ringImage != null)
            ringImage.color = selectedRingColor;
    }

    void OnSwatchSelected(GameObject swatch, int number, Color color)
    {
        if (currentSelectedSwatch != null)
        {
            Transform oldRing = currentSelectedSwatch.transform.Find("SelectionRing");
            if (oldRing != null)
                oldRing.gameObject.SetActive(false);
        }

        Transform newRing = swatch.transform.Find("SelectionRing");
        if (newRing != null)
            newRing.gameObject.SetActive(true);

        currentSelectedSwatch = swatch;
        currentSelectedNumber = number;
        currentSelectedColor = color;

        if (ColoringEngine.Instance != null)
            ColoringEngine.Instance.SetPaint(number, color);
        else
            Debug.LogWarning("ColorPaletteManager: ColoringEngine.Instance is null!");

        Debug.Log("Selected paint: " + number + " / " + ColorUtility.ToHtmlStringRGB(color));
    }

    public int GetSelectedNumber()
    {
        return currentSelectedNumber;
    }

    public Color GetSelectedColor()
    {
        return currentSelectedColor;
    }
}
