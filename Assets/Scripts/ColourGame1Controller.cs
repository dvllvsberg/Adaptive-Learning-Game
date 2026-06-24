using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColourGame1Controller : MonoBehaviour, IOptionSelectionHandler, ILevelRestartHandler
{
    [Serializable]
    private struct ColourEntry
    {
        public string name;
        public Color color;
    }

    [Serializable]
    private struct OptionData
    {
        public int colourIndex;
        public int shapeIndex;
    }

    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text taskText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Transform optionsRoot;
    [SerializeField] private GridLayoutGroup optionsGridLayout;
    [SerializeField] private ColourGameOptionButton optionButtonPrefab;
    [SerializeField] private Button nextButton;

    [Header("Game Setup")]
    [SerializeField] private int maxLevel = 10;
    [SerializeField] private string titleLocalizationKey = "game.colour.1.title";
    [SerializeField] private string titleFallback = "Colour Game 1";
    [SerializeField] private string levelLabelKey = "game.common.level";
    [SerializeField] private string levelLabelFallback = "Level";
    [SerializeField] private string taskChooseColorKey = "game.colour.1.task.choose_color";
    [SerializeField] private string taskChooseColorFallback = "Choose color: {0}";
    [SerializeField] private string taskChooseColorShapeKey = "game.colour.1.task.choose_color_shape";
    [SerializeField] private string taskChooseColorShapeFallback = "Choose: {0} {1}";
    [SerializeField] private string resultCorrectKey = "game.common.result.correct";
    [SerializeField] private string resultCorrectFallback = "Correct!";
    [SerializeField] private string resultTryAgainKey = "game.common.result.try_again";
    [SerializeField] private string resultTryAgainFallback = "Try again";
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
    [SerializeField] private string[] colourLocalizationKeys =
    {
        "common.color.red",
        "common.color.blue",
        "common.color.green",
        "common.color.yellow"
    };
    [SerializeField] private string[] shapeNames = { "Circle", "Square", "Triangle", "Star" };
    [SerializeField] private Sprite[] shapeSprites;
    [SerializeField] private ColourEntry[] colours =
    {
        new ColourEntry { name = "Red", color = Color.red },
        new ColourEntry { name = "Blue", color = Color.blue },
        new ColourEntry { name = "Green", color = Color.green },
        new ColourEntry { name = "Yellow", color = Color.yellow }
    };

    private readonly List<ColourGameOptionButton> spawnedButtons = new List<ColourGameOptionButton>();

    private int currentLevel = 1;
    private int targetColourIndex;
    private int targetShapeIndex;
    private bool levelCompleted;

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
        RefreshTitleText();

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(LoadNextLevel);
            nextButton.gameObject.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.text = string.Empty;
            TMPTextAutoFit.ApplyTo(resultText, 14f, resultText.fontSize > 0f ? resultText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
        }

        LoadLevel();
    }

    public void OnOptionSelected(int selectedColourIndex, int selectedShapeIndex)
    {
        if (levelCompleted)
        {
            return;
        }

        bool isCorrect = IsColourAndShapeLevel()
            ? selectedColourIndex == targetColourIndex && selectedShapeIndex == targetShapeIndex
            : selectedColourIndex == targetColourIndex;

        if (isCorrect)
        {
            levelCompleted = true;

            if (resultText != null)
            {
                resultText.text = GetLocalizedText(resultCorrectKey, resultCorrectFallback);
                resultText.color = Color.green;
                TMPTextAutoFit.ApplyTo(resultText, 14f, resultText.fontSize > 0f ? resultText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
                PulseResultText(false);
            }

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
            }

            SetOptionsInteractable(false);
            return;
        }

        if (resultText != null)
        {
            resultText.text = GetLocalizedText(resultTryAgainKey, resultTryAgainFallback);
            resultText.color = Color.red;
            TMPTextAutoFit.ApplyTo(resultText, 14f, resultText.fontSize > 0f ? resultText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
            PulseResultText(true);
        }
    }

    private void LoadNextLevel()
    {
        currentLevel++;

        if (currentLevel > maxLevel)
        {
            ShowGameCompleted();
            return;
        }

        LoadLevel();
    }

    private void LoadLevel()
    {
        levelCompleted = false;

        if (levelText != null)
        {
            levelText.text = $"{GetLocalizedText(levelLabelKey, levelLabelFallback)} {currentLevel}/{maxLevel}";
            TMPTextAutoFit.ApplyTo(levelText, 16f, levelText.fontSize > 0f ? levelText.fontSize : 32f, true, TextOverflowModes.Ellipsis);
        }

        if (resultText != null)
        {
            resultText.text = string.Empty;
            TMPTextAutoFit.ApplyTo(resultText, 14f, resultText.fontSize > 0f ? resultText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
        }

        BuildOptionsForCurrentLevel();
        UpdateTaskText();
    }

    private void BuildOptionsForCurrentLevel()
    {
        if (optionButtonPrefab == null || optionsRoot == null)
        {
            Debug.LogWarning("ColourGame1Controller: Option prefab or options root is not assigned.");
            return;
        }

        if (colours.Length == 0 || shapeNames.Length == 0)
        {
            Debug.LogWarning("ColourGame1Controller: Colours or shapes are not configured.");
            return;
        }

        List<OptionData> options;
        if (currentLevel <= 3)
        {
            options = BuildLevel1To3Options();
        }
        else if (currentLevel <= 5)
        {
            options = BuildLevel4To5Options();
        }
        else
        {
            options = BuildLevel6To10Options();
        }

        ClearOldButtons();
        ConfigureGrid(options.Count);
        SpawnOptionButtons(options);
    }

    private List<OptionData> BuildLevel1To3Options()
    {
        // Levels 1-3: 4 options, unique colors, unique shapes.
        int optionCount = Mathf.Min(4, colours.Length);
        optionCount = Mathf.Min(optionCount, shapeNames.Length);

        List<int> colourIndices = new List<int>();
        for (int i = 0; i < colours.Length; i++)
        {
            colourIndices.Add(i);
        }
        Shuffle(colourIndices);

        List<int> shapeIndices = new List<int>();
        for (int i = 0; i < shapeNames.Length; i++)
        {
            shapeIndices.Add(i);
        }
        Shuffle(shapeIndices);

        List<OptionData> options = new List<OptionData>();
        for (int i = 0; i < optionCount; i++)
        {
            options.Add(new OptionData
            {
                colourIndex = colourIndices[i],
                shapeIndex = shapeIndices[i]
            });
        }

        int targetOption = UnityEngine.Random.Range(0, options.Count);
        targetColourIndex = options[targetOption].colourIndex;
        targetShapeIndex = options[targetOption].shapeIndex;

        Shuffle(options);
        return options;
    }

    private List<OptionData> BuildLevel4To5Options()
    {
        // Levels 4-5: 4 options, duplicate non-target color is allowed.
        int optionCount = 4;
        targetColourIndex = UnityEngine.Random.Range(0, colours.Length);
        targetShapeIndex = UnityEngine.Random.Range(0, shapeNames.Length);

        int duplicatedNonTargetColour = GetRandomNonTargetColour(targetColourIndex);
        int secondNonTargetColour = GetRandomNonTargetColour(targetColourIndex, duplicatedNonTargetColour);

        List<int> colorPlan = new List<int>
        {
            targetColourIndex,
            duplicatedNonTargetColour,
            duplicatedNonTargetColour,
            secondNonTargetColour
        };

        List<int> shapePlan = BuildUniqueShapePlan(optionCount);

        List<OptionData> options = new List<OptionData>();
        for (int i = 0; i < optionCount; i++)
        {
            options.Add(new OptionData
            {
                colourIndex = colorPlan[i],
                shapeIndex = shapePlan[i]
            });
        }

        Shuffle(options);
        return options;
    }

    private List<OptionData> BuildLevel6To10Options()
    {
        // Levels 6-10: 9 options, target is color+shape.
        int optionCount = Mathf.Min(9, colours.Length * shapeNames.Length);
        targetColourIndex = UnityEngine.Random.Range(0, colours.Length);
        targetShapeIndex = UnityEngine.Random.Range(0, shapeNames.Length);

        List<OptionData> options = new List<OptionData>();
        options.Add(new OptionData
        {
            colourIndex = targetColourIndex,
            shapeIndex = targetShapeIndex
        });

        // Add close decoys: same color and same shape.
        TryAddUniqueOption(options, new OptionData
        {
            colourIndex = targetColourIndex,
            shapeIndex = GetRandomDifferentShape(targetShapeIndex)
        });

        TryAddUniqueOption(options, new OptionData
        {
            colourIndex = GetRandomNonTargetColour(targetColourIndex),
            shapeIndex = targetShapeIndex
        });

        while (options.Count < optionCount)
        {
            OptionData candidate = new OptionData
            {
                colourIndex = UnityEngine.Random.Range(0, colours.Length),
                shapeIndex = UnityEngine.Random.Range(0, shapeNames.Length)
            };

            if (candidate.colourIndex == targetColourIndex && candidate.shapeIndex == targetShapeIndex)
            {
                continue;
            }

            TryAddUniqueOption(options, candidate);
        }

        Shuffle(options);
        return options;
    }

    private void SpawnOptionButtons(List<OptionData> options)
    {
        for (int i = 0; i < options.Count; i++)
        {
            OptionData option = options[i];
            ColourGameOptionButton newButton = Instantiate(optionButtonPrefab, optionsRoot);
            newButton.Setup(
                this,
                option.colourIndex,
                option.shapeIndex,
                colours[option.colourIndex].color,
                GetLocalizedShapeName(option.shapeIndex),
                GetShapeSprite(option.shapeIndex));
            spawnedButtons.Add(newButton);
        }
    }

    private void UpdateTaskText()
    {
        if (taskText == null)
        {
            return;
        }

        if (IsColourAndShapeLevel())
        {
            string template = GetLocalizedText(taskChooseColorShapeKey, taskChooseColorShapeFallback);
            taskText.text = string.Format(template, GetLocalizedColourName(targetColourIndex), GetLocalizedShapeName(targetShapeIndex));
            TMPTextAutoFit.ApplyTo(taskText, 14f, taskText.fontSize > 0f ? taskText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
            return;
        }

        string colorTemplate = GetLocalizedText(taskChooseColorKey, taskChooseColorFallback);
        taskText.text = string.Format(colorTemplate, GetLocalizedColourName(targetColourIndex));
        TMPTextAutoFit.ApplyTo(taskText, 14f, taskText.fontSize > 0f ? taskText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
    }

    private void ConfigureGrid(int optionCount)
    {
        if (optionsGridLayout == null)
        {
            return;
        }

        optionsGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        optionsGridLayout.constraintCount = optionCount == 9 ? 3 : 2;
        optionsGridLayout.childAlignment = TextAnchor.MiddleCenter;
    }

    private void SetOptionsInteractable(bool value)
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null)
            {
                spawnedButtons[i].SetInteractable(value);
            }
        }
    }

    private void ClearOldButtons()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null)
            {
                Destroy(spawnedButtons[i].gameObject);
            }
        }

        spawnedButtons.Clear();
    }

    private bool IsColourAndShapeLevel()
    {
        return currentLevel >= 6;
    }

    private void ShowGameCompleted()
    {
        ClearOldButtons();

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
            resultText.color = Color.green;
            TMPTextAutoFit.ApplyTo(resultText, 14f, resultText.fontSize > 0f ? resultText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
            PulseResultText(false);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
        }
    }

    public void RestartCurrentLevel()
    {
        LoadLevel();
    }

    public void RestartFromLevel1()
    {
        currentLevel = 1;
        LoadLevel();
    }

    private void RefreshTitleText()
    {
        if (titleText == null)
        {
            return;
        }

        if (LanguageManager.Instance != null)
        {
            titleText.text = LanguageManager.Instance.GetText(titleLocalizationKey, titleFallback);
            TMPTextAutoFit.ApplyTo(titleText, 18f, titleText.fontSize > 0f ? titleText.fontSize : 40f, true, TextOverflowModes.Ellipsis);
            return;
        }

        titleText.text = titleFallback;
        TMPTextAutoFit.ApplyTo(titleText, 18f, titleText.fontSize > 0f ? titleText.fontSize : 40f, true, TextOverflowModes.Ellipsis);
    }

    private void OnLanguageChanged(AppLanguage _)
    {
        RefreshTitleText();
        RefreshDynamicTexts();
    }

    private void RefreshDynamicTexts()
    {
        if (levelText != null)
        {
            levelText.text = $"{GetLocalizedText(levelLabelKey, levelLabelFallback)} {currentLevel}/{maxLevel}";
            TMPTextAutoFit.ApplyTo(levelText, 16f, levelText.fontSize > 0f ? levelText.fontSize : 32f, true, TextOverflowModes.Ellipsis);
        }

        UpdateTaskText();
    }

    private string GetLocalizedText(string key, string fallback)
    {
        if (LanguageManager.Instance != null)
        {
            return LanguageManager.Instance.GetText(key, fallback);
        }

        return fallback;
    }

    private void PulseResultText(bool isError)
    {
        if (resultText == null)
        {
            return;
        }

        TMPTextPulseOnChange pulse = resultText.GetComponent<TMPTextPulseOnChange>();
        if (pulse != null)
        {
            if (isError)
            {
                pulse.Shake();
            }
            else
            {
                pulse.Pulse();
            }
        }
    }

    private string GetLocalizedColourName(int colourIndex)
    {
        string fallback = GetFallbackColourName(colourIndex);
        string key = GetKeyAtIndex(colourLocalizationKeys, colourIndex);
        return GetLocalizedText(key, fallback);
    }

    private string GetLocalizedShapeName(int shapeIndex)
    {
        string fallback = GetFallbackShapeName(shapeIndex);
        string key = GetKeyAtIndex(shapeLocalizationKeys, shapeIndex);
        return GetLocalizedText(key, fallback);
    }

    private string GetFallbackColourName(int colourIndex)
    {
        if (colourIndex < 0 || colourIndex >= colours.Length)
        {
            return "Color";
        }

        return string.IsNullOrWhiteSpace(colours[colourIndex].name) ? "Color" : colours[colourIndex].name;
    }

    private string GetFallbackShapeName(int shapeIndex)
    {
        if (shapeIndex < 0 || shapeIndex >= shapeNames.Length)
        {
            return "Shape";
        }

        return string.IsNullOrWhiteSpace(shapeNames[shapeIndex]) ? "Shape" : shapeNames[shapeIndex];
    }

    private string GetKeyAtIndex(string[] keys, int index)
    {
        if (keys == null || index < 0 || index >= keys.Length)
        {
            return string.Empty;
        }

        return keys[index];
    }

    private int GetRandomNonTargetColour(int target, int blocked = -1)
    {
        List<int> candidates = new List<int>();
        for (int i = 0; i < colours.Length; i++)
        {
            if (i != target && i != blocked)
            {
                candidates.Add(i);
            }
        }

        if (candidates.Count == 0)
        {
            return target;
        }

        int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
        return candidates[randomIndex];
    }

    private int GetRandomDifferentShape(int targetShape)
    {
        if (shapeNames.Length <= 1)
        {
            return targetShape;
        }

        int shapeIndex = targetShape;
        while (shapeIndex == targetShape)
        {
            shapeIndex = UnityEngine.Random.Range(0, shapeNames.Length);
        }

        return shapeIndex;
    }

    private List<int> BuildUniqueShapePlan(int count)
    {
        List<int> pool = new List<int>();
        for (int i = 0; i < shapeNames.Length; i++)
        {
            pool.Add(i);
        }
        Shuffle(pool);

        List<int> plan = new List<int>();
        for (int i = 0; i < count; i++)
        {
            if (i < pool.Count)
            {
                plan.Add(pool[i]);
            }
            else
            {
                plan.Add(UnityEngine.Random.Range(0, shapeNames.Length));
            }
        }

        return plan;
    }

    private void TryAddUniqueOption(List<OptionData> options, OptionData candidate)
    {
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].colourIndex == candidate.colourIndex &&
                options[i].shapeIndex == candidate.shapeIndex)
            {
                return;
            }
        }

        options.Add(candidate);
    }

    private Sprite GetShapeSprite(int shapeIndex)
    {
        if (shapeSprites == null || shapeIndex < 0 || shapeIndex >= shapeSprites.Length)
        {
            return null;
        }

        return shapeSprites[shapeIndex];
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
