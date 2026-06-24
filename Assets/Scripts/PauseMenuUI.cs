using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : SceneNavigationBase
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Navigation")]
    [SerializeField] private string sectionSelectSceneName = "MiniGameSelect";

    [Header("Level Restart")]
    [SerializeField] private MonoBehaviour levelRestartHandlerSource;

    private ILevelRestartHandler levelRestartHandler;
    private bool isPaused;

    private void Awake()
    {
        if (levelRestartHandlerSource != null)
        {
            levelRestartHandler = levelRestartHandlerSource as ILevelRestartHandler;
            if (levelRestartHandler == null)
            {
                Debug.LogWarning("PauseMenuUI: levelRestartHandlerSource does not implement ILevelRestartHandler.");
            }
        }
    }

    private void Start()
    {
        SetPauseState(false);
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenPause()
    {
        SetPauseState(true);
    }

    public void Resume()
    {
        SetPauseState(false);
    }

    public void RestartCurrentLevel()
    {
        Resume();

        if (levelRestartHandler != null)
        {
            levelRestartHandler.RestartCurrentLevel();
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RestartFromLevel1()
    {
        Resume();

        if (levelRestartHandler != null)
        {
            levelRestartHandler.RestartFromLevel1();
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToSectionSelect()
    {
        Resume();
        LoadScene(sectionSelectSceneName, nameof(BackToSectionSelect));
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void ExitGame()
    {
        Resume();
        QuitApplication(nameof(ExitGame));
    }

    private void SetPauseState(bool paused)
    {
        isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(paused);
        }

        if (!paused && settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}
