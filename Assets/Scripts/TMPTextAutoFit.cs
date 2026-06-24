using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TMPTextAutoFit : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private bool applyOnEnable = true;
    [SerializeField] private bool reapplyOnRectTransformChange = true;
    [SerializeField] private bool enableAutoSizing = true;
    [SerializeField] private float minFontSize = 18f;
    [SerializeField] private float maxFontSize = 36f;
    [SerializeField] private bool enableWordWrapping = true;
    [SerializeField] private TextOverflowModes overflowMode = TextOverflowModes.Ellipsis;
    [SerializeField] private bool resizeTextHeightToPreferred;
    [SerializeField] private float minPreferredHeight;
    [SerializeField] private float extraHeightPadding = 4f;

    private void Reset()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        if (applyOnEnable)
        {
            Apply();
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        if (reapplyOnRectTransformChange && isActiveAndEnabled)
        {
            Apply();
        }
    }

    public void Apply()
    {
        ApplyTo(
            targetText,
            minFontSize,
            maxFontSize,
            enableWordWrapping,
            overflowMode,
            enableAutoSizing,
            resizeTextHeightToPreferred,
            minPreferredHeight,
            extraHeightPadding);
    }

    public static void ApplyTo(
        TMP_Text targetText,
        float minFontSize,
        float maxFontSize,
        bool enableWordWrapping,
        TextOverflowModes overflowMode,
        bool enableAutoSizing = true,
        bool resizeTextHeightToPreferred = false,
        float minPreferredHeight = 0f,
        float extraHeightPadding = 0f)
    {
        if (targetText == null)
        {
            return;
        }

        targetText.enableAutoSizing = enableAutoSizing;
        targetText.fontSizeMin = Mathf.Min(minFontSize, maxFontSize);
        targetText.fontSizeMax = Mathf.Max(minFontSize, maxFontSize);
        targetText.enableWordWrapping = enableWordWrapping;
        targetText.overflowMode = overflowMode;
        targetText.ForceMeshUpdate();

        RectTransform rectTransform = targetText.rectTransform;
        if (rectTransform == null)
        {
            return;
        }

        if (resizeTextHeightToPreferred)
        {
            float width = rectTransform.rect.width;
            if (width <= 0.01f)
            {
                width = Mathf.Max(1f, rectTransform.sizeDelta.x);
            }

            Vector2 preferred = targetText.GetPreferredValues(targetText.text, width, 0f);
            float preferredHeight = Mathf.Max(minPreferredHeight, preferred.y + Mathf.Max(0f, extraHeightPadding));
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}
