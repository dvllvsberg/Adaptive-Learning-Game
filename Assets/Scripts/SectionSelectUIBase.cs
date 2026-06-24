using TMPro;
using UnityEngine;

public abstract class SectionSelectUIBase : SceneNavigationBase
{
    [SerializeField] private string miniGameSelectSceneName = "MiniGameSelect";
    [SerializeField] private GameObject comingSoonPanel;

    [Header("Localization")]
    [SerializeField] private string titleKey = "menu.section_select.title";
    [SerializeField] private string game1Key = "menu.section_select.game1";
    [SerializeField] private string backKey = "ui.common.back";
    [SerializeField] private string comingSoonKey = "menu.section_select.coming_soon";
    [SerializeField] private string comingSoonBodyKey = "menu.section_select.coming_soon.body";
    [SerializeField] private string okKey = "ui.common.ok";

    private TMP_Text titleText;
    private TMP_Text game1Text;
    private TMP_Text backText;
    private TMP_Text comingSoonTitleText;
    private TMP_Text comingSoonBodyText;
    private TMP_Text okText;

    protected virtual void Start()
    {
        CacheTexts();
        RefreshLocalizedTexts();
        HideComingSoon();
    }

    protected virtual void OnEnable()
    {
        LanguageManager.LanguageChanged += OnLanguageChanged;
    }

    protected virtual void OnDisable()
    {
        LanguageManager.LanguageChanged -= OnLanguageChanged;
    }

    public void BackToMiniGameSelect()
    {
        LoadScene(miniGameSelectSceneName, nameof(BackToMiniGameSelect));
    }

    public void ShowComingSoon()
    {
        if (comingSoonPanel != null)
        {
            comingSoonPanel.SetActive(true);
            return;
        }

        Debug.Log("Coming soon: this mini-game is still in development.");
    }

    public void HideComingSoon()
    {
        if (comingSoonPanel != null)
        {
            comingSoonPanel.SetActive(false);
        }
    }

    private void OnLanguageChanged(AppLanguage _)
    {
        RefreshLocalizedTexts();
    }

    private void CacheTexts()
    {
        titleText = FindTextWithContent("Choose Mini-Game") ?? FindTextByName("TitleText");
        game1Text = FindTextInside("Game1Button");
        backText = FindTextInside("BackButton") ?? FindTextInside("Back");
        okText = FindTextInside("OkButton");

        if (comingSoonPanel != null)
        {
            TMP_Text[] panelTexts = comingSoonPanel.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < panelTexts.Length; i++)
            {
                TMP_Text textItem = panelTexts[i];
                if (textItem == null)
                {
                    continue;
                }

                if (okText == null && IsInsideObjectNamed(textItem.transform, "OkButton"))
                {
                    okText = textItem;
                    continue;
                }

                if (comingSoonTitleText == null && ContainsText(textItem, "Coming Soon"))
                {
                    comingSoonTitleText = textItem;
                    continue;
                }

                if (comingSoonBodyText == null && ContainsText(textItem, "under development"))
                {
                    comingSoonBodyText = textItem;
                }
            }
        }

        comingSoonTitleText ??= FindTextWithContent("Coming Soon");
        comingSoonBodyText ??= FindTextWithContent("under development");
    }

    private void RefreshLocalizedTexts()
    {
        SetText(titleText, GetLocalizedText(titleKey, "Choose Mini-Game"));
        SetText(game1Text, GetLocalizedText(game1Key, "Game 1"));
        SetText(backText, GetLocalizedText(backKey, "Back"));
        SetText(comingSoonTitleText, GetLocalizedText(comingSoonKey, "Coming Soon"));
        SetText(comingSoonBodyText, GetLocalizedText(
            comingSoonBodyKey,
            "This game is under development.\nYou will be able to play it very soon."));
        SetText(okText, GetLocalizedText(okKey, "OK"));

        ConfigureTitleText(titleText);
        ConfigureButtonText(game1Text);
        ConfigureButtonText(backText);
        ConfigureButtonText(comingSoonTitleText);
        ConfigureButtonText(okText);
        ConfigureBodyText(comingSoonBodyText);
    }

    private string GetLocalizedText(string key, string fallback)
    {
        if (LanguageManager.Instance != null)
        {
            return LanguageManager.Instance.GetText(key, fallback);
        }

        return fallback;
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target == null)
        {
            return;
        }

        target.text = value;
    }

    private static void ConfigureTitleText(TMP_Text target)
    {
        ConfigureText(target, 24f, target != null ? target.fontSize : 72f, true, TextOverflowModes.Ellipsis);
    }

    private static void ConfigureButtonText(TMP_Text target)
    {
        ConfigureText(target, 14f, target != null ? target.fontSize : 50f, true, TextOverflowModes.Ellipsis);
    }

    private static void ConfigureBodyText(TMP_Text target)
    {
        ConfigureText(target, 12f, target != null ? Mathf.Min(target.fontSize, 26f) : 26f, true, TextOverflowModes.Ellipsis);
    }

    private static void ConfigureText(
        TMP_Text target,
        float minFontSize,
        float maxFontSize,
        bool wordWrapping,
        TextOverflowModes overflowMode,
        bool resizeHeightToPreferred = false,
        float minPreferredHeight = 0f,
        float extraHeightPadding = 0f)
    {
        TMPTextAutoFit.ApplyTo(
            target,
            minFontSize,
            maxFontSize,
            wordWrapping,
            overflowMode,
            true,
            resizeHeightToPreferred,
            minPreferredHeight,
            extraHeightPadding);
    }

    private TMP_Text FindTextInside(string objectName)
    {
        GameObject targetObject = FindChildByNameInScene(objectName);
        if (targetObject == null)
        {
            return null;
        }

        return targetObject.GetComponentInChildren<TMP_Text>(true);
    }

    private TMP_Text FindTextByName(string objectName)
    {
        GameObject targetObject = FindChildByNameInScene(objectName);
        if (targetObject == null)
        {
            return null;
        }

        return targetObject.GetComponent<TMP_Text>() ?? targetObject.GetComponentInChildren<TMP_Text>(true);
    }

    private TMP_Text FindTextWithContent(string contentPart)
    {
        TMP_Text[] texts = GetAllSceneTexts();

        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text textItem = texts[i];
            if (ContainsText(textItem, contentPart))
            {
                return textItem;
            }
        }

        return null;
    }

    private TMP_Text[] GetAllSceneTexts()
    {
        GameObject[] rootObjects = gameObject.scene.GetRootGameObjects();
        System.Collections.Generic.List<TMP_Text> result = new System.Collections.Generic.List<TMP_Text>();

        for (int i = 0; i < rootObjects.Length; i++)
        {
            result.AddRange(rootObjects[i].GetComponentsInChildren<TMP_Text>(true));
        }

        return result.ToArray();
    }

    private bool ContainsText(TMP_Text textItem, string contentPart)
    {
        return textItem != null &&
               !string.IsNullOrWhiteSpace(textItem.text) &&
               textItem.text.IndexOf(contentPart, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private bool IsInsideObjectNamed(Transform current, string objectName)
    {
        while (current != null)
        {
            if (current.name == objectName)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private GameObject FindChildByNameInScene(string objectName)
    {
        GameObject[] rootObjects = gameObject.scene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            GameObject result = FindChildByName(rootObjects[i].transform, objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private GameObject FindChildByName(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == objectName)
        {
            return root.gameObject;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            GameObject result = FindChildByName(root.GetChild(i), objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

}
