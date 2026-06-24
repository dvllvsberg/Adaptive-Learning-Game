using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LanguageManager : MonoBehaviour
{
    private const string LanguagePrefsKey = "app_language";
    private const AppLanguage InitialLanguage = AppLanguage.Russian;

    [SerializeField] private LocalizationDatabase localizationDatabase;

    public static LanguageManager Instance { get; private set; }
    public static event Action<AppLanguage> LanguageChanged;

    public AppLanguage CurrentLanguage { get; private set; }

    private void Awake()
    {
        AppLanguage languageToUse = ResolveStartupLanguage();

        if (Instance != null && Instance != this)
        {
            // Replace stale singleton with current scene instance so UI button references
            // in newly loaded scenes remain valid.
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentLanguage = languageToUse;
    }

    private void Start()
    {
        // Notify all listeners once on startup.
        NotifyLanguageChanged();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetLanguage(AppLanguage language)
    {
        CurrentLanguage = language;

        PlayerPrefs.SetInt(LanguagePrefsKey, (int)language);
        PlayerPrefs.Save();

        NotifyLanguageChanged();
    }

    public void SetRussian()
    {
        SetLanguage(AppLanguage.Russian);
    }

    public void SetKazakh()
    {
        SetLanguage(AppLanguage.Kazakh);
    }

    public void SetEnglish()
    {
        SetLanguage(AppLanguage.English);
    }

    public string GetText(string key, string fallback = "")
    {
        if (localizationDatabase == null)
        {
            return string.IsNullOrWhiteSpace(fallback) ? key : fallback;
        }

        return localizationDatabase.GetText(key, CurrentLanguage, fallback);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(NotifyLanguageChangedNextFrame());
    }

    private IEnumerator NotifyLanguageChangedNextFrame()
    {
        yield return null;
        NotifyLanguageChanged();
    }

    private AppLanguage ResolveStartupLanguage()
    {
        if (PlayerPrefs.HasKey(LanguagePrefsKey))
        {
            int savedLanguage = PlayerPrefs.GetInt(LanguagePrefsKey);
            if (Enum.IsDefined(typeof(AppLanguage), savedLanguage))
            {
                return (AppLanguage)savedLanguage;
            }
        }

        if (Instance != null && Instance != this)
        {
            return Instance.CurrentLanguage;
        }

        return InitialLanguage;
    }

    private void NotifyLanguageChanged()
    {
        LanguageChanged?.Invoke(CurrentLanguage);
    }
}
