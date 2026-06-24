using System.Collections;
using UnityEngine;

public class SettingsPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private bool hideOnStart = true;
    [SerializeField] private bool animateTransitions = true;
    [SerializeField] private float transitionSeconds = 0.18f;

    private CanvasGroup panelCanvasGroup;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        TryResolvePanelReference();
        EnsureCanvasGroup();
    }

    private void Start()
    {
        if (hideOnStart && settingsPanel != null)
        {
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }
            settingsPanel.SetActive(false);
        }
    }

    public void OpenSettings()
    {
        if (!EnsurePanel())
        {
            return;
        }

        SetSettingsState(true);
    }

    public void CloseSettings()
    {
        if (!EnsurePanel())
        {
            return;
        }

        SetSettingsState(false);
    }

    public void ToggleSettings()
    {
        if (!EnsurePanel())
        {
            return;
        }

        SetSettingsState(!settingsPanel.activeSelf);
    }

    private bool EnsurePanel()
    {
        if (settingsPanel != null)
        {
            EnsureCanvasGroup();
            return true;
        }

        TryResolvePanelReference();
        if (settingsPanel != null)
        {
            EnsureCanvasGroup();
            return true;
        }

        Debug.LogWarning("SettingsPanelUI: Settings panel is not assigned and could not be auto-found.");
        return false;
    }

    private void TryResolvePanelReference()
    {
        if (settingsPanel != null)
        {
            return;
        }

        // 1) If script is attached directly to SettingsPanel.
        if (gameObject.name == "SettingsPanel")
        {
            settingsPanel = gameObject;
            return;
        }

        // 2) Try find a panel named "SettingsPanel" in active scene.
        GameObject foundByName = GameObject.Find("SettingsPanel");
        if (foundByName != null)
        {
            settingsPanel = foundByName;
        }
    }

    private void EnsureCanvasGroup()
    {
        if (settingsPanel == null)
        {
            panelCanvasGroup = null;
            return;
        }

        panelCanvasGroup = settingsPanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = settingsPanel.AddComponent<CanvasGroup>();
        }
    }

    private void SetSettingsState(bool open)
    {
        if (!animateTransitions || panelCanvasGroup == null)
        {
            settingsPanel.SetActive(open);
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        transitionCoroutine = StartCoroutine(AnimatePanel(open));
    }

    private IEnumerator AnimatePanel(bool open)
    {
        if (settingsPanel == null || panelCanvasGroup == null)
        {
            yield break;
        }

        float duration = Mathf.Max(0.01f, transitionSeconds);
        if (open)
        {
            settingsPanel.SetActive(true);
        }

        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;

        float from = panelCanvasGroup.alpha;
        float to = open ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            panelCanvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        panelCanvasGroup.alpha = to;
        panelCanvasGroup.interactable = open;
        panelCanvasGroup.blocksRaycasts = open;

        if (!open)
        {
            settingsPanel.SetActive(false);
        }

        transitionCoroutine = null;
    }
}
