using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTitleTextAuto : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private bool refreshOnEnable = true;
    [SerializeField] private bool useCustomTitle;
    [SerializeField] private string customTitle = "";

    private static readonly Regex LowerToUpperRegex = new Regex("([a-z])([A-Z])");
    private static readonly Regex LetterToDigitRegex = new Regex("([A-Za-z])([0-9])");

    private void Awake()
    {
        UpdateTitle();
    }

    private void OnEnable()
    {
        if (refreshOnEnable)
        {
            UpdateTitle();
        }
    }

    public void UpdateTitle()
    {
        if (titleText == null)
        {
            Debug.LogWarning("SceneTitleTextAuto: Title Text is not assigned.");
            return;
        }

        if (useCustomTitle && !string.IsNullOrWhiteSpace(customTitle))
        {
            titleText.text = customTitle.Trim();
            TMPTextAutoFit.ApplyTo(titleText, 18f, titleText.fontSize > 0f ? titleText.fontSize : 40f, true, TextOverflowModes.Ellipsis);
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        titleText.text = FormatSceneName(sceneName);
        TMPTextAutoFit.ApplyTo(titleText, 18f, titleText.fontSize > 0f ? titleText.fontSize : 40f, true, TextOverflowModes.Ellipsis);
    }

    private string FormatSceneName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "Untitled Scene";
        }

        string value = raw.Replace("_", " ").Replace("-", " ").Trim();
        value = LowerToUpperRegex.Replace(value, "$1 $2");
        value = LetterToDigitRegex.Replace(value, "$1 $2");

        return value;
    }
}
