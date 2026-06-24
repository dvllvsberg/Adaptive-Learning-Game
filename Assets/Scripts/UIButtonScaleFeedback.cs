using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonScaleFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform target;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.04f;
    [SerializeField] private float pressedScale = 0.96f;
    [SerializeField] private float transitionSeconds = 0.09f;
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine scaleCoroutine;
    private bool isHovered;
    private bool isPressed;

    private void Reset()
    {
        if (target == null)
        {
            target = transform as RectTransform;
        }
    }

    private void Awake()
    {
        if (target == null)
        {
            target = transform as RectTransform;
        }
    }

    private void OnEnable()
    {
        ApplyImmediate(normalScale);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        AnimateTo(CurrentTargetScale());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
        AnimateTo(CurrentTargetScale());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        AnimateTo(CurrentTargetScale());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        AnimateTo(CurrentTargetScale());
    }

    private float CurrentTargetScale()
    {
        if (isPressed)
        {
            return pressedScale;
        }

        return isHovered ? hoverScale : normalScale;
    }

    private void AnimateTo(float scale)
    {
        if (target == null)
        {
            return;
        }

        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        scaleCoroutine = StartCoroutine(ScaleRoutine(scale));
    }

    private IEnumerator ScaleRoutine(float targetScale)
    {
        Vector3 from = target.localScale;
        Vector3 to = Vector3.one * targetScale;

        float duration = Mathf.Max(0.01f, transitionSeconds);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        target.localScale = to;
        scaleCoroutine = null;
    }

    private void ApplyImmediate(float scale)
    {
        if (target != null)
        {
            target.localScale = Vector3.one * scale;
        }
    }

    public void Configure(
        float normal,
        float hover,
        float pressed,
        float transition,
        bool unscaledTime)
    {
        normalScale = normal;
        hoverScale = hover;
        pressedScale = pressed;
        transitionSeconds = transition;
        useUnscaledTime = unscaledTime;
    }
}
