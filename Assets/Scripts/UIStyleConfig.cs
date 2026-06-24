using UnityEngine;

[CreateAssetMenu(fileName = "UIStyleConfig", menuName = "UI/Style Config")]
public class UIStyleConfig : ScriptableObject
{
    [Header("Background")]
    public Color backgroundColor = new Color(0.16f, 0.88f, 0.72f, 1f);
    public Color panelColor = new Color(0.2f, 0.6f, 0.75f, 0.95f);

    [Header("Buttons")]
    public Color buttonNormalColor = Color.white;
    public Color buttonHighlightedColor = new Color(0.94f, 0.94f, 0.94f, 1f);
    public Color buttonPressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
    public Color buttonSelectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public Color buttonDisabledColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
    public float buttonFadeDuration = 0.1f;
    public Vector2 buttonSize = new Vector2(260f, 60f);
    public Color buttonTextColor = new Color(0.16f, 0.16f, 0.16f, 1f);
    public float buttonTextSize = 32f;

    [Header("Text")]
    public Color titleTextColor = Color.white;
    public float titleTextSize = 60f;
    public Color bodyTextColor = Color.white;
    public float bodyTextSize = 30f;
}
