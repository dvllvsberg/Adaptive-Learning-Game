using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberTileButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.18f);
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 1f, 0.9f);

    private NumberGame1Controller controller;
    private int tileIndex;

    public void Initialize(NumberGame1Controller owner, int index, string label)
    {
        controller = owner;
        tileIndex = index;

        if (labelText != null)
        {
            labelText.text = label;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPressed);
        }

        SetInteractable(false);
        SetNormalVisual();
    }

    public void SetInteractable(bool value)
    {
        if (button != null)
        {
            button.interactable = value;
        }
    }

    public void SetNormalVisual()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }

    public void SetHighlightedVisual()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = highlightColor;
        }
    }

    private void OnPressed()
    {
        if (controller != null)
        {
            controller.OnTilePressed(tileIndex);
        }
    }
}
