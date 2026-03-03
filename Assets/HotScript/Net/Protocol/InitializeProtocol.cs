using System.Collections.Generic;
using Game.Data;

namespace Game.Net.Protocol
{
    public class Initialize : Base
    {
        public class UITexts
        {
            public string namePlaceholder;
            public string randomButton;
            public string confirmButton;
            public string errorNameEmpty;
            public string errorNameUnsafe;
        }

        public string description;
        public Dictionary<string, int> grade = new Dictionary<string, int>();
        public UITexts ui;

        public override void Processed()
        {
            DataManager.Instance.Initialize = this.ToLogic();
        }
    }

    public class InitializeRandom : Base
    {
    }

    public class InitializeConfirm : Base
    {
        public InitializeConfirm(string name)
        {
            this.name = name;
        }
        public string name;
    }

    public class InitialResponse : Base
    {
        public enum Code
        {
            Success,
            Empty,
            TooLong,
            Unsafe,
            AlreadyExsit,
        }
        public int code;
        public string message;

        public override void Processed()
        {
            DataManager.Instance.InitialResponseMessage = message;
            DataManager.Instance.InitialResponse = code;
        }
    }
}
