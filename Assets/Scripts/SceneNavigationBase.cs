using UnityEngine;

public abstract class SceneNavigationBase : MonoBehaviour
{
    protected void LoadScene(string sceneName, string callerName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning($"{callerName}: scene name is empty on {name}.");
            return;
        }

        SceneTransitionController.TransitionToScene(sceneName);
    }

    protected void QuitApplication(string callerName)
    {
        Application.Quit();
        Debug.Log($"{callerName}: exit requested.");
    }
}
