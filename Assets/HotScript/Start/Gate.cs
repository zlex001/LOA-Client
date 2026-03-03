using Framework;
using Game.Basic;
using Game.Data;
using Game.Net;
using Game.Presentation;
using System;
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
            DataManager.Instance.Dark = Localization.Instance.Get("loading");
            Net.Authentication.Gateway.Request();
        }

        public static void Continue(StartStep next) => flow.Next(next);
    }
}
