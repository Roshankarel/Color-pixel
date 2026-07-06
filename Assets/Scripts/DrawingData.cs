using UnityEngine;

public enum DrawingPack
{
    Mandala,
    Animal,
    Nature,
    Fantasy,
    Seasonal,
    Premium
}

[CreateAssetMenu(fileName = "NewDrawing", menuName = "Color Pixel/Drawing Data")]
public class DrawingData : ScriptableObject
{
    [Header("Assets")]
    public Texture2D outlineTexture;
    public Texture2D regionIdMap;
    public TextAsset paletteJson;

    [Header("Info")]
    public DrawingPack pack;
    public int levelNumber;
}
