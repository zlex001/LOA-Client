using System;
using UnityEngine;

namespace Framework
{
    public class LanguageDetector
    {
        public enum DetectedLanguage
        {
            ChineseSimplified,
            ChineseTraditional,
            English,
            Japanese,
            Korean,
            French,
            German,
            Spanish,
            Portuguese,
            Russian,
            Turkish,
            Thai,
            Indonesian,
            Vietnamese,
            Italian,
            Polish,
            Dutch,
            Swedish,
            Norwegian,
            Danish,
            Finnish,
            Ukrainian,
            Unknown
        }

        public static DetectedLanguage DetectFromSystem()
        {
            SystemLanguage systemLang = Application.systemLanguage;

            DetectedLanguage detected = systemLang switch
            {
                SystemLanguage.Chinese => DetectedLanguage.ChineseSimplified,
                SystemLanguage.ChineseSimplified => DetectedLanguage.ChineseSimplified,
                SystemLanguage.ChineseTraditional => DetectedLanguage.ChineseTraditional,
                SystemLanguage.English => DetectedLanguage.English,
                SystemLanguage.Japanese => DetectedLanguage.Japanese,
                SystemLanguage.Korean => DetectedLanguage.Korean,
                SystemLanguage.French => DetectedLanguage.French,
                SystemLanguage.German => DetectedLanguage.German,
                SystemLanguage.Spanish => DetectedLanguage.Spanish,
                SystemLanguage.Portuguese => DetectedLanguage.Portuguese,
                SystemLanguage.Russian => DetectedLanguage.Russian,
                SystemLanguage.Turkish => DetectedLanguage.Turkish,
                SystemLanguage.Thai => DetectedLanguage.Thai,
                SystemLanguage.Indonesian => DetectedLanguage.Indonesian,
                SystemLanguage.Vietnamese => DetectedLanguage.Vietnamese,
                SystemLanguage.Italian => DetectedLanguage.Italian,
                SystemLanguage.Polish => DetectedLanguage.Polish,
                SystemLanguage.Dutch => DetectedLanguage.Dutch,
                SystemLanguage.Swedish => DetectedLanguage.Swedish,
                SystemLanguage.Norwegian => DetectedLanguage.Norwegian,
                SystemLanguage.Danish => DetectedLanguage.Danish,
                SystemLanguage.Finnish => DetectedLanguage.Finnish,
                SystemLanguage.Ukrainian => DetectedLanguage.Ukrainian,
                _ => DetectedLanguage.English
            };

            return detected;
        }

        public static void DetectFromServer(string gateway, Action<string> callback)
        {
            string url = $"http://{gateway}:8880/api/authentication/language-detect";
            
            Http.Instance.RequestPost(url, "{}", (success, response) =>
            {
                if (success)
                {
                    try
                    {
                        var data = JsonUtility.FromJson<LanguageDetectResponse>(response);
                        
                        if (!string.IsNullOrEmpty(data.language))
                        {
                            string internalLanguage = StandardCodeToInternal(data.language);
                            callback(internalLanguage);
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        // Failed to parse server response, fallback to system detection
                    }
                }

                callback(null);
            });
        }

        public static string GetLanguageString(DetectedLanguage language)
        {
            return language switch
            {
                DetectedLanguage.ChineseSimplified => "ChineseSimplified",
                DetectedLanguage.ChineseTraditional => "ChineseTraditional",
                DetectedLanguage.English => "English",
                DetectedLanguage.Japanese => "Japanese",
                DetectedLanguage.Korean => "Korean",
                DetectedLanguage.French => "French",
                DetectedLanguage.German => "German",
                DetectedLanguage.Spanish => "Spanish",
                DetectedLanguage.Portuguese => "Portuguese",
                DetectedLanguage.Russian => "Russian",
                DetectedLanguage.Turkish => "Turkish",
                DetectedLanguage.Thai => "Thai",
                DetectedLanguage.Indonesian => "Indonesian",
                DetectedLanguage.Vietnamese => "Vietnamese",
                DetectedLanguage.Italian => "Italian",
                DetectedLanguage.Polish => "Polish",
                DetectedLanguage.Dutch => "Dutch",
                DetectedLanguage.Swedish => "Swedish",
                DetectedLanguage.Norwegian => "Norwegian",
                DetectedLanguage.Danish => "Danish",
                DetectedLanguage.Finnish => "Finnish",
                DetectedLanguage.Ukrainian => "Ukrainian",
                _ => "English"
            };
        }

        public static string StandardCodeToInternal(string standardCode)
        {
            if (string.IsNullOrEmpty(standardCode))
                return "English";

            string lower = standardCode.ToLower();
            return lower switch
            {
                "zh-cn" or "zh-hans" or "zh-chs" or "zh" or "chinesesimplified" => "ChineseSimplified",
                "zh-tw" or "zh-hant" or "zh-cht" or "zh-hk" or "zh-mo" or "chinesetraditional" => "ChineseTraditional",
                "en" or "en-us" or "en-gb" or "english" => "English",
                "ja" or "ja-jp" or "japanese" => "Japanese",
                "ko" or "ko-kr" or "korean" => "Korean",
                "fr" or "fr-fr" or "french" => "French",
                "de" or "de-de" or "german" => "German",
                "es" or "es-es" or "spanish" => "Spanish",
                "pt" or "pt-br" or "pt-pt" or "portuguese" => "Portuguese",
                "ru" or "ru-ru" or "russian" => "Russian",
                "tr" or "tr-tr" or "turkish" => "Turkish",
                "th" or "th-th" or "thai" => "Thai",
                "id" or "id-id" or "indonesian" => "Indonesian",
                "vi" or "vi-vn" or "vietnamese" => "Vietnamese",
                "it" or "it-it" or "italian" => "Italian",
                "pl" or "pl-pl" or "polish" => "Polish",
                "nl" or "nl-nl" or "dutch" => "Dutch",
                "sv" or "sv-se" or "swedish" => "Swedish",
                "no" or "nb" or "nn" or "norwegian" => "Norwegian",
                "da" or "da-dk" or "danish" => "Danish",
                "fi" or "fi-fi" or "finnish" => "Finnish",
                "uk" or "uk-ua" or "ukrainian" => "Ukrainian",
                _ => "English"
            };
        }

        public static string InternalToStandardCode(string internalName)
        {
            return internalName switch
            {
                "ChineseSimplified" => "zh-CN",
                "ChineseTraditional" => "zh-TW",
                "English" => "en",
                "Japanese" => "ja",
                "Korean" => "ko",
                "French" => "fr",
                "German" => "de",
                "Spanish" => "es",
                "Portuguese" => "pt",
                "Russian" => "ru",
                "Turkish" => "tr",
                "Thai" => "th",
                "Indonesian" => "id",
                "Vietnamese" => "vi",
                "Italian" => "it",
                "Polish" => "pl",
                "Dutch" => "nl",
                "Swedish" => "sv",
                "Norwegian" => "no",
                "Danish" => "da",
                "Finnish" => "fi",
                "Ukrainian" => "uk",
                _ => "en"
            };
        }

        public static string DetermineLanguage()
        {
            if (PlayerPrefs.HasKey("LANGUAGE"))
            {
                return PlayerPrefs.GetString("LANGUAGE");
            }

            DetectedLanguage systemLang = DetectFromSystem();
            return GetLanguageString(systemLang);
        }

        [System.Serializable]
        private class LanguageDetectResponse
        {
            public string language;
        }
    }
}
