using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PaintByNumberPaletteData
{
    public int imageWidth;
    public int imageHeight;
    public PaintByNumberColorEntry[] colors;
    public PaintByNumberRegionEntry[] regions;

    private Dictionary<int, PaintByNumberColorEntry> _colorsByNumber;
    private Dictionary<int, PaintByNumberRegionEntry> _regionsById;

    public static PaintByNumberPaletteData FromJson(TextAsset jsonAsset)
    {
        if (jsonAsset == null)
            return null;

        PaintByNumberPaletteData data = JsonUtility.FromJson<PaintByNumberPaletteData>(jsonAsset.text);
        if (data != null)
            data.BuildLookup();

        return data;
    }

    public void BuildLookup()
    {
        _colorsByNumber = new Dictionary<int, PaintByNumberColorEntry>();
        _regionsById = new Dictionary<int, PaintByNumberRegionEntry>();

        if (colors != null)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (!_colorsByNumber.ContainsKey(colors[i].number))
                    _colorsByNumber.Add(colors[i].number, colors[i]);
            }
        }

        if (regions != null)
        {
            for (int i = 0; i < regions.Length; i++)
            {
                if (!_regionsById.ContainsKey(regions[i].regionId))
                    _regionsById.Add(regions[i].regionId, regions[i]);
            }
        }
    }

    public bool TryGetRegion(int regionId, out PaintByNumberRegionEntry regionEntry)
    {
        EnsureLookup();
        return _regionsById.TryGetValue(regionId, out regionEntry);
    }

    public bool TryGetUnityColor(int colorNumber, out Color color)
    {
        EnsureLookup();
        color = Color.clear;

        if (!_colorsByNumber.TryGetValue(colorNumber, out PaintByNumberColorEntry colorEntry))
            return false;

        if (!ColorUtility.TryParseHtmlString(colorEntry.hex, out color))
            return false;

        color.a = 1f;
        return true;
    }

    private void EnsureLookup()
    {
        if (_colorsByNumber == null || _regionsById == null)
            BuildLookup();
    }
}

[Serializable]
public class PaintByNumberColorEntry
{
    public int number;
    public string hex;
}

[Serializable]
public class PaintByNumberRegionEntry
{
    public int regionId;
    public int colorNumber;
}
