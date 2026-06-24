using UnityEngine;

public class NumbersSelectUI : SectionSelectUIBase
{
    [SerializeField] private string numbersGame1SceneName = "NumberGame1";

    public void OpenNumbersGame1()
    {
        LoadScene(numbersGame1SceneName, nameof(OpenNumbersGame1));
    }
}
