using TMPro;
using UnityEngine;

public class MiniGameSelectUI : SceneNavigationBase
{
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string colourSelectSceneName = "ColourSelect";
    [SerializeField] private string shapeSelectSceneName = "ShapeSelect";
    [SerializeField] private string numbersSelectSceneName = "NumbersSelect";

    [Header("Localization")]
    [SerializeField] private string titleKey = "menu.minigame_select.title";
    [SerializeField] private string colourButtonKey = "menu.minigame_select.colours";
    [SerializeField] private string shapeButtonKey = "menu.minigame_select.shapes";
    [SerializeField] private string numbersButtonKey = "menu.minigame_select.numbers";
    [SerializeField] private string backButtonKey = "ui.common.back";

    private TMP_Text titleText;
    private TMP_Text colourButtonText;
    private TMP_Text shapeButtonText;
    private TMP_Text numbersButtonText;
    private TMP_Text backButtonText;

    private void Start()
    {
        CacheTexts();
        RefreshLocalizedTexts();
    }

    private void OnEnable()
    {
        LanguageManager.LanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LanguageManager.LanguageChanged -= OnLanguageChanged;
    }

    public void OpenColourSection()
    {
        LoadScene(colourSelectSceneName, nameof(OpenColourSection));
    }

    public void OpenShapeSection()
    {
        LoadScene(shapeSelectSceneName, nameof(OpenShapeSection));
    }

    public void OpenNumbersSection()
    {
        LoadScene(numbersSelectSceneName, nameof(OpenNumbersSection));
    }

    public void BackToMainMenu()
    {
        LoadScene(mainMenuSceneName, nameof(BackToMainMenu));
    }

    private void CacheTexts()
    {
        titleText = FindTextByName("TitleText");
        colourButtonText = FindTextInside("ColoursButton");
        shapeButtonText = FindTextInside("ShapesButton");
        numbersButtonText = FindTextInside("NumbersButton");
        backButtonText = FindTextInside("BackToMenuButton");
    }

    private void RefreshLocalizedTexts()
    {
        SetText(titleText, GetLocalizedText(titleKey, "Choose Mini-Game Type"));
        SetText(colourButtonText, GetLocalizedText(colourButtonKey, "Colours"));
        SetText(shapeButtonText, GetLocalizedText(shapeButtonKey, "Shapes"));
        SetText(numbersButtonText, GetLocalizedText(numbersButtonKey, "Numbers"));
        SetText(backButtonText, GetLocalizedText(backButtonKey, "Back"));

        ConfigureTitleText(titleText);
        ConfigureButtonText(colourButtonText);
        ConfigureButtonText(shapeButtonText);
        ConfigureButtonText(numbersButtonText);
        ConfigureButtonText(backButtonText);
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

    private static void ConfigureTitleText(TMP_Text target)
    {
        ConfigureText(target, 24f, target != null ? target.fontSize : 72f, true, TextOverflowModes.Ellipsis);
    }

    private static void ConfigureButtonText(TMP_Text target)
    {
        ConfigureText(target, 14f, target != null ? target.fontSize : 50f, true, TextOverflowModes.Ellipsis);
    }

    private static void ConfigureText(TMP_Text target, float minFontSize, float maxFontSize, bool wordWrapping, TextOverflowModes overflowMode)
    {
        TMPTextAutoFit.ApplyTo(
            target,
            minFontSize,
            maxFontSize,
            wordWrapping,
            overflowMode);
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
