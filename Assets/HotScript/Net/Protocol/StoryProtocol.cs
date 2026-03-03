using System.Collections.Generic;
using System.Linq;
using Game.Data;

namespace Game.Net.Protocol
{
    public class Story : Base
    {
        public class Dialogue
        {
            public string character;
            public string words;
        }

        public List<Dialogue> dialogues;

        public override void Processed()
        {
            DataManager.Instance.StoryDialogues = dialogues?.Select(d => d.ToLogic()).ToList();
        }
    }

    public class StoryComplete : Base { }
}
