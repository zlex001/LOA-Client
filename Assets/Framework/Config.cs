using UnityEngine;

namespace Framework
{
    public enum Environments
    {
        Development,
        Production
    }

    [CreateAssetMenu(fileName = "Config", menuName = "ScriptableObject/Config", order = 0)]
    public class Config : ScriptableObject
    {
        public string appVersion;
        public string hotVersion;
        public Environments environment;
        public string developmentGateway;
        public string productionGateway;

        public string Gateway => environment == Environments.Development ? developmentGateway : productionGateway;
    }
}
