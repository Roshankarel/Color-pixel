using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.RemoteConfig;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    private bool _isInitialized = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeFirebase();
    }

    private async void InitializeFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            _isInitialized = true;
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            CrashlyticsSetup();
            await FetchRemoteConfig();
            Debug.Log("[Firebase] Initialized successfully");
        }
        else
        {
            Debug.LogError($"[Firebase] Failed: {dependencyStatus}");
        }
    }

    private void CrashlyticsSetup()
    {
        Crashlytics.ReportUncaughtExceptionsAsFatal = true;
    }

    // --- Analytics Events ---

    public void LogLevelComplete(string levelId, int stars, bool hintUsed, bool skipUsed)
    {
        if (!_isInitialized) return;
        FirebaseAnalytics.LogEvent("level_complete",
            new Parameter("level_id", levelId),
            new Parameter("star_count", stars),
            new Parameter("hint_used", hintUsed ? 1 : 0),
            new Parameter("skip_used", skipUsed ? 1 : 0));
    }

    public void LogAdWatched(string adType, string placement)
    {
        if (!_isInitialized) return;
        FirebaseAnalytics.LogEvent("ad_watched",
            new Parameter("ad_type", adType),
            new Parameter("placement", placement));
    }

    public void LogPackUnlocked(string packName, string method)
    {
        if (!_isInitialized) return;
        FirebaseAnalytics.LogEvent("pack_unlocked",
            new Parameter("pack_name", packName),
            new Parameter("method", method));
    }

    public void LogSessionStart()
    {
        if (!_isInitialized) return;
        FirebaseAnalytics.LogEvent("session_start");
    }

    // --- Remote Config ---

    private async Task FetchRemoteConfig()
    {
        var defaults = new Dictionary<string, object>
        {
            { "interstitial_frequency", 3 },
            { "coins_per_rv", 50 },
            { "hint_cost_coins", 10 }
        };

        var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        await remoteConfig.SetDefaultsAsync(defaults);
        await remoteConfig.FetchAndActivateAsync();
        Debug.Log("[Firebase] Remote Config fetched");
    }

    public int GetInt(string key) =>
        (int)FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
}