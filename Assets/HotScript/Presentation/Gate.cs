using Framework;
using Game.Basic;
using Game.Data;
using Game.Net;
using System;
using UnityEngine;

namespace Game.Presentation
{
    using Protocol = Game.Net.Protocol;

    public class Gate
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

            Game.Basic.Event.Instance.Add("Game.Initialize.Click.Confirm", OnInitializeConfirmClick);
            Game.Basic.Event.Instance.Add("Game.Initialize.Click.Random", OnInitializeRandomClick);

            DataManager.Instance.Dark = Localization.Instance.Get("loading");
            Net.Authentication.Gateway.Request();
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
}
