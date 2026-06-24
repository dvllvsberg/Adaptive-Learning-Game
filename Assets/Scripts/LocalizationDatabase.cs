using System;
using System.Collections.Generic;
using UnityEngine;

public enum AppLanguage
{
    Russian = 0,
    Kazakh = 1,
    English = 2
}

[CreateAssetMenu(fileName = "LocalizationDatabase", menuName = "Localization/Database")]
public class LocalizationDatabase : ScriptableObject
{
    [Serializable]
    private struct Entry
    {
        public string key;

        [TextArea(2, 6)]
        public string russian;

        [TextArea(2, 6)]
        public string kazakh;

        [TextArea(2, 6)]
        public string english;
    }

    [SerializeField] private Entry[] entries;

    private Dictionary<string, Entry> lookup;

    public string GetText(string key, AppLanguage language, string fallback = "")
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return NormalizeText(fallback);
        }

        EnsureLookup();

        if (!lookup.TryGetValue(key, out Entry entry))
        {
            return NormalizeText(string.IsNullOrWhiteSpace(fallback) ? key : fallback);
        }

        string value = language switch
        {
            AppLanguage.Russian => entry.russian,
            AppLanguage.Kazakh => entry.kazakh,
            AppLanguage.English => entry.english,
            _ => entry.english
        };

        if (!string.IsNullOrWhiteSpace(value))
        {
            return NormalizeText(value);
        }

        return NormalizeText(string.IsNullOrWhiteSpace(fallback) ? key : fallback);
    }

    private string NormalizeText(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        // Allow users to type "\n" in inspector and still get proper line breaks.
        return value.Replace("\\n", "\n");
    }

    private void EnsureLookup()
    {
        if (lookup != null)
        {
            return;
        }

        lookup = new Dictionary<string, Entry>(StringComparer.Ordinal);

        if (entries == null)
        {
            return;
        }

        for (int i = 0; i < entries.Length; i++)
        {
            Entry entry = entries[i];
            if (string.IsNullOrWhiteSpace(entry.key))
            {
                continue;
            }

            lookup[entry.key] = entry;
        }
    }
}
