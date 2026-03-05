using System;
using System.Collections.Generic;
using Game.Basic;
using Game.Data;
using UnityEngine;

namespace Game.Net.Protocol
{
    public class Ping : Base
    {
        public DateTime dateTime;
        public Ping(DateTime dateTime)
        {
            this.dateTime = dateTime;
#if UNITY_EDITOR
            Utils.Debug.LogHeartbeat("Protocol", $"Ping created, timestamp: {dateTime}");
#endif
        }
    }

    public class Pong : Base
    {
        public DateTime dateTime;
        public override void Processed()
        {
#if UNITY_EDITOR
            TimeSpan roundTrip = DateTime.Now - DataManager.Instance.Ping;
            TimeSpan serverDelta = dateTime - DataManager.Instance.Ping;
            Utils.Debug.LogHeartbeat("Protocol", $"Pong processed. Server time: {dateTime}, RTT: {roundTrip.TotalMilliseconds:F2}ms, server delta: {serverDelta.TotalMilliseconds:F2}ms");
#endif
            DataManager.Instance.Pong = dateTime;
        }
    }

    public class DataPair : Base
    {
        public enum Type
        {
            Hp,
            Mp,
            Lp,
            Shield
        }
        public Type type;
        public int[] value;
        public string color;
        public override void Processed()
        {
            DataManager.Instance.Change(type, new { value = value, color = color });
        }
    }

    public class QuitToDesktop : Base
    {
        public override void Processed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public class Dark : Base
    {
        public string text;

        public override void Processed()
        {
            DataManager.Instance.Dark = text;
        }
    }

    public class FlyTip : Base
    {
        public string text;

        public override void Processed()
        {
            if (!string.IsNullOrEmpty(text))
            {
                DataManager.Instance.Tip = (TipType.Fly, text);
            }
        }
    }

    public class LanguageChanged : Base
    {
        public string language;

        public override void Processed()
        {
            if (Enum.TryParse(language, out DataManager.Languages lang))
            {
                DataManager.Instance.Language = lang;
            }
        }
    }

    public class ScreenAdaptation : Base
    {
        public int value;

        public override void Processed()
        {
            DataManager.Instance.User.ScreenUIAdaptation = value / 100f;
            Local.Instance.Save(DataManager.Instance.User);

            Game.Basic.Event.Instance.Fire("UI.ScreenUIAdaptation.Changed");
        }
    }

    public class UILock : Base
    {
        public List<string> unlockedPanels;

        public override void Processed()
        {
            DataManager.Instance.UILock = this.ToLogic();
        }
    }

    public class Texts : Base
    {
        public Dictionary<string, string> data;

        public override void Processed()
        {
            var existing = DataManager.Instance.Texts ?? new Dictionary<string, string>();
            foreach (var kv in data)
                existing[kv.Key] = kv.Value;
            DataManager.Instance.Texts = existing;
            Utils.Debug.Log("Protocol", $"Texts protocol processed, keyCount={existing.Count}");
        }
    }

}
