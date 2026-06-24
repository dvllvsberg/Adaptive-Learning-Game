using UnityEngine;

public class ShapeSelectUI : SectionSelectUIBase
{
    [SerializeField] private string shapeGame1SceneName = "ShapeGame1";

    public void OpenShapeGame1()
    {
        LoadScene(shapeGame1SceneName, nameof(OpenShapeGame1));
    }
}
