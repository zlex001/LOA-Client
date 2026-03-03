using Framework;
using Game.Basic;
using Game.Data;
using Game.Logic;
using Game.Net;
using Game.Presentation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Start
{
    using Config = Game.Data.Config;
    using Protocol = Game.Net.Protocol;

    public enum StartStep
    {
        RequestGateway,
    }

    public  class Gate
    {
        public static void Entrance(string gateway)
        {
            DataManager.Instance.Gateway = gateway;
            
            string languageStr = UnityEngine.PlayerPrefs.GetString("LANGUAGE", "ChineseSimplified");
            
            if (Enum.TryParse(languageStr, out DataManager.Languages language))
            {
                DataManager.Instance.Language = language;
            }
            
            DataManager.Instance.Init();
            UI.Instance.Init(9f, 16f);
            Audio.Instance.Init();
            NetManager.Instance.Init();
            Authentication.Init();
            
            // Register event listeners for layer separation (Data -> Start -> Net)
            Game.Basic.Event.Instance.Add("Game.Initialize.Click.Confirm", OnInitializeConfirmClick);
            Game.Basic.Event.Instance.Add("Game.Initialize.Click.Random", OnInitializeRandomClick);
            
            StartupFlowManager.Start();
        }
        
        private static void OnInitializeConfirmClick(params object[] args)
        {
            string name = (string)args[0];
            NetManager.Instance.Send(new Protocol.InitializeConfirm(name));
        }

        private static void OnInitializeRandomClick(params object[] args)
        {
            NetManager.Instance.Send(new Protocol.InitializeRandom());
        }
    }
    public static class StartupFlowManager
    {
        private static Flow<StartStep> flow = new();
        public static void Next(StartStep step) => flow.Next(step);
        public static void Goto(StartStep step) => flow.Goto(step);
        public static StartStep Current => flow.Current;
        public static void Start()
        {
            RegisterSteps();
            flow.Start(StartStep.RequestGateway);
        }

        private static void RegisterSteps()
        {
            flow.Register(StartStep.RequestGateway, OnRequestGateway);
        }

        private static void OnRequestGateway()
        {
            string gatewayUrl = $"http://{DataManager.Instance.Gateway}:8880";

            Http.Instance.AcceptLanguage = DataManager.Instance.Language.ToString();
            string url = $"{gatewayUrl}/api/authentication/ips";
            
            UI.Instance.Open(Config.UI.Dark, Localization.Instance.Get("loading"));
            Http.Instance.RequestGet(url, OnGatewayResponse);
        }

        private static void OnGatewayResponse(bool success, string response)
        {
            if (!success)
            {
                Debug.LogError($"[Gateway] Request failed: {response ?? "Timeout"}");
                UI.Instance.Open(Config.UI.Dark, Localization.Instance.Get(string.IsNullOrEmpty(response) ? "connection_timeout" : "network_error"));
                return;
            }

            try
            {
                var gatewayResponse = JsonConvert.DeserializeObject<GatewayResponse>(response);
                
                if (gatewayResponse == null || gatewayResponse.Servers == null || gatewayResponse.Servers.Count == 0)
                {
                    Debug.LogError($"[Gateway] Parse failed: servers is null or empty");
                    UI.Instance.Open(Config.UI.Dark, Localization.Instance.Get("parse_error"));
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
                    
                    List<Server> updatedServers = new List<Server>(DataManager.Instance.Servers);
                    updatedServers.Add(server);
                    DataManager.Instance.Servers = updatedServers;
                }

                // Always show Start UI, let user click to proceed
                if (gatewayResponse.UI != null)
                {
                    UI.Instance.Close(Config.UI.Dark);
                    var uiConfig = typeof(Config.UI).GetField(gatewayResponse.UI.Name).GetValue(null);
                    UI.Instance.Open(((string, string, int, bool))uiConfig, gatewayResponse.UI.Data);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Gateway] Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
                UI.Instance.Open(Config.UI.Dark, Localization.Instance.Get("parse_error"));
            }
        }

        public static void Continue(StartStep next) => flow.Next(next);
    }
}
