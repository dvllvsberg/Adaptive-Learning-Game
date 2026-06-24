using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpOverlayUI : MonoBehaviour
{
    [Serializable]
    private struct HelpPage
    {
        public string titleKey;
        public string bodyKey;

        public string title;

        [TextArea(4, 12)]
        public string body;
    }

    [Header("Panel References")]
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text pageCounterText;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private float panelFadeDuration = 0.18f;

    [Header("Pages")]
    [SerializeField] private HelpPage[] pages;

    private int currentPageIndex;
    private CanvasGroup panelCanvasGroup;
    private Coroutine panelFadeCoroutine;

    private void Awake()
    {
        NormalizeFullscreenRects();
    }

    private void OnEnable()
    {
        LanguageManager.LanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LanguageManager.LanguageChanged -= OnLanguageChanged;
    }

    private void Start()
    {
        NormalizeFullscreenRects();
        EnsurePanelCanvasGroup();

        if (helpPanel != null)
        {
            helpPanel.SetActive(false);
        }

        currentPageIndex = 0;
        RefreshPage();
    }

    public void OpenHelp()
    {
        NormalizeFullscreenRects();

        currentPageIndex = 0;
        RefreshPage();

        if (helpPanel != null)
        {
            EnsurePanelCanvasGroup();
            if (panelFadeCoroutine != null)
            {
                StopCoroutine(panelFadeCoroutine);
            }

            if (!helpPanel.activeSelf)
            {
                panelCanvasGroup.alpha = 0f;
            }

            helpPanel.SetActive(true);
            panelFadeCoroutine = StartCoroutine(FadePanel(1f, false));
        }
    }

    public void CloseHelp()
    {
        if (helpPanel != null)
        {
            EnsurePanelCanvasGroup();
            if (panelFadeCoroutine != null)
            {
                StopCoroutine(panelFadeCoroutine);
            }

            if (!isActiveAndEnabled)
            {
                helpPanel.SetActive(false);
                return;
            }

            panelFadeCoroutine = StartCoroutine(FadePanel(0f, true));
        }
    }

    public void NextPage()
    {
        if (pages == null || pages.Length == 0)
        {
            return;
        }

        if (currentPageIndex < pages.Length - 1)
        {
            currentPageIndex++;
            RefreshPage();
        }
    }

    public void PreviousPage()
    {
        if (pages == null || pages.Length == 0)
        {
            return;
        }

        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            RefreshPage();
        }
    }

    private void RefreshPage()
    {
        if (pages == null || pages.Length == 0)
        {
            if (titleText != null)
            {
                titleText.text = "Help";
                TMPTextAutoFit.ApplyTo(titleText, 18f, titleText.fontSize > 0f ? titleText.fontSize : 32f, true, TextOverflowModes.Ellipsis);
            }

            if (bodyText != null)
            {
                bodyText.text = "No help pages configured yet.";
                TMPTextAutoFit.ApplyTo(bodyText, 10f, bodyText.fontSize > 0f ? Mathf.Min(bodyText.fontSize, 22f) : 22f, true, TextOverflowModes.Ellipsis);
            }

            if (pageCounterText != null)
            {
                pageCounterText.text = "0/0";
                TMPTextAutoFit.ApplyTo(pageCounterText, 14f, pageCounterText.fontSize > 0f ? pageCounterText.fontSize : 24f, true, TextOverflowModes.Ellipsis);
            }

            if (previousButton != null)
            {
                previousButton.interactable = false;
            }

            if (nextButton != null)
            {
                nextButton.interactable = false;
            }

            return;
        }

        HelpPage page = pages[currentPageIndex];

        if (titleText != null)
        {
            titleText.text = ResolveLocalizedText(page.titleKey, page.title, "Help");
            TMPTextAutoFit.ApplyTo(titleText, 18f, titleText.fontSize > 0f ? titleText.fontSize : 32f, true, TextOverflowModes.Ellipsis);
        }

        if (bodyText != null)
        {
            bodyText.text = ResolveLocalizedText(page.bodyKey, page.body, "No help text available.");
            TMPTextAutoFit.ApplyTo(bodyText, 10f, bodyText.fontSize > 0f ? Mathf.Min(bodyText.fontSize, 22f) : 22f, true, TextOverflowModes.Ellipsis);
        }

        if (pageCounterText != null)
        {
            pageCounterText.text = $"{currentPageIndex + 1}/{pages.Length}";
            TMPTextAutoFit.ApplyTo(pageCounterText, 14f, pageCounterText.fontSize > 0f ? pageCounterText.fontSize : 24f, true, TextOverflowModes.Ellipsis);
        }

        if (previousButton != null)
        {
            previousButton.interactable = currentPageIndex > 0;
        }

        if (nextButton != null)
        {
            nextButton.interactable = currentPageIndex < pages.Length - 1;
        }
    }

    private string ResolveLocalizedText(string key, string fallback, string defaultText)
    {
        if (!string.IsNullOrWhiteSpace(key) && LanguageManager.Instance != null)
        {
            return LanguageManager.Instance.GetText(key, string.IsNullOrWhiteSpace(fallback) ? defaultText : fallback);
        }

        if (!string.IsNullOrWhiteSpace(fallback))
        {
            return fallback;
        }

        return defaultText;
    }

    private void OnLanguageChanged(AppLanguage _)
    {
        RefreshPage();
    }

    private void NormalizeFullscreenRects()
    {
        RectTransform panelRect = helpPanel != null ? helpPanel.transform as RectTransform : null;
        StretchToParent(panelRect);

        RectTransform parentRect = panelRect != null ? panelRect.parent as RectTransform : null;
        if (parentRect != null && parentRect.name == "MiniGameHUDRoot")
        {
            StretchToParent(parentRect);
        }
    }

    private static void StretchToParent(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    private void EnsurePanelCanvasGroup()
    {
        if (helpPanel == null)
        {
            panelCanvasGroup = null;
            return;
        }

        panelCanvasGroup = helpPanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = helpPanel.AddComponent<CanvasGroup>();
        }
    }

    private IEnumerator FadePanel(float targetAlpha, bool deactivateOnComplete)
    {
        if (helpPanel == null || panelCanvasGroup == null)
        {
            yield break;
        }

        float duration = Mathf.Max(0.01f, panelFadeDuration);
        float initialAlpha = panelCanvasGroup.alpha;
        float elapsed = 0f;

        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            panelCanvasGroup.alpha = Mathf.Lerp(initialAlpha, targetAlpha, t);
            yield return null;
        }

        panelCanvasGroup.alpha = targetAlpha;
        if (targetAlpha >= 0.99f)
        {
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }
        else if (deactivateOnComplete)
        {
            helpPanel.SetActive(false);
        }

        panelFadeCoroutine = null;
    }
}
