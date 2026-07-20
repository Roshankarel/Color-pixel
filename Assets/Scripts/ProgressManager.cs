using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    private const string SaveKey = "color_pixel_progress_v1";

    [SerializeField] private ProgressSaveData saveData = new ProgressSaveData();

    public event Action OnProgressChanged;
   

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public static ProgressManager EnsureExists()
    {
        if (Instance != null)
            return Instance;

        GameObject managerObject = new GameObject("ProgressManager");
        return managerObject.AddComponent<ProgressManager>();
    }

    public void InitializeFromCatalog(DrawingCatalog catalog)
    {
        EnsurePackEntries();

        if (catalog != null)
        {
            for (int i = 0; i < DrawingCatalog.PackOrder.Length; i++)
            {
                DrawingPack pack = DrawingCatalog.PackOrder[i];
                PackProgressData packData = GetOrCreatePackData(pack);
                int levelCount = catalog.GetLevelCount(pack);

                while (packData.levels.Count < levelCount)
                    packData.levels.Add(new LevelProgressData());

                if (pack == DrawingPack.Mandala && levelCount > 0)
                {
                    packData.isUnlocked = true;
                    if (packData.currentUnlockedLevelIndex < 0)
                        packData.currentUnlockedLevelIndex = 0;
                }
            }
        }

        Save();
    }

    public DrawingData GetCurrentPlayableDrawing(DrawingCatalog catalog)
    {
        if (catalog == null)
            return null;

        DrawingData candidate = null;

        for (int i = 0; i < DrawingCatalog.PackOrder.Length; i++)
        {
            DrawingPack pack = DrawingCatalog.PackOrder[i];
            if (!IsPackUnlocked(pack))
                continue;

            List<DrawingData> drawings = catalog.GetDrawings(pack);
            if (drawings.Count == 0)
                continue;

            PackProgressData packData = GetOrCreatePackData(pack);
            int index = packData.currentUnlockedLevelIndex;

            // Pack finished
            if (index >= drawings.Count)
                continue;

            candidate = drawings[index];
        }

        return candidate;
    }

    public bool IsPackUnlocked(DrawingPack pack)
    {
        return GetOrCreatePackData(pack).isUnlocked;
    }

    public bool IsPackPurchasable(DrawingCatalog catalog, DrawingPack pack)
    {
        if (pack == DrawingPack.Mandala || IsPackUnlocked(pack))
            return false;

        int packIndex = Array.IndexOf(DrawingCatalog.PackOrder, pack);
        if (packIndex <= 0)
            return false;

        DrawingPack previousPack = DrawingCatalog.PackOrder[packIndex - 1];
        return IsPackFinished(catalog, previousPack);
    }

    public bool IsPackFinished(DrawingCatalog catalog, DrawingPack pack)
    {
        if (catalog == null)
            return false;

        int levelCount = catalog.GetLevelCount(pack);
        if (levelCount <= 0)
            return false;

        for (int i = 0; i < levelCount; i++)
        {
            LevelProgressData level = GetLevelProgress(pack, i);
            if (level == null || (!level.completed && !level.skipped))
                return false;
        }

        return true;
    }

    public int GetCompletedOrSkippedCount(DrawingPack pack)
    {
        PackProgressData packData = GetOrCreatePackData(pack);
        int count = 0;

        for (int i = 0; i < packData.levels.Count; i++)
        {
            if (packData.levels[i].completed || packData.levels[i].skipped)
                count++;
        }

        return count;
    }

    public bool IsLevelUnlocked(DrawingPack pack, int levelIndex)
    {
        if (!IsPackUnlocked(pack))
            return false;

        PackProgressData packData = GetOrCreatePackData(pack);
        LevelProgressData level = GetLevelProgress(pack, levelIndex);

        if (level != null && (level.completed || level.skipped))
            return true;

        return levelIndex == packData.currentUnlockedLevelIndex;
    }

    public LevelProgressData GetLevelProgress(DrawingPack pack, int levelIndex)
    {
        if (levelIndex < 0)
            return null;

        PackProgressData packData = GetOrCreatePackData(pack);
        while (packData.levels.Count <= levelIndex)
            packData.levels.Add(new LevelProgressData());

        return packData.levels[levelIndex];
    }

    public int GetStarCount(DrawingPack pack, int levelIndex)
    {
        LevelProgressData level = GetLevelProgress(pack, levelIndex);
        return level == null ? 0 : Mathf.Clamp(level.starCount, 0, 3);
    }

    public void MarkLevelCompleted(DrawingPack pack, int levelIndex, int starCount)
    {
        Debug.Log($"MarkLevelCompleted: {pack} Level {levelIndex}");

        LevelProgressData level = GetLevelProgress(pack, levelIndex);
        if (level == null)
            return;

        level.completed = true;
        level.skipped = false;
        level.starCount = Mathf.Clamp(Mathf.Max(level.starCount, starCount), 0, 3);
        AdvanceUnlockedLevel(pack, levelIndex);
        SaveAndNotify();

        Debug.Log("Level marked as completed and progress saved.");
    }

    public bool IsLevelCompleted(DrawingPack pack, int levelIndex)
    {
        LevelProgressData level = GetLevelProgress(pack, levelIndex);

        return level != null && level.completed;
    }

    public void MarkLevelSkipped(DrawingPack pack, int levelIndex)
    {
        LevelProgressData level = GetLevelProgress(pack, levelIndex);
        if (level == null)
            return;

        level.skipped = true;
        AdvanceUnlockedLevel(pack, levelIndex);
        SaveAndNotify();
    }

    public void UnlockPackForFutureEconomy(DrawingPack pack)
    {
        PackProgressData packData = GetOrCreatePackData(pack);
        packData.isUnlocked = true;
        if (packData.currentUnlockedLevelIndex < 0)
            packData.currentUnlockedLevelIndex = 0;

        SaveAndNotify();
    }

    private void AdvanceUnlockedLevel(DrawingPack pack, int completedLevelIndex)
    {
        PackProgressData packData = GetOrCreatePackData(pack);
        if (packData.currentUnlockedLevelIndex <= completedLevelIndex)
            packData.currentUnlockedLevelIndex = completedLevelIndex + 1;
    }

    private void SaveAndNotify()
    {
        Save();
        OnProgressChanged?.Invoke();
    }

    private PackProgressData GetOrCreatePackData(DrawingPack pack)
    {
        EnsurePackEntries();
        int packValue = (int)pack;

        for (int i = 0; i < saveData.packs.Count; i++)
        {
            if (saveData.packs[i].pack == packValue)
                return saveData.packs[i];
        }

        PackProgressData packData = CreatePackData(pack);
        saveData.packs.Add(packData);
        return packData;
    }

    private void EnsurePackEntries()
    {
        if (saveData == null)
            saveData = new ProgressSaveData();

        if (saveData.packs == null)
            saveData.packs = new List<PackProgressData>();

        for (int i = 0; i < DrawingCatalog.PackOrder.Length; i++)
            GetOrCreatePackDataWithoutEnsure(DrawingCatalog.PackOrder[i]);
    }

    private PackProgressData GetOrCreatePackDataWithoutEnsure(DrawingPack pack)
    {
        int packValue = (int)pack;
        for (int i = 0; i < saveData.packs.Count; i++)
        {
            if (saveData.packs[i].pack == packValue)
                return saveData.packs[i];
        }

        PackProgressData packData = CreatePackData(pack);
        saveData.packs.Add(packData);
        return packData;
    }

    private PackProgressData CreatePackData(DrawingPack pack)
    {
        return new PackProgressData
        {
            pack = (int)pack,
            isUnlocked = pack == DrawingPack.Mandala,
            currentUnlockedLevelIndex = pack == DrawingPack.Mandala ? 0 : -1,
            levels = new List<LevelProgressData>()
        };
    }
    #region Coins

    public int GetCoins()
    {
        return saveData.coins;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        saveData.coins += amount;

        Save();
        OnProgressChanged?.Invoke();

        Debug.Log($"Coins = {saveData.coins}");
    }

    public bool SpendCoins(int amount)
    {
        if (saveData.coins < amount)
            return false;

        saveData.coins -= amount;

        Save();
        OnProgressChanged?.Invoke();

        Debug.Log($"Coins = {saveData.coins}");

        return true;
    }

    #endregion

    private void Load()
    {
        string json = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                saveData = JsonUtility.FromJson<ProgressSaveData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("ProgressManager: Failed to load progress JSON. Starting fresh. " + exception.Message);
                saveData = new ProgressSaveData();
            }
        }

        EnsurePackEntries();
    }

    public DrawingData GetNextDrawing(DrawingCatalog catalog, DrawingData currentDrawing)
    {
        if (catalog == null || currentDrawing == null)
            return null;

        bool foundCurrent = false;

        foreach (DrawingPack pack in DrawingCatalog.PackOrder)
        {
            List<DrawingData> drawings = catalog.GetDrawings(pack);

            foreach (DrawingData drawing in drawings)
            {
                if (foundCurrent)
                    return drawing;

                if (drawing == currentDrawing)
                    foundCurrent = true;
            }
        }

        return null;
    }

    private void Save()
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    

}

[Serializable]
public class ProgressSaveData
{
    public int coins = 0;

    public List<PackProgressData> packs = new List<PackProgressData>();
}

[Serializable]
public class PackProgressData
{
    public int pack;
    public bool isUnlocked;
    public int currentUnlockedLevelIndex;
    public List<LevelProgressData> levels = new List<LevelProgressData>();
}

[Serializable]
public class LevelProgressData
{
    public int starCount;
    public bool completed;
    public bool skipped;
}
