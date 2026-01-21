using System.Collections.Generic;
using UnityEngine;

public class Localization
{
    private static Localization _instance;
    public static Localization Instance => _instance ??= new Localization();

    private Dictionary<string, string> _texts;
    private Dictionary<string, string> _serverTexts;
    private string _currentLanguage;

    public string CurrentLanguage => _currentLanguage;
    public string StandardLanguageCode => LanguageDetector.InternalToStandardCode(_currentLanguage);

    public void Init(string language)
    {
        _currentLanguage = language;
        string path = $"Localization_{language}";
        TextAsset textAsset = Resources.Load<TextAsset>(path);

        if (textAsset != null)
        {
            _texts = ParseJson(textAsset.text);
        }
        else
        {
            Debug.LogError($"[Localization] Failed to load: {path}");
            _texts = new Dictionary<string, string>();
        }
    }

    public void MergeServerTexts(Dictionary<string, string> serverTexts)
    {
        if (_serverTexts == null)
            _serverTexts = new Dictionary<string, string>();

        foreach (var kvp in serverTexts)
        {
            _serverTexts[kvp.Key] = kvp.Value;
        }
    }

    public string Get(string key, params object[] args)
    {
        // Auto-initialize if not initialized yet (for hot-loaded assemblies)
        if (_texts == null)
        {
            string savedLanguage = UnityEngine.PlayerPrefs.GetString("LANGUAGE", "ChineseSimplified");
            Init(savedLanguage);
        }

        string text = null;

        if (_serverTexts != null && _serverTexts.TryGetValue(key, out text))
        {
        }
        else if (_texts != null && _texts.TryGetValue(key, out text))
        {
        }
        else
        {
            Debug.LogWarning($"[Localization] Missing key: {key}");
            return key;
        }

        return args.Length > 0 ? string.Format(text, args) : text;
    }

    private Dictionary<string, string> ParseJson(string json)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        
        json = json.Trim();
        if (!json.StartsWith("{") || !json.EndsWith("}"))
            return dict;

        json = json.Substring(1, json.Length - 2);
        
        string[] pairs = json.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string pair in pairs)
        {
            int colonIndex = pair.IndexOf(':');
            if (colonIndex > 0)
            {
                string key = pair.Substring(0, colonIndex).Trim().Trim('"');
                string value = pair.Substring(colonIndex + 1).Trim().Trim('"');
                dict[key] = value;
            }
        }

        return dict;
    }
}

