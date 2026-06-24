using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedTMPText : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private string localizationKey;
    [TextArea(1, 3)]
    [SerializeField] private string fallbackText;
    [Header("Default Auto Fit")]
    [SerializeField] private bool useDefaultAutoFit = true;
    [SerializeField] private float defaultMinFontSize = 14f;
    [SerializeField] private bool defaultEnableWordWrapping = true;
    [SerializeField] private TextOverflowModes defaultOverflowMode = TextOverflowModes.Ellipsis;

    private void Reset()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        LanguageManager.LanguageChanged += OnLanguageChanged;
        Refresh();
    }

    private void OnDisable()
    {
        LanguageManager.LanguageChanged -= OnLanguageChanged;
    }

    public void Refresh()
    {
        if (targetText == null)
        {
            return;
        }

        if (LanguageManager.Instance != null && !string.IsNullOrWhiteSpace(localizationKey))
        {
            targetText.text = LanguageManager.Instance.GetText(localizationKey, fallbackText);
        }
        else if (!string.IsNullOrWhiteSpace(fallbackText))
        {
            targetText.text = fallbackText;
        }

        ApplyAutoFitIfPresent();
    }

    private void OnLanguageChanged(AppLanguage _)
    {
        Refresh();
    }

    private void ApplyAutoFitIfPresent()
    {
        TMPTextAutoFit autoFit = GetComponent<TMPTextAutoFit>();
        if (autoFit != null)
        {
            autoFit.Apply();
            LayoutRebuilder.ForceRebuildLayoutImmediate(targetText.rectTransform);
            return;
        }

        if (useDefaultAutoFit && targetText != null)
        {
            TMPTextAutoFit.ApplyTo(
                targetText,
                defaultMinFontSize,
                Mathf.Max(defaultMinFontSize, targetText.fontSize > 0f ? targetText.fontSize : 36f),
                defaultEnableWordWrapping,
                defaultOverflowMode);
        }
    }
}
