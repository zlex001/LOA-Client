using System;
using System.Collections.Generic;
using Framework;
using Game.Data;
using Newtonsoft.Json;

namespace Game.Net.Authentication
{
    public static class Gateway
    {
        private static Action _onComplete;

        public static void Request(Action onComplete = null)
        {
            _onComplete = onComplete;
            string gatewayUrl = $"http://{DataManager.Instance.Gateway}:8880";
            Http.Instance.AcceptLanguage = DataManager.Instance.Language.ToString();
            string url = $"{gatewayUrl}/api/authentication/ips";

            Http.Instance.RequestGet(url, OnResponse);
        }

        private static void OnResponse(bool success, string response)
        {
            if (!success)
            {
                Utils.Debug.LogError("Gateway", $"Request failed: {response ?? "Timeout"}");
                string errorKey = string.IsNullOrEmpty(response) ? "connectionTimeout" : "networkError";
                DataManager.Instance.Dark = DataManager.Instance.GetText(errorKey);
                var cb = _onComplete;
                _onComplete = null;
                cb?.Invoke();
                return;
            }

            try
            {
                var gatewayResponse = JsonConvert.DeserializeObject<GatewayResponse>(response);

                if (gatewayResponse?.Servers == null || gatewayResponse.Servers.Count == 0)
                {
                    Utils.Debug.LogError("Gateway", "Parse failed: servers is null or empty");
                    DataManager.Instance.Dark = DataManager.Instance.GetText("parseError");
                    var cb = _onComplete;
                    _onComplete = null;
                    cb?.Invoke();
                    return;
                }

                if (gatewayResponse.Texts != null)
                {
                    DataManager.Instance.Texts = gatewayResponse.Texts;
                    FillPreConnectKeysFromLocal();
                    string lang = DataManager.Instance.Language.ToString();
                    string titleSample = gatewayResponse.Texts.TryGetValue("title", out var t) ? t : "(none)";
                    Utils.Debug.Log("Gateway", $"Texts set for language={lang}, keyCount={gatewayResponse.Texts.Count}, title=\"{titleSample}\"");
                }

                var servers = new List<Server>();
                foreach (var kvp in gatewayResponse.Servers)
                {
                    servers.Add(new Server
                    {
                        Key = kvp.Key,
                        Ip = kvp.Value.Ip,
                        Port = kvp.Value.Port
                    });
                }
                DataManager.Instance.Servers = servers;
            }
            catch (Exception ex)
            {
                Utils.Debug.LogError("Gateway", $"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
                DataManager.Instance.Dark = DataManager.Instance.GetText("parseError");
            }
            var callback = _onComplete;
            _onComplete = null;
            callback?.Invoke();
        }

        private static readonly string[] PreConnectKeys = { "loading", "connecting", "connectionTimeout", "networkError", "parseError" };

        private static void FillPreConnectKeysFromLocal()
        {
            var texts = DataManager.Instance.Texts;
            if (texts == null) return;
            Localization.Instance.Init(DataManager.Instance.Language.ToString());
            var local = Localization.Instance.GetAll();
            foreach (var key in PreConnectKeys)
            {
                if (!texts.ContainsKey(key) && local.TryGetValue(key, out var val))
                    texts[key] = val;
            }
        }
    }
}
