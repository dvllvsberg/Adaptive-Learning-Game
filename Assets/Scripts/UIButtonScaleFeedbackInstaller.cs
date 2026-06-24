using UnityEngine;
using UnityEngine.UI;

public class UIButtonScaleFeedbackInstaller : MonoBehaviour
{
    [SerializeField] private bool includeInactive = true;
    [SerializeField] private bool installOnEnable = true;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.04f;
    [SerializeField] private float pressedScale = 0.96f;
    [SerializeField] private float transitionSeconds = 0.09f;
    [SerializeField] private bool useUnscaledTime = true;

    private void OnEnable()
    {
        if (installOnEnable)
        {
            Install();
        }
    }

    [ContextMenu("Install Scale Feedback To Child Buttons")]
    public void Install()
    {
        Button[] buttons = GetComponentsInChildren<Button>(includeInactive);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
            {
                continue;
            }

            UIButtonScaleFeedback feedback = button.GetComponent<UIButtonScaleFeedback>();
            if (feedback == null)
            {
                feedback = button.gameObject.AddComponent<UIButtonScaleFeedback>();
            }

            ApplyValues(feedback);
        }
    }

    private void ApplyValues(UIButtonScaleFeedback feedback)
    {
        if (feedback == null)
        {
            return;
        }

        feedback.Configure(
            normalScale,
            hoverScale,
            pressedScale,
            transitionSeconds,
            useUnscaledTime);
    }
}
