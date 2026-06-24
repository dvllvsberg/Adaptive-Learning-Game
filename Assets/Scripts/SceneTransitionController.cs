using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionController : MonoBehaviour
{
    private static SceneTransitionController instance;

    [Header("Transition")]
    [SerializeField] private float fadeOutSeconds = 0.24f;
    [SerializeField] private float fadeInSeconds = 0.36f;
    [SerializeField] private float holdBlackSecondsAfterLoad = 0.08f;
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 1f);

    private Canvas overlayCanvas;
    private Image fadeImage;
    private bool isTransitioning;

    public static void TransitionToScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        SceneTransitionController controller = GetOrCreate();
        controller.StartCoroutine(controller.TransitionRoutine(sceneName));
    }

    private static SceneTransitionController GetOrCreate()
    {
        if (instance != null)
        {
            return instance;
        }

        SceneTransitionController existing = FindObjectOfType<SceneTransitionController>();
        if (existing != null)
        {
            instance = existing;
            instance.Initialize();
            return instance;
        }

        GameObject root = new GameObject("SceneTransitionController");
        instance = root.AddComponent<SceneTransitionController>();
        instance.Initialize();
        return instance;
    }

    private void Initialize()
    {
        if (instance == null)
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
        EnsureOverlay(true);
    }

    private void Awake()
    {
        Initialize();
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        if (isTransitioning)
        {
            yield break;
        }

        isTransitioning = true;
        EnsureOverlay(true);

        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(true);
        }

        yield return Fade(0f, 1f, fadeOutSeconds);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation == null)
        {
            if (overlayCanvas != null)
            {
                overlayCanvas.gameObject.SetActive(false);
            }

            isTransitioning = false;
            yield break;
        }

        while (!operation.isDone)
        {
            yield return null;
        }

        yield return null;
        EnsureOverlay(false);
        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(true);
        }

        if (holdBlackSecondsAfterLoad > 0f)
        {
            float elapsedHold = 0f;
            while (elapsedHold < holdBlackSecondsAfterLoad)
            {
                elapsedHold += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        yield return Fade(1f, 0f, fadeInSeconds);

        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(false);
        }

        isTransitioning = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null)
        {
            yield break;
        }

        float clampedDuration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;

        SetOverlayAlpha(from);

        while (elapsed < clampedDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / clampedDuration);
            SetOverlayAlpha(Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetOverlayAlpha(to);
    }

    private void EnsureOverlay(bool resetVisualState)
    {
        if (overlayCanvas == null)
        {
            GameObject canvasObject = new GameObject("SceneTransitionOverlay");
            canvasObject.transform.SetParent(transform, false);

            overlayCanvas = canvasObject.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 10000;

            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        if (fadeImage == null)
        {
            GameObject imageObject = new GameObject("FadeImage");
            imageObject.transform.SetParent(overlayCanvas.transform, false);

            fadeImage = imageObject.AddComponent<Image>();
            RectTransform fadeRect = fadeImage.rectTransform;
            fadeRect.anchorMin = Vector2.zero;
            fadeRect.anchorMax = Vector2.one;
            fadeRect.offsetMin = Vector2.zero;
            fadeRect.offsetMax = Vector2.zero;
        }

        fadeImage.color = overlayColor;
        if (resetVisualState)
        {
            SetOverlayAlpha(0f);
            overlayCanvas.gameObject.SetActive(false);
        }
    }

    private void SetOverlayAlpha(float normalizedAlpha)
    {
        if (fadeImage == null)
        {
            return;
        }

        Color color = overlayColor;
        color.a = Mathf.Clamp01(normalizedAlpha) * overlayColor.a;
        fadeImage.color = color;
    }
}
