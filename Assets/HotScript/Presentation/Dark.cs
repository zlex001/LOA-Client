using Framework;
using Game.Data;
using Config = Game.Data.Config;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using DG.Tweening;

namespace Game.Presentation
{
    public class Dark : UI.Core
    {


        private Coroutine connectingTextCoroutine;
        public override void OnCreate(params object[] args)
        {
            DataManager.Instance.after.Register(DataManager.Type.Servers, OnAfterServersChanged);
            DataManager.Instance.after.Register(DataManager.Type.Dark, OnAfterDataDark);
        }
        public override void OnEnter(params object[] args)
        {
            string text = (string)args[0];
            Text textComponent = transform.Find("Progress/Text").GetComponent<Text>();
            
            if (string.IsNullOrEmpty(text))
            {
                textComponent.text = "●";
            }
            else
            {
                textComponent.text = text;
            }
        }
        public override void OnClose()
        {
            DataManager.Instance.after.Unregister(DataManager.Type.Servers, OnAfterServersChanged);
        }


        private void OnAfterServersChanged(params object[] args)
        {
            List<Server> servers = (List<Server>)args[0];
            if (servers.Count > 0)
            {
                Close();
            }
        }
        private void OnAfterDataDark(params object[] args)
        {
            string v = (string)args[0];
            if (string.IsNullOrEmpty(v))
            {
                Close();
            }
        }
    }
}
