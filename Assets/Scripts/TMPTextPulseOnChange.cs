using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TMPTextPulseOnChange : MonoBehaviour
{
    [SerializeField] private TMP_Text target;
    [SerializeField] private float pulseScale = 1.08f;
    [SerializeField] private float pulseDuration = 0.18f;
    [SerializeField] private bool ignoreEmptyText = true;
    [SerializeField] private bool useUnscaledTime = true;
    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeStrength = 8f;
    [SerializeField] private int shakeVibrato = 18;

    private string lastText;
    private Vector3 baseScale = Vector3.one;
    private float pulseTime = -1f;
    private float shakeTime = -1f;
    private Vector2 shakeOffset = Vector2.zero;
    private RectTransform rectTransform;
    private Vector2 baseAnchoredPosition = Vector2.zero;

    private void Reset()
    {
        if (target == null)
        {
            target = GetComponent<TMP_Text>();
        }
    }

    private void Awake()
    {
        if (target == null)
        {
            target = GetComponent<TMP_Text>();
        }

        baseScale = transform.localScale;
        lastText = target != null ? target.text : string.Empty;
        rectTransform = transform as RectTransform;
        if (rectTransform != null)
        {
            baseAnchoredPosition = rectTransform.anchoredPosition;
        }
    }

    private void OnEnable()
    {
        baseScale = transform.localScale;
        rectTransform = transform as RectTransform;
        if (rectTransform != null)
        {
            baseAnchoredPosition = rectTransform.anchoredPosition;
        }
        if (target != null)
        {
            lastText = target.text;
        }
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        if (lastText != target.text)
        {
            string current = target.text;
            lastText = current;

            if (!ignoreEmptyText || !string.IsNullOrWhiteSpace(current))
            {
                StartPulse();
            }
        }

        if (pulseTime < 0f)
        {
            transform.localScale = baseScale;
        }
        else
        {
            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            pulseTime += delta;

            float duration = Mathf.Max(0.01f, pulseDuration);
            float t = Mathf.Clamp01(pulseTime / duration);
            float wave = Mathf.Sin(t * Mathf.PI);
            float scale = Mathf.Lerp(1f, pulseScale, wave);
            transform.localScale = baseScale * scale;

            if (t >= 1f)
            {
                transform.localScale = baseScale;
                pulseTime = -1f;
            }
        }

        if (rectTransform != null)
        {
            if (shakeTime >= 0f)
            {
                float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                shakeTime += delta;

                float duration = Mathf.Max(0.01f, shakeDuration);
                float t = Mathf.Clamp01(shakeTime / duration);
                float damper = 1f - t;
                float frequency = Mathf.Max(1, shakeVibrato);
                float x = Mathf.Sin(t * Mathf.PI * frequency) * shakeStrength * damper;
                shakeOffset = new Vector2(x, 0f);

                if (t >= 1f)
                {
                    shakeTime = -1f;
                    shakeOffset = Vector2.zero;
                }
            }

            rectTransform.anchoredPosition = baseAnchoredPosition + shakeOffset;
        }
    }

    public void Pulse()
    {
        if (target == null)
        {
            return;
        }

        if (ignoreEmptyText && string.IsNullOrWhiteSpace(target.text))
        {
            return;
        }

        StartPulse();
    }

    private void StartPulse()
    {
        pulseTime = 0f;
    }

    public void Shake()
    {
        if (target == null)
        {
            return;
        }

        if (ignoreEmptyText && string.IsNullOrWhiteSpace(target.text))
        {
            return;
        }

        shakeTime = 0f;
    }
}
