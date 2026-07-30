using UnityEngine;

public class PremiumManager : MonoBehaviour
{
    public static PremiumManager Instance { get; private set; }

    [SerializeField] private bool hasPremium = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasPremium()
    {
        return hasPremium;
    }

    public void UnlockPremium()
    {
        hasPremium = true;
    }

    public static PremiumManager EnsureExists()
    {
        if (Instance != null)
            return Instance;

        GameObject managerObject = new GameObject("PremiumManager");
        return managerObject.AddComponent<PremiumManager>();
    }
}