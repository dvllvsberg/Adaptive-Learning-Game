using System.Collections;
using TMPro;
using UnityEngine;

public class MainMenuUI : SceneNavigationBase
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject mainButtonsGroup;
    [SerializeField] private string miniGameSelectSceneName = "MiniGameSelect";
    [Header("Polish")]
    [SerializeField] private bool animatePanelTransitions = true;
    [SerializeField] private float panelTransitionSeconds = 0.18f;

    [Header("Localization")]
    [SerializeField] private string playButtonKey = "ui.common.play";
    [SerializeField] private string tutorialButtonKey = "ui.common.tutorial";
    [SerializeField] private string exitButtonKey = "ui.common.exit";
    [SerializeField] private string backButtonKey = "ui.common.back";
    [SerializeField] private string closeButtonKey = "ui.common.close";
    [SerializeField] private string settingsButtonKey = "ui.common.settings";
    [SerializeField] private string settingsTitleKey = "ui.settings.title";
    [SerializeField] private string languageLabelKey = "ui.common.language";
    [SerializeField] private string tutorialBodyKey = "menu.main.tutorial.body";

    private TMP_Text playButtonText;
    private TMP_Text tutorialButtonText;
    private TMP_Text exitButtonText;
    private TMP_Text backButtonText;
    private TMP_Text closeButtonText;
    private TMP_Text settingsButtonText;
    private TMP_Text settingsTitleText;
    private TMP_Text languageLabelText;
    private TMP_Text tutorialBodyText;
    private CanvasGroup tutorialCanvasGroup;
    private CanvasGroup mainButtonsCanvasGroup;
    private Coroutine panelTransitionCoroutine;

    private void Start()
    {
        EnsureCanvasGroups();
        CacheTexts();
        RefreshLocalizedTexts();

        // Hide tutorial by default and keep main buttons visible.
        SetTutorialState(false, true);
    }

    private void OnEnable()
    {
        LanguageManager.LanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LanguageManager.LanguageChanged -= OnLanguageChanged;
    }

    public void OpenTutorial()
    {
        SetTutorialState(true);
    }

    public void CloseTutorial()
    {
        SetTutorialState(false);
    }

    public void Play()
    {
        LoadScene(miniGameSelectSceneName, nameof(Play));
    }

    public void ExitGame()
    {
        QuitApplication(nameof(ExitGame));
    }

    private void SetTutorialState(bool isOpen, bool immediate = false)
    {
        if (panelTransitionCoroutine != null)
        {
            StopCoroutine(panelTransitionCoroutine);
            panelTransitionCoroutine = null;
        }

        if (!animatePanelTransitions || immediate)
        {
            ApplyTutorialStateImmediate(isOpen);
            return;
        }

        if (tutorialCanvasGroup == null || mainButtonsCanvasGroup == null)
        {
            ApplyTutorialStateImmediate(isOpen);
            return;
        }

        panelTransitionCoroutine = StartCoroutine(AnimateTutorialState(isOpen));
    }

    private void EnsureCanvasGroups()
    {
        tutorialCanvasGroup = EnsureCanvasGroup(tutorialPanel);
        mainButtonsCanvasGroup = EnsureCanvasGroup(mainButtonsGroup);
    }

    private static CanvasGroup EnsureCanvasGroup(GameObject target)
    {
        if (target == null)
        {
            return null;
        }

        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = target.AddComponent<CanvasGroup>();
        }

        return group;
    }

    private void ApplyTutorialStateImmediate(bool isOpen)
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(isOpen);
        }

        if (mainButtonsGroup != null)
        {
            mainButtonsGroup.SetActive(!isOpen);
        }
    }

    private IEnumerator AnimateTutorialState(bool openTutorial)
    {
        if (tutorialPanel == null || mainButtonsGroup == null || tutorialCanvasGroup == null || mainButtonsCanvasGroup == null)
        {
            ApplyTutorialStateImmediate(openTutorial);
            yield break;
        }

        float duration = Mathf.Max(0.01f, panelTransitionSeconds);

        if (openTutorial)
        {
            tutorialPanel.SetActive(true);
        }
        else
        {
            mainButtonsGroup.SetActive(true);
        }

        tutorialCanvasGroup.interactable = false;
        tutorialCanvasGroup.blocksRaycasts = false;
        mainButtonsCanvasGroup.interactable = false;
        mainButtonsCanvasGroup.blocksRaycasts = false;

        float tutorialFrom = tutorialCanvasGroup.alpha;
        float tutorialTo = openTutorial ? 1f : 0f;
        float buttonsFrom = mainButtonsCanvasGroup.alpha;
        float buttonsTo = openTutorial ? 0f : 1f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            tutorialCanvasGroup.alpha = Mathf.Lerp(tutorialFrom, tutorialTo, t);
            mainButtonsCanvasGroup.alpha = Mathf.Lerp(buttonsFrom, buttonsTo, t);
            yield return null;
        }

        tutorialCanvasGroup.alpha = tutorialTo;
        mainButtonsCanvasGroup.alpha = buttonsTo;

        tutorialPanel.SetActive(openTutorial);
        mainButtonsGroup.SetActive(!openTutorial);

        tutorialCanvasGroup.interactable = openTutorial;
        tutorialCanvasGroup.blocksRaycasts = openTutorial;
        mainButtonsCanvasGroup.interactable = !openTutorial;
        mainButtonsCanvasGroup.blocksRaycasts = !openTutorial;

        panelTransitionCoroutine = null;
    }

    private void CacheTexts()
    {
        playButtonText = FindTextInside("PlayButton");
        tutorialButtonText = FindTextInside("TutorialButton");
        exitButtonText = FindTextInside("ExitButton");
        backButtonText = FindTextInside("BackButton");
        closeButtonText = FindTextInside("CloseButton");
        settingsButtonText = FindTextInside("SettingsButton");
        tutorialBodyText = FindTextByName("TutorialText");

        GameObject settingsPanelObject = FindChildByNameInScene("SettingsPanel");
        if (settingsPanelObject != null)
        {
            settingsTitleText = FindTextByName("TitleText", settingsPanelObject.transform);
            languageLabelText = FindTextByName("LanguageLabel", settingsPanelObject.transform);
        }
    }

    private void RefreshLocalizedTexts()
    {
        SetText(playButtonText, GetLocalizedText(playButtonKey, "Play"));
        SetText(tutorialButtonText, GetLocalizedText(tutorialButtonKey, "Tutorial"));
        SetText(exitButtonText, GetLocalizedText(exitButtonKey, "Exit"));
        SetText(backButtonText, GetLocalizedText(backButtonKey, "Back"));
        SetText(closeButtonText, GetLocalizedText(closeButtonKey, "Close"));
        SetText(settingsButtonText, GetLocalizedText(settingsButtonKey, "Settings"));
        SetText(settingsTitleText, GetLocalizedText(settingsTitleKey, "Settings"));
        SetText(languageLabelText, GetLocalizedText(languageLabelKey, "Language"));
        SetText(tutorialBodyText, GetLocalizedText(
            tutorialBodyKey,
            "This is an educational game for cognitive development.\n\nSkills developed:\n- Attention\n- Memory\n- Logic\n\nMini-games:\n1) Attention game - find the right objects or symbols in time.\n2) Memory game - remember and repeat sequences.\n3) Logic game - solve simple logical tasks."));

        ConfigureButtonText(playButtonText);
        ConfigureButtonText(tutorialButtonText);
        ConfigureButtonText(exitButtonText);
        ConfigureButtonText(backButtonText);
        ConfigureButtonText(closeButtonText);
        ConfigureButtonText(settingsButtonText);
        ConfigureHeaderText(settingsTitleText);
        ConfigureHeaderText(languageLabelText);
        ConfigureTutorialBodyText(tutorialBodyText);
    }

    private void OnLanguageChanged(AppLanguage _)
    {
        RefreshLocalizedTexts();
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

    private static void ConfigureButtonText(TMP_Text target)
    {
        ConfigureText(target, 14f, 50f, true, TextOverflowModes.Ellipsis);
    }

    private static void ConfigureHeaderText(TMP_Text target)
    {
        ConfigureText(target, 20f, target != null ? target.fontSize : 40f, true, TextOverflowModes.Ellipsis);
    }

    private static void ConfigureTutorialBodyText(TMP_Text target)
    {
        ConfigureText(target, 16f, target != null ? target.fontSize : 32f, true, TextOverflowModes.Overflow, true, 180f, 8f);
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

    private TMP_Text FindTextByName(string objectName, Transform root)
    {
        GameObject targetObject = FindChildByName(root, objectName);
        if (targetObject == null)
        {
            return null;
        }

        return targetObject.GetComponent<TMP_Text>() ?? targetObject.GetComponentInChildren<TMP_Text>(true);
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
}