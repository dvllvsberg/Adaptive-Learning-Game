using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStyleApplier : MonoBehaviour
{
    [SerializeField] private UIStyleConfig style;
    [SerializeField] private bool applyOnStart = true;

    [Header("Scene References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image[] panelImages;
    [SerializeField] private Button[] buttons;
    [SerializeField] private TMP_Text[] titleTexts;
    [SerializeField] private TMP_Text[] bodyTexts;
    [SerializeField] private TMP_Text[] buttonTexts;

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyStyle();
        }
    }

    public void ApplyStyle()
    {
        if (style == null)
        {
            Debug.LogWarning("UIStyleApplier: style config is not assigned.");
            return;
        }

        ApplyBackground();
        ApplyPanels();
        ApplyButtons();
        ApplyTexts();
    }

    private void ApplyBackground()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = style.backgroundColor;
        }
    }

    private void ApplyPanels()
    {
        if (panelImages == null)
        {
            return;
        }

        foreach (Image panelImage in panelImages)
        {
            if (panelImage != null)
            {
                panelImage.color = style.panelColor;
            }
        }
    }

    private void ApplyButtons()
    {
        if (buttons == null)
        {
            return;
        }

        foreach (Button button in buttons)
        {
            if (button == null)
            {
                continue;
            }

            ColorBlock colors = button.colors;
            colors.normalColor = style.buttonNormalColor;
            colors.highlightedColor = style.buttonHighlightedColor;
            colors.pressedColor = style.buttonPressedColor;
            colors.selectedColor = style.buttonSelectedColor;
            colors.disabledColor = style.buttonDisabledColor;
            colors.fadeDuration = style.buttonFadeDuration;
            button.colors = colors;

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null && style.buttonSize.x > 0f && style.buttonSize.y > 0f)
            {
                rect.sizeDelta = style.buttonSize;
            }
        }
    }

    private void ApplyTexts()
    {
        ApplyTextGroup(titleTexts, style.titleTextColor, style.titleTextSize);
        ApplyTextGroup(bodyTexts, style.bodyTextColor, style.bodyTextSize);
        ApplyTextGroup(buttonTexts, style.buttonTextColor, style.buttonTextSize);
    }

    private static void ApplyTextGroup(TMP_Text[] texts, Color color, float size)
    {
        if (texts == null)
        {
            return;
        }

        foreach (TMP_Text textItem in texts)
        {
            if (textItem == null)
            {
                continue;
            }

            textItem.color = color;
            textItem.fontSize = size;
        }
    }
}
