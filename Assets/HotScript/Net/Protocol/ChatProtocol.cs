using System.Collections.Generic;
using Game.Data;

namespace Game.Net.Protocol
{
    public class Information : Base
    {
        public enum Channel
        {
            System,
            Private,
            Local,
            Battle,
            All,
            Rumor,
            Automation,
        }
        public string message;
        public Channel channel;
        public override void Processed()
        {
            var informations = new List<InformationData>(DataManager.Instance.Informations);
            informations.Add(this.ToLogic());
            if (informations.Count > 250) { informations.RemoveAt(0); }
            DataManager.Instance.Informations = informations;
        }
    }

    public class Chat : Base
    {
        public string content;
        public Chat(string content)
        {
            this.content = content;
        }
    }
}
