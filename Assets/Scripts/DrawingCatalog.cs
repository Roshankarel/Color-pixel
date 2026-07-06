using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DrawingCatalog", menuName = "Color Pixel/Drawing Catalog")]
public class DrawingCatalog : ScriptableObject
{
    public List<DrawingData> drawings = new List<DrawingData>();

    public static readonly DrawingPack[] PackOrder =
    {
        DrawingPack.Mandala,
        DrawingPack.Animal,
        DrawingPack.Nature,
        DrawingPack.Fantasy,
        DrawingPack.Seasonal,
        DrawingPack.Premium
    };

    public List<DrawingData> GetDrawings(DrawingPack pack)
    {
        List<DrawingData> results = new List<DrawingData>();

        for (int i = 0; i < drawings.Count; i++)
        {
            DrawingData drawing = drawings[i];
            if (drawing != null && drawing.pack == pack)
                results.Add(drawing);
        }

        results.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));
        return results;
    }

    public DrawingData GetDrawing(DrawingPack pack, int levelIndex)
    {
        List<DrawingData> packDrawings = GetDrawings(pack);
        if (levelIndex < 0 || levelIndex >= packDrawings.Count)
            return null;

        return packDrawings[levelIndex];
    }

    public DrawingData GetFirstDrawing(DrawingPack pack)
    {
        return GetDrawing(pack, 0);
    }

    public int GetLevelCount(DrawingPack pack)
    {
        return GetDrawings(pack).Count;
    }
}
