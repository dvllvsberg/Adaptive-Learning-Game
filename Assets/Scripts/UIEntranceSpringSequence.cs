using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEntranceSpringSequence : MonoBehaviour
{
    [Header("Playback")]
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private float startDelay = 0.22f;
    [SerializeField] private float stagger = 0.045f;
    [SerializeField] private float duration = 0.28f;

    [Header("Spring")]
    [SerializeField] private float startScale = 0.82f;
    [SerializeField] private float overshoot = 1.5f;
    [SerializeField] private bool includeDirectChildrenOnly = true;
    [SerializeField] private bool includeInactiveChildren = false;

    private readonly List<Entry> entries = new List<Entry>(32);
    private Coroutine playCoroutine;

    private struct Entry
    {
        public RectTransform rect;
        public CanvasGroup group;
        public Vector3 baseScale;
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Play();
        }
    }

    [ContextMenu("Play Entrance Sequence")]
    public void Play()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
        }

        BuildEntryList();
        if (entries.Count == 0)
        {
            return;
        }

        playCoroutine = StartCoroutine(PlayRoutine());
    }

    private void BuildEntryList()
    {
        entries.Clear();

        if (includeDirectChildrenOnly)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (!includeInactiveChildren && !child.gameObject.activeInHierarchy)
                {
                    continue;
                }

                TryAddEntry(child as RectTransform);
            }

            return;
        }

        Graphic[] graphics = GetComponentsInChildren<Graphic>(includeInactiveChildren);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic graphic = graphics[i];
            if (graphic == null)
            {
                continue;
            }

            if (!includeInactiveChildren && !graphic.gameObject.activeInHierarchy)
            {
                continue;
            }

            TryAddEntry(graphic.rectTransform);
        }
    }

    private void TryAddEntry(RectTransform rect)
    {
        if (rect == null || rect == transform as RectTransform)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].rect == rect)
            {
                return;
            }
        }

        CanvasGroup group = rect.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = rect.gameObject.AddComponent<CanvasGroup>();
        }

        entries.Add(new Entry
        {
            rect = rect,
            group = group,
            baseScale = rect.localScale
        });
    }

    private IEnumerator PlayRoutine()
    {
        float initialDelay = Mathf.Max(0f, startDelay);
        if (initialDelay > 0f)
        {
            float delayElapsed = 0f;
            while (delayElapsed < initialDelay)
            {
                delayElapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }
        }

        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            if (entry.rect == null || entry.group == null)
            {
                continue;
            }

            entry.group.alpha = 0f;
            entry.rect.localScale = entry.baseScale * startScale;
            entries[i] = entry;
        }

        float totalDuration = Mathf.Max(0.05f, duration);
        float spacing = Mathf.Max(0f, stagger);
        float sequenceDuration = totalDuration + (entries.Count - 1) * spacing;
        float elapsed = 0f;

        while (elapsed < sequenceDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                if (entry.rect == null || entry.group == null)
                {
                    continue;
                }

                float local = Mathf.Clamp01((elapsed - i * spacing) / totalDuration);
                if (local <= 0f)
                {
                    continue;
                }

                float spring = EaseOutBack(local, overshoot);
                entry.rect.localScale = Vector3.LerpUnclamped(entry.baseScale * startScale, entry.baseScale, spring);
                entry.group.alpha = Mathf.SmoothStep(0f, 1f, local);
            }

            yield return null;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            if (entry.rect == null || entry.group == null)
            {
                continue;
            }

            entry.rect.localScale = entry.baseScale;
            entry.group.alpha = 1f;
        }

        playCoroutine = null;
    }

    private static float EaseOutBack(float t, float amount)
    {
        float x = Mathf.Clamp01(t) - 1f;
        return 1f + (amount + 1f) * x * x * x + amount * x * x;
    }
}
