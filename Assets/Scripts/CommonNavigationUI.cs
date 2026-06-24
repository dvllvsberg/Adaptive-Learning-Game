using UnityEngine;

public class CommonNavigationUI : SceneNavigationBase
{
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string miniGameSelectSceneName = "MiniGameSelect";

    public void GoToMainMenu()
    {
        LoadScene(mainMenuSceneName, nameof(GoToMainMenu));
    }

    public void GoToMiniGameSelect()
    {
        LoadScene(miniGameSelectSceneName, nameof(GoToMiniGameSelect));
    }

    public void ExitGame()
    {
        QuitApplication(nameof(ExitGame));
    }
}
