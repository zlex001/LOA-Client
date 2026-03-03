using Game.Data;

namespace Game.Net.Protocol
{
    public class Tutorial : Base
    {
        public enum TargetType
        {
            UI = 1,
            Map = 2,
            Creature = 3,
            Item = 4
        }

        public int step;
        public int targetType;
        public int targetId;
        public string targetPath;
        public int[] targetPos;
        public string hint;

        public bool IsClear => step == 0;

        public override void Processed()
        {
            string posStr = targetPos != null ? $"[{string.Join(",", targetPos)}]" : "null";
            Utils.Debug.Log("Tutorial", $"Protocol received: step={step}, targetType={targetType}, targetId={targetId}, targetPath={targetPath}, targetPos={posStr}, hint={hint}");

            if (IsClear)
            {
                Utils.Debug.Log("Tutorial", "Received clear command (step=0), clearing TutorialStep");
                DataManager.Instance.TutorialStep = null;
            }
            else
            {
                DataManager.Instance.TutorialStep = this.ToLogic();
            }
        }
    }
}
