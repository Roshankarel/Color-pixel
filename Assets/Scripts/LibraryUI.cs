using UnityEngine;
using System.Linq;

public class LibraryUI : MonoBehaviour
{
    [Header("References")]
    public ProgressManager progressManager;
    public DrawingCatalog catalog;
    public Transform packTileContainer;
    public PackTileView packTilePrefab;
    public MainMenuBootstrap mainMenu;

    private void OnEnable()
    {
        BuildLibrary();
    }

    private void BuildLibrary()
    {
        // Remove old tiles
        foreach (Transform child in packTileContainer)
            Destroy(child.gameObject);

        // Get every pack that exists in the catalog
        var packs = catalog.drawings
            .Select(d => d.pack)
            .Distinct();

        foreach (var pack in packs)
        {
            PackTileView tile = Instantiate(packTilePrefab, packTileContainer);

            Debug.Log("Created Pack Tile: " + pack);
        }
    }
}