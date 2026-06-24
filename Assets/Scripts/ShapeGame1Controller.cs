using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShapeGame1Controller : MonoBehaviour, ILevelRestartHandler
{
    [System.Serializable]
    private struct ShapeVisual
    {
        public ShapeKind shapeKind;
        public Sprite sprite;
    }

    [System.Serializable]
    private struct ColourEntry
    {
        public string name;
        public Color color;
    }

    [Header("Scene References")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private Transform dragLayer;
    [SerializeField] private Transform shapeContainer;
    [SerializeField] private DraggableShape draggableShapePrefab;
    [SerializeField] private ShapeDropZone[] dropZones;

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text taskText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Button nextButton;

    [Header("Gameplay")]
    [SerializeField] private int maxLevel = 10;
    [SerializeField] private string titleLocalizationKey = "game.shape.1.title";
    [SerializeField] private string titleFallback = "Shape Sort";
    [SerializeField] private string levelLabelKey = "game.common.level";
    [SerializeField] private string levelLabelFallback = "Level";
    [SerializeField] private string taskPlaceShapeKey = "game.shape.1.task.place_shape";
    [SerializeField] private string taskPlaceShapeFallback = "Place: {0}";
    [SerializeField] private string resultTryAgainKey = "game.common.result.try_again";
    [SerializeField] private string resultTryAgainFallback = "Try again";
    [SerializeField] private string resultWrongTargetKey = "game.shape.1.result.wrong_target";
    [SerializeField] private string resultWrongTargetFallback = "Wrong target shape";
    [SerializeField] private string resultCorrectKey = "game.common.result.correct";
    [SerializeField] private string resultCorrectFallback = "Correct!";
    [SerializeField] private string resultLevelCompleteKey = "game.shape.1.result.level_complete";
    [SerializeField] private string resultLevelCompleteFallback = "Level complete!";
    [SerializeField] private string resultCompletedKey = "game.common.result.completed";
    [SerializeField] private string resultCompletedFallback = "All levels completed!";
    [SerializeField] private string resultGreatJobKey = "game.common.result.great_job";
    [SerializeField] private string resultGreatJobFallback = "Great job!";
    [SerializeField] private string[] shapeLocalizationKeys =
    {
        "common.shape.circle",
        "common.shape.square",
        "common.shape.triangle",
        "common.shape.star"
    };
    [SerializeField] private string[] shapeFallbackNames =
    {
        "Circle",
        "Square",
        "Triangle",
        "Star"
    };
    [SerializeField] private int tasksPerLevel = 5;
    [SerializeField] private float spawnDelaySeconds = 0.5f;
    [SerializeField] private float silhouetteVisibleSeconds = 3f;
    [SerializeField] private float shapeVisualScale = 1f;
    [SerializeField] private bool enableBasketSwap = true;
    [SerializeField] private float swapIntervalSeconds = 2.4f;
    [SerializeField] private float swapDurationSeconds = 0.8f;
    [SerializeField] private float firstSwapDelaySeconds = 1f;
    [SerializeField] private ShapeVisual[] shapeVisuals;
    [SerializeField] private ColourEntry[] colours =
    {
        new ColourEntry { name = "Red", color = Color.red },
        new ColourEntry { name = "Blue", color = Color.blue },
        new ColourEntry { name = "Green", color = Color.green },
        new ColourEntry { name = "Yellow", color = Color.yellow }
    };

    private readonly Dictionary<ShapeKind, Sprite> shapeSpriteMap = new Dictionary<ShapeKind, Sprite>();
    private readonly Dictionary<ShapeDropZone, Vector2> defaultZonePositions = new Dictionary<ShapeDropZone, Vector2>();

    private int currentLevel = 1;
    private int solvedTasksThisLevel;
    private ShapeKind currentTargetShape;
    private int currentTargetColourIndex;
    private DraggableShape activeShape;
    private bool isLevelCompleted;
    private float defaultZoneSpacing = 180f;
    private float defaultZoneCenterX;
    private Coroutine hintPhaseCoroutine;

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
        ResolveTitleText();
        RefreshTitleText();
        BuildShapeSpriteMap();
        CacheDropZoneLayout();

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextLevel);
            nextButton.gameObject.SetActive(false);
        }

        StartLevel();
    }

    public bool TryPlaceShape(DraggableShape shape, ShapeDropZone zone)
    {
        if (shape == null || zone == null || isLevelCompleted)
        {
            return false;
        }

        bool isShapeCorrect = shape.ShapeKind == zone.AcceptedShape;
        if (!isShapeCorrect)
        {
            zone.FlashWrong();
            SetResult(GetLocalizedText(resultTryAgainKey, resultTryAgainFallback), Color.red);
            return false;
        }

        bool isTargetShape = shape.ShapeKind == currentTargetShape;
        if (!isTargetShape)
        {
            zone.FlashWrong();
            SetResult(GetLocalizedText(resultWrongTargetKey, resultWrongTargetFallback), Color.red);
            return false;
        }

        zone.FlashCorrect();
        SetResult(GetLocalizedText(resultCorrectKey, resultCorrectFallback), Color.green);
        solvedTasksThisLevel++;

        if (activeShape != null)
        {
            Destroy(activeShape.gameObject);
            activeShape = null;
        }

        UpdateProgressText();

        if (solvedTasksThisLevel >= tasksPerLevel)
        {
            CompleteLevel();
        }
        else
        {
            Invoke(nameof(SpawnTaskShape), spawnDelaySeconds);
        }

        return true;
    }

    private void StartLevel()
    {
        isLevelCompleted = false;
        solvedTasksThisLevel = 0;
        StopHintPhaseRoutine();
        SetResult(string.Empty, Color.white);
        ConfigureDropZonesForLevel();
        StartHintPhaseForCurrentLevel();
        UpdateTopTexts();
        UpdateProgressText();

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
        }

        if (activeShape != null)
        {
            Destroy(activeShape.gameObject);
            activeShape = null;
        }

        SpawnTaskShape();
    }

    private void SpawnTaskShape()
    {
        if (draggableShapePrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("ShapeGame1Controller: Assign draggableShapePrefab and spawnPoint.");
            return;
        }

        List<ShapeKind> availableShapes = GetShapesForLevel(currentLevel);
        if (availableShapes.Count == 0)
        {
            Debug.LogWarning("ShapeGame1Controller: No shapes available for this level.");
            return;
        }

        currentTargetShape = availableShapes[Random.Range(0, availableShapes.Count)];
        currentTargetColourIndex = Random.Range(0, colours.Length);

        Color randomColour = colours[currentTargetColourIndex].color;
        Sprite shapeSprite = GetShapeSprite(currentTargetShape);
        float visualScale = GetShapeScaleForLevel(currentLevel);

        activeShape = Instantiate(draggableShapePrefab, shapeContainer != null ? shapeContainer : spawnPoint.parent);
        RectTransform activeRect = activeShape.GetComponent<RectTransform>();
        if (activeRect != null)
        {
            activeRect.anchoredPosition = spawnPoint.anchoredPosition;
        }

        activeShape.Initialize(
            this,
            rootCanvas,
            dragLayer != null ? dragLayer : activeShape.transform.parent,
            currentTargetShape,
            shapeSprite,
            randomColour,
            visualScale);

        UpdateTopTexts();
    }

    private void CompleteLevel()
    {
        isLevelCompleted = true;
        StopHintPhaseRoutine();
        SetResult(GetLocalizedText(resultLevelCompleteKey, resultLevelCompleteFallback), Color.green);

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
        }
    }

    private void NextLevel()
    {
        currentLevel++;
        if (currentLevel > maxLevel)
        {
            ShowGameCompleted();
            return;
        }

        StartLevel();
    }

    private void ShowGameCompleted()
    {
        StopHintPhaseRoutine();

        if (activeShape != null)
        {
            Destroy(activeShape.gameObject);
            activeShape = null;
        }

        if (levelText != null)
        {
            levelText.text = $"{GetLocalizedText(levelLabelKey, levelLabelFallback)} {maxLevel}/{maxLevel}";
            TMPTextAutoFit.ApplyTo(levelText, 16f, levelText.fontSize > 0f ? levelText.fontSize : 32f, true, TextOverflowModes.Ellipsis);
        }

        if (taskText != null)
        {
            taskText.text = GetLocalizedText(resultCompletedKey, resultCompletedFallback);
            TMPTextAutoFit.ApplyTo(taskText, 14f, taskText.fontSize > 0f ? taskText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
        }

        if (progressText != null)
        {
            progressText.text = $"{tasksPerLevel}/{tasksPerLevel}";
            TMPTextAutoFit.ApplyTo(progressText, 14f, progressText.fontSize > 0f ? progressText.fontSize : 24f, true, TextOverflowModes.Ellipsis);
        }

        SetResult(GetLocalizedText(resultGreatJobKey, resultGreatJobFallback), Color.green);

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
        }
    }

    private void UpdateTopTexts()
    {
        if (levelText != null)
        {
            levelText.text = $"{GetLocalizedText(levelLabelKey, levelLabelFallback)} {currentLevel}/{maxLevel}";
            TMPTextAutoFit.ApplyTo(levelText, 16f, levelText.fontSize > 0f ? levelText.fontSize : 32f, true, TextOverflowModes.Ellipsis);
        }

        if (taskText != null)
        {
            string template = GetLocalizedText(taskPlaceShapeKey, taskPlaceShapeFallback);
            taskText.text = string.Format(template, GetLocalizedShapeName(currentTargetShape));
            TMPTextAutoFit.ApplyTo(taskText, 14f, taskText.fontSize > 0f ? taskText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
        }
    }

    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = $"{solvedTasksThisLevel}/{tasksPerLevel}";
            TMPTextAutoFit.ApplyTo(progressText, 14f, progressText.fontSize > 0f ? progressText.fontSize : 24f, true, TextOverflowModes.Ellipsis);
        }
    }

    private void SetResult(string message, Color color)
    {
        if (resultText != null)
        {
            resultText.text = message;
            resultText.color = color;
            TMPTextAutoFit.ApplyTo(resultText, 14f, resultText.fontSize > 0f ? resultText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
            TMPTextPulseOnChange pulse = resultText.GetComponent<TMPTextPulseOnChange>();
            if (pulse != null)
            {
                if (IsErrorColor(color))
                {
                    pulse.Shake();
                }
                else
                {
                    pulse.Pulse();
                }
            }
        }
    }

    private static bool IsErrorColor(Color color)
    {
        return color.r > color.g + 0.05f && color.r > color.b + 0.05f;
    }

    private List<ShapeKind> GetShapesForLevel(int level)
    {
        List<ShapeKind> list = new List<ShapeKind>
        {
            ShapeKind.Circle,
            ShapeKind.Square,
            ShapeKind.Triangle
        };

        if (level >= 4)
        {
            list.Add(ShapeKind.Star);
        }

        return list;
    }

    private float GetShapeScaleForLevel(int level)
    {
        return shapeVisualScale;
    }

    private void ConfigureDropZonesForLevel()
    {
        if (dropZones == null || dropZones.Length == 0)
        {
            return;
        }

        List<ShapeKind> availableShapes = GetShapesForLevel(currentLevel);
        for (int i = 0; i < dropZones.Length; i++)
        {
            ShapeDropZone zone = dropZones[i];
            if (zone == null)
            {
                continue;
            }

            bool isActive = availableShapes.Contains(zone.AcceptedShape);
            zone.gameObject.SetActive(isActive);

            if (!isActive)
            {
                continue;
            }

            zone.ShowSilhouette();
        }

        ArrangeActiveDropZones(availableShapes);

    }

    private void CacheDropZoneLayout()
    {
        defaultZonePositions.Clear();
        if (dropZones == null || dropZones.Length == 0)
        {
            return;
        }

        List<float> xPositions = new List<float>();
        for (int i = 0; i < dropZones.Length; i++)
        {
            ShapeDropZone zone = dropZones[i];
            if (zone == null)
            {
                continue;
            }

            RectTransform rect = zone.GetComponent<RectTransform>();
            if (rect == null)
            {
                continue;
            }

            defaultZonePositions[zone] = rect.anchoredPosition;
            xPositions.Add(rect.anchoredPosition.x);
        }

        if (xPositions.Count == 0)
        {
            return;
        }

        xPositions.Sort();
        float spacingSum = 0f;
        int spacingCount = 0;
        for (int i = 1; i < xPositions.Count; i++)
        {
            float spacing = Mathf.Abs(xPositions[i] - xPositions[i - 1]);
            if (spacing > 0.01f)
            {
                spacingSum += spacing;
                spacingCount++;
            }
        }

        if (spacingCount > 0)
        {
            defaultZoneSpacing = spacingSum / spacingCount;
        }

        float centerSum = 0f;
        for (int i = 0; i < xPositions.Count; i++)
        {
            centerSum += xPositions[i];
        }
        defaultZoneCenterX = centerSum / xPositions.Count;
    }

    private void ArrangeActiveDropZones(List<ShapeKind> activeShapes)
    {
        if (dropZones == null || dropZones.Length == 0 || defaultZonePositions.Count == 0)
        {
            return;
        }

        List<ShapeDropZone> activeZones = new List<ShapeDropZone>();
        for (int i = 0; i < activeShapes.Count; i++)
        {
            ShapeKind shape = activeShapes[i];
            for (int j = 0; j < dropZones.Length; j++)
            {
                ShapeDropZone zone = dropZones[j];
                if (zone != null && zone.gameObject.activeSelf && zone.AcceptedShape == shape)
                {
                    activeZones.Add(zone);
                    break;
                }
            }
        }

        if (activeZones.Count == 0)
        {
            return;
        }

        if (activeZones.Count == 4)
        {
            for (int i = 0; i < activeZones.Count; i++)
            {
                RectTransform rect = activeZones[i].GetComponent<RectTransform>();
                if (rect != null && defaultZonePositions.TryGetValue(activeZones[i], out Vector2 defaultPos))
                {
                    rect.anchoredPosition = defaultPos;
                }
            }
            return;
        }

        if (activeZones.Count == 3)
        {
            float centerY = 0f;
            int yCount = 0;
            for (int i = 0; i < activeZones.Count; i++)
            {
                if (defaultZonePositions.TryGetValue(activeZones[i], out Vector2 defaultPos))
                {
                    centerY += defaultPos.y;
                    yCount++;
                }
            }
            if (yCount > 0)
            {
                centerY /= yCount;
            }

            for (int i = 0; i < activeZones.Count; i++)
            {
                RectTransform rect = activeZones[i].GetComponent<RectTransform>();
                if (rect == null)
                {
                    continue;
                }

                float targetX = defaultZoneCenterX + (i - 1) * defaultZoneSpacing;
                rect.anchoredPosition = new Vector2(targetX, centerY);
            }
        }
    }

    private void StartHintPhaseForCurrentLevel()
    {
        if (currentLevel < 4)
        {
            return;
        }

        hintPhaseCoroutine = StartCoroutine(HintPhaseRoutine());
    }

    private bool ShouldSwapBasketsThisLevel()
    {
        return enableBasketSwap && currentLevel >= 8;
    }

    private void StopHintPhaseRoutine()
    {
        if (hintPhaseCoroutine != null)
        {
            StopCoroutine(hintPhaseCoroutine);
            hintPhaseCoroutine = null;
        }
    }

    private IEnumerator HintPhaseRoutine()
    {
        float phaseStart = Time.time;
        float phaseEnd = phaseStart + Mathf.Max(0f, silhouetteVisibleSeconds);
        float nextSwapAt = phaseStart + Mathf.Max(0f, firstSwapDelaySeconds);

        while (!isLevelCompleted && Time.time < phaseEnd)
        {
            if (ShouldSwapBasketsThisLevel() && Time.time >= nextSwapAt)
            {
                List<ShapeDropZone> activeZones = GetActiveDropZones();
                if (activeZones.Count >= 2)
                {
                    int firstIndex = Random.Range(0, activeZones.Count);
                    int secondIndex = firstIndex;
                    while (secondIndex == firstIndex)
                    {
                        secondIndex = Random.Range(0, activeZones.Count);
                    }

                    ShapeDropZone firstZone = activeZones[firstIndex];
                    ShapeDropZone secondZone = activeZones[secondIndex];

                    yield return StartCoroutine(SwapZonePositions(firstZone, secondZone));
                }

                nextSwapAt += Mathf.Max(0.01f, swapIntervalSeconds);
            }
            else
            {
                yield return null;
            }
        }

        HideActiveZoneSilhouettes();
        hintPhaseCoroutine = null;
    }

    private IEnumerator SwapZonePositions(ShapeDropZone firstZone, ShapeDropZone secondZone)
    {
        if (firstZone == null || secondZone == null)
        {
            yield break;
        }

        RectTransform firstRect = firstZone.GetComponent<RectTransform>();
        RectTransform secondRect = secondZone.GetComponent<RectTransform>();
        if (firstRect == null || secondRect == null)
        {
            yield break;
        }

        Vector2 firstStart = firstRect.anchoredPosition;
        Vector2 secondStart = secondRect.anchoredPosition;
        float elapsed = 0f;
        float duration = Mathf.Max(0.05f, swapDurationSeconds);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);

            firstRect.anchoredPosition = Vector2.Lerp(firstStart, secondStart, t);
            secondRect.anchoredPosition = Vector2.Lerp(secondStart, firstStart, t);
            yield return null;
        }

        firstRect.anchoredPosition = secondStart;
        secondRect.anchoredPosition = firstStart;
    }

    private List<ShapeDropZone> GetActiveDropZones()
    {
        List<ShapeDropZone> list = new List<ShapeDropZone>();
        if (dropZones == null)
        {
            return list;
        }

        for (int i = 0; i < dropZones.Length; i++)
        {
            ShapeDropZone zone = dropZones[i];
            if (zone != null && zone.gameObject.activeInHierarchy)
            {
                list.Add(zone);
            }
        }

        return list;
    }

    private void HideActiveZoneSilhouettes()
    {
        if (dropZones == null)
        {
            return;
        }

        for (int i = 0; i < dropZones.Length; i++)
        {
            ShapeDropZone zone = dropZones[i];
            if (zone == null || !zone.gameObject.activeInHierarchy)
            {
                continue;
            }

            zone.HideSilhouette();
        }
    }

    private void BuildShapeSpriteMap()
    {
        shapeSpriteMap.Clear();
        for (int i = 0; i < shapeVisuals.Length; i++)
        {
            ShapeVisual visual = shapeVisuals[i];
            if (visual.sprite == null)
            {
                continue;
            }

            shapeSpriteMap[visual.shapeKind] = visual.sprite;
        }
    }

    private Sprite GetShapeSprite(ShapeKind shapeKind)
    {
        if (shapeSpriteMap.TryGetValue(shapeKind, out Sprite sprite))
        {
            return sprite;
        }

        return null;
    }

    private string GetLocalizedText(string key, string fallback)
    {
        if (LanguageManager.Instance != null)
        {
            return LanguageManager.Instance.GetText(key, fallback);
        }

        return fallback;
    }

    private void ResolveTitleText()
    {
        if (IsMainTitleCandidate(titleText))
        {
            return;
        }

        titleText = null;
        TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        TMP_Text bestCandidate = null;
        float bestY = float.MinValue;

        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text textItem = texts[i];
            if (!IsMainTitleCandidate(textItem))
            {
                continue;
            }

            float candidateY = textItem.rectTransform != null ? textItem.rectTransform.position.y : 0f;
            if (bestCandidate == null || candidateY > bestY)
            {
                bestCandidate = textItem;
                bestY = candidateY;
            }
        }

        titleText = bestCandidate;
    }

    private void RefreshTitleText()
    {
        ResolveTitleText();
        if (titleText == null)
        {
            return;
        }

        titleText.text = GetLocalizedText(titleLocalizationKey, titleFallback);
        TMPTextAutoFit.ApplyTo(titleText, 18f, titleText.fontSize > 0f ? titleText.fontSize : 32f, true, TextOverflowModes.Ellipsis);
    }

    private bool IsMainTitleCandidate(TMP_Text textItem)
    {
        return textItem != null &&
               textItem.name == "TitleText" &&
               !IsInsideOverlayPanel(textItem.transform);
    }

    private bool IsInsideOverlayPanel(Transform current)
    {
        while (current != null)
        {
            string objectName = current.name;
            if (objectName == "HelpPanel" ||
                objectName == "HelpWindow" ||
                objectName == "PauseMenuPanel" ||
                objectName == "PausePanel" ||
                objectName == "SettingsPanel" ||
                objectName == "ComingSoonPanel")
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private string GetLocalizedShapeName(ShapeKind shapeKind)
    {
        int index = (int)shapeKind;
        string fallback = GetFallbackShapeName(index);
        string key = GetKeyAtIndex(shapeLocalizationKeys, index);
        return GetLocalizedText(key, fallback);
    }

    private string GetFallbackShapeName(int index)
    {
        if (shapeFallbackNames != null && index >= 0 && index < shapeFallbackNames.Length &&
            !string.IsNullOrWhiteSpace(shapeFallbackNames[index]))
        {
            return shapeFallbackNames[index];
        }

        return "Shape";
    }

    private string GetKeyAtIndex(string[] keys, int index)
    {
        if (keys == null || index < 0 || index >= keys.Length)
        {
            return string.Empty;
        }

        return keys[index];
    }

    private void OnLanguageChanged(AppLanguage _)
    {
        RefreshTitleText();

        if (!isLevelCompleted)
        {
            UpdateTopTexts();
            return;
        }

        if (levelText != null)
        {
            levelText.text = $"{GetLocalizedText(levelLabelKey, levelLabelFallback)} {maxLevel}/{maxLevel}";
            TMPTextAutoFit.ApplyTo(levelText, 16f, levelText.fontSize > 0f ? levelText.fontSize : 32f, true, TextOverflowModes.Ellipsis);
        }

        if (taskText != null)
        {
            taskText.text = GetLocalizedText(resultCompletedKey, resultCompletedFallback);
            TMPTextAutoFit.ApplyTo(taskText, 14f, taskText.fontSize > 0f ? taskText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
        }

        if (resultText != null)
        {
            resultText.text = GetLocalizedText(resultGreatJobKey, resultGreatJobFallback);
            TMPTextAutoFit.ApplyTo(resultText, 14f, resultText.fontSize > 0f ? resultText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
        }
    }

    public void RestartCurrentLevel()
    {
        StartLevel();
    }

    public void RestartFromLevel1()
    {
        currentLevel = 1;
        StartLevel();
    }
}
