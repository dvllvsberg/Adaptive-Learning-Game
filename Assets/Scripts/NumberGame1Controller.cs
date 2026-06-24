using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberGame1Controller : MonoBehaviour, ILevelRestartHandler
{
    private enum RoundState
    {
        WaitingToStart,
        ShowingSequence,
        AwaitingInput,
        RoundSuccess,
        RoundFailed,
        GameCompleted
    }

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text taskText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private NumberTileButton[] tiles;

    [Header("Gameplay")]
    [SerializeField] private int maxLevel = 10;
    [SerializeField] private float pauseBeforePlayback = 0.5f;
    [SerializeField] private string titleLocalizationKey = "game.number.1.title";
    [SerializeField] private string titleFallback = "Repeat the Sequence";
    [SerializeField] private string levelLabelKey = "game.common.level";
    [SerializeField] private string levelLabelFallback = "Level";
    [SerializeField] private string taskPressStartKey = "game.number.1.task.press_start";
    [SerializeField] private string taskPressStartFallback = "Press Start";
    [SerializeField] private string taskWatchSequenceKey = "game.number.1.task.watch_sequence";
    [SerializeField] private string taskWatchSequenceFallback = "Watch the sequence";
    [SerializeField] private string taskRepeatSequenceKey = "game.number.1.task.repeat_sequence";
    [SerializeField] private string taskRepeatSequenceFallback = "Repeat the sequence";
    [SerializeField] private string taskSequenceCompletedKey = "game.number.1.task.sequence_completed";
    [SerializeField] private string taskSequenceCompletedFallback = "Sequence completed";
    [SerializeField] private string taskPressRetryKey = "game.number.1.task.press_retry";
    [SerializeField] private string taskPressRetryFallback = "Press Retry";
    [SerializeField] private string resultCorrectKey = "game.common.result.correct";
    [SerializeField] private string resultCorrectFallback = "Correct!";
    [SerializeField] private string resultWrongSequenceKey = "game.number.1.result.wrong_sequence";
    [SerializeField] private string resultWrongSequenceFallback = "Wrong sequence. Try again.";
    [SerializeField] private string resultCompletedKey = "game.common.result.completed";
    [SerializeField] private string resultCompletedFallback = "All levels completed!";
    [SerializeField] private string resultGreatJobKey = "game.common.result.great_job";
    [SerializeField] private string resultGreatJobFallback = "Great job!";
    [SerializeField] private string actionStartKey = "game.number.1.action.start";
    [SerializeField] private string actionStartFallback = "Start";
    [SerializeField] private string actionRetryKey = "game.number.1.action.retry";
    [SerializeField] private string actionRetryFallback = "Retry";
    [SerializeField] private string actionNextLevelKey = "game.number.1.action.next_level";
    [SerializeField] private string actionNextLevelFallback = "Next Level";
    [SerializeField] private string actionFinishKey = "game.number.1.action.finish";
    [SerializeField] private string actionFinishFallback = "Finish";

    private readonly List<int> sequence = new List<int>();

    private int currentLevel = 1;
    private int inputIndex;
    private RoundState state = RoundState.WaitingToStart;

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

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionButtonPressed);
        }

        InitializeTiles();
        EnterWaitingState();
        RefreshHeaderTexts();
    }

    public void OnTilePressed(int tileIndex)
    {
        if (state != RoundState.AwaitingInput)
        {
            return;
        }

        if (tileIndex == sequence[inputIndex])
        {
            inputIndex++;
            UpdateProgressText();

            if (inputIndex >= sequence.Count)
            {
                OnRoundCompleted();
            }

            return;
        }

        OnRoundFailed();
    }

    private void OnActionButtonPressed()
    {
        if (state == RoundState.WaitingToStart || state == RoundState.RoundFailed)
        {
            StartCoroutine(PlayRoundRoutine());
            return;
        }

        if (state == RoundState.RoundSuccess)
        {
            GoToNextLevel();
        }
    }

    private IEnumerator PlayRoundRoutine()
    {
        BuildSequence();
        inputIndex = 0;
        state = RoundState.ShowingSequence;

        SetTilesInteractable(false);
        SetTask(GetLocalizedText(taskWatchSequenceKey, taskWatchSequenceFallback));
        SetResult(string.Empty, Color.white);
        UpdateProgressText();
        SetActionButtonVisible(false);

        if (pauseBeforePlayback > 0f)
        {
            yield return new WaitForSeconds(pauseBeforePlayback);
        }

        float flashDuration = GetFlashDuration();
        float gapDuration = GetGapDuration();

        for (int i = 0; i < sequence.Count; i++)
        {
            int tileIndex = sequence[i];
            if (tileIndex < 0 || tileIndex >= tiles.Length || tiles[tileIndex] == null)
            {
                continue;
            }

            tiles[tileIndex].SetHighlightedVisual();
            yield return new WaitForSeconds(flashDuration);
            tiles[tileIndex].SetNormalVisual();
            yield return new WaitForSeconds(gapDuration);
        }

        state = RoundState.AwaitingInput;
        SetTask(GetLocalizedText(taskRepeatSequenceKey, taskRepeatSequenceFallback));
        SetTilesInteractable(true);
    }

    private void OnRoundCompleted()
    {
        state = RoundState.RoundSuccess;
        SetTilesInteractable(false);
        SetResult(GetLocalizedText(resultCorrectKey, resultCorrectFallback), Color.green);
        SetTask(GetLocalizedText(taskSequenceCompletedKey, taskSequenceCompletedFallback));
        SetActionButtonVisible(true);
        SetActionButtonLabel(currentLevel >= maxLevel
            ? GetLocalizedText(actionFinishKey, actionFinishFallback)
            : GetLocalizedText(actionNextLevelKey, actionNextLevelFallback));
    }

    private void OnRoundFailed()
    {
        state = RoundState.RoundFailed;
        SetTilesInteractable(false);
        SetResult(GetLocalizedText(resultWrongSequenceKey, resultWrongSequenceFallback), Color.red);
        SetTask(GetLocalizedText(taskPressRetryKey, taskPressRetryFallback));
        SetActionButtonVisible(true);
        SetActionButtonLabel(GetLocalizedText(actionRetryKey, actionRetryFallback));
    }

    private void GoToNextLevel()
    {
        currentLevel++;
        if (currentLevel > maxLevel)
        {
            state = RoundState.GameCompleted;
            SetTilesInteractable(false);
            SetTask(GetLocalizedText(resultCompletedKey, resultCompletedFallback));
            SetResult(GetLocalizedText(resultGreatJobKey, resultGreatJobFallback), Color.green);
            progressText.text = $"{sequence.Count}/{sequence.Count}";
            SetActionButtonVisible(false);
            return;
        }

        EnterWaitingState();
        RefreshHeaderTexts();
    }

    private void EnterWaitingState()
    {
        state = RoundState.WaitingToStart;
        SetTilesInteractable(false);
        SetTask(GetLocalizedText(taskPressStartKey, taskPressStartFallback));
        SetResult(string.Empty, Color.white);
        progressText.text = "0/0";
        SetActionButtonVisible(true);
        SetActionButtonLabel(GetLocalizedText(actionStartKey, actionStartFallback));
    }

    private void BuildSequence()
    {
        sequence.Clear();
        int length = GetSequenceLengthForLevel(currentLevel);
        for (int i = 0; i < length; i++)
        {
            sequence.Add(Random.Range(0, tiles.Length));
        }
    }

    private int GetSequenceLengthForLevel(int level)
    {
        if (level <= 3)
        {
            return Mathf.Clamp(2 + level - 1, 2, 3);
        }

        if (level <= 7)
        {
            return Mathf.Clamp(4 + (level - 4) / 2, 4, 5);
        }

        return 6;
    }

    private float GetFlashDuration()
    {
        if (currentLevel <= 3)
        {
            return 0.7f;
        }

        if (currentLevel <= 7)
        {
            return 0.55f;
        }

        return 0.42f;
    }

    private float GetGapDuration()
    {
        if (currentLevel <= 3)
        {
            return 0.3f;
        }

        if (currentLevel <= 7)
        {
            return 0.22f;
        }

        return 0.14f;
    }

    private void InitializeTiles()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null)
            {
                continue;
            }

            tiles[i].Initialize(this, i, (i + 1).ToString());
        }
    }

    private void SetTilesInteractable(bool value)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null)
            {
                continue;
            }

            tiles[i].SetInteractable(value);
            if (!value)
            {
                tiles[i].SetNormalVisual();
            }
        }
    }

    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = $"{inputIndex}/{sequence.Count}";
            TMPTextAutoFit.ApplyTo(progressText, 14f, progressText.fontSize > 0f ? progressText.fontSize : 24f, true, TextOverflowModes.Ellipsis);
        }
    }

    private void RefreshHeaderTexts()
    {
        if (levelText != null)
        {
            levelText.text = $"{GetLocalizedText(levelLabelKey, levelLabelFallback)} {currentLevel}/{maxLevel}";
            TMPTextAutoFit.ApplyTo(levelText, 16f, levelText.fontSize > 0f ? levelText.fontSize : 32f, true, TextOverflowModes.Ellipsis);
        }
    }

    private void SetTask(string value)
    {
        if (taskText != null)
        {
            taskText.text = value;
            TMPTextAutoFit.ApplyTo(taskText, 14f, taskText.fontSize > 0f ? taskText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
        }
    }

    private void SetResult(string value, Color color)
    {
        if (resultText != null)
        {
            resultText.text = value;
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

    private void SetActionButtonLabel(string label)
    {
        if (actionButtonText != null)
        {
            actionButtonText.text = label;
            TMPTextAutoFit.ApplyTo(actionButtonText, 12f, actionButtonText.fontSize > 0f ? actionButtonText.fontSize : 28f, true, TextOverflowModes.Ellipsis);
        }
    }

    private void SetActionButtonVisible(bool visible)
    {
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(visible);
        }
    }

    public void RestartCurrentLevel()
    {
        EnterWaitingState();
        RefreshHeaderTexts();
    }

    public void RestartFromLevel1()
    {
        currentLevel = 1;
        EnterWaitingState();
        RefreshHeaderTexts();
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

    private void OnLanguageChanged(AppLanguage _)
    {
        RefreshTitleText();
        RefreshHeaderTexts();

        switch (state)
        {
            case RoundState.WaitingToStart:
                SetTask(GetLocalizedText(taskPressStartKey, taskPressStartFallback));
                SetActionButtonLabel(GetLocalizedText(actionStartKey, actionStartFallback));
                break;
            case RoundState.ShowingSequence:
                SetTask(GetLocalizedText(taskWatchSequenceKey, taskWatchSequenceFallback));
                break;
            case RoundState.AwaitingInput:
                SetTask(GetLocalizedText(taskRepeatSequenceKey, taskRepeatSequenceFallback));
                break;
            case RoundState.RoundSuccess:
                SetTask(GetLocalizedText(taskSequenceCompletedKey, taskSequenceCompletedFallback));
                SetResult(GetLocalizedText(resultCorrectKey, resultCorrectFallback), Color.green);
                SetActionButtonLabel(currentLevel >= maxLevel
                    ? GetLocalizedText(actionFinishKey, actionFinishFallback)
                    : GetLocalizedText(actionNextLevelKey, actionNextLevelFallback));
                break;
            case RoundState.RoundFailed:
                SetTask(GetLocalizedText(taskPressRetryKey, taskPressRetryFallback));
                SetResult(GetLocalizedText(resultWrongSequenceKey, resultWrongSequenceFallback), Color.red);
                SetActionButtonLabel(GetLocalizedText(actionRetryKey, actionRetryFallback));
                break;
            case RoundState.GameCompleted:
                SetTask(GetLocalizedText(resultCompletedKey, resultCompletedFallback));
                SetResult(GetLocalizedText(resultGreatJobKey, resultGreatJobFallback), Color.green);
                break;
        }
    }
}
