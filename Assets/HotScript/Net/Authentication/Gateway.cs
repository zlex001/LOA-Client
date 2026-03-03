using System;
using System.Collections.Generic;
using Framework;
using Game.Data;
using Newtonsoft.Json;

namespace Game.Net.Authentication
{
    /// <summary>
    /// Gateway module for the Authentication system.
    /// Handles HTTP request to fetch server list and optional UI command from the gateway.
    /// </summary>
    public static class Gateway
    {
        public static void Request()
        {
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
                string errorKey = string.IsNullOrEmpty(response) ? "connection_timeout" : "network_error";
                DataManager.Instance.Dark = Localization.Instance.Get(errorKey);
                return;
            }

            try
            {
                var gatewayResponse = JsonConvert.DeserializeObject<GatewayResponse>(response);

                if (gatewayResponse?.Servers == null || gatewayResponse.Servers.Count == 0)
                {
                    Utils.Debug.LogError("Gateway", "Parse failed: servers is null or empty");
                    DataManager.Instance.Dark = Localization.Instance.Get("parse_error");
                    return;
                }

                DataManager.Instance.Servers.Clear();
                foreach (var serverInfo in gatewayResponse.Servers)
                {
                    var server = new Server
                    {
                        Id = serverInfo.Id,
                        Name = serverInfo.Name,
                        Ip = serverInfo.Ip,
                        Port = 19881 // TODO: Restore to serverInfo.Port after testing
                    };

                    var updatedServers = new List<Server>(DataManager.Instance.Servers);
                    updatedServers.Add(server);
                    DataManager.Instance.Servers = updatedServers;
                }

                if (gatewayResponse.UI != null)
                {
                    DataManager.Instance.GatewayUI = gatewayResponse.UI;
                }
            }
            catch (Exception ex)
            {
                Utils.Debug.LogError("Gateway", $"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
                DataManager.Instance.Dark = Localization.Instance.Get("parse_error");
            }
        }
    }
}
