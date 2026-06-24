using UnityEngine;

public class ColourSelectUI : SectionSelectUIBase
{
    [SerializeField] private string colourGame1SceneName = "ColourGame1";

    public void OpenColourGame1()
    {
        LoadScene(colourGame1SceneName, nameof(OpenColourGame1));
    }
}
