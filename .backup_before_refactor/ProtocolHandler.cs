using System;
using Game.Data;
using Game.Net.Protocol;

namespace Game.Logic
{
    /// <summary>
    /// Protocol Handler - Handles business logic when protocol is received
    /// This separates data structure (Protocol) from business logic (Handler)
    /// </summary>
    public static class ProtocolHandler
    {
        public static void Initialize()
        {
            // Register protocol handlers
            RegisterHandler<Pong>(HandlePong);
            RegisterHandler<LoginResponse>(HandleLoginResponse);
            RegisterHandler<InitialResponse>(HandleInitialResponse);
            RegisterHandler<Home>(HandleHome);
            RegisterHandler<Scene>(HandleScene);
            RegisterHandler<Characters>(HandleCharacters);
            RegisterHandler<BattleProgress>(HandleBattleProgress);
            RegisterHandler<Information>(HandleInformation);
            RegisterHandler<Option>(HandleOption);
            RegisterHandler<WorldMap>(HandleWorldMap);
            RegisterHandler<Story>(HandleStory);
            RegisterHandler<Tutorial>(HandleTutorial);
            RegisterHandler<UILock>(HandleUILock);
        }
        
        private static void RegisterHandler<T>(Action<T> handler) where T : Base
        {
            // TODO: Implement protocol routing mechanism
            // This will be called when Net receives protocol of type T
        }
        
        #region Protocol Handlers
        
        private static void HandlePong(Pong protocol)
        {
#if UNITY_EDITOR
            TimeSpan roundTrip = DateTime.Now - DataManager.Instance.Ping;
            TimeSpan serverDelta = protocol.dateTime - DataManager.Instance.Ping;
            Utils.Debug.LogHeartbeat("Protocol", $"Pong processed. Server time: {protocol.dateTime}, Round trip: {roundTrip.TotalMilliseconds:F2}ms, Server processing: {serverDelta.TotalMilliseconds:F2}ms");
#endif
            DataManager.Instance.Pong = protocol.dateTime;
        }
        
        private static void HandleLoginResponse(LoginResponse protocol)
        {
            // TODO: Migrate from Protocol.cs Processed()
        }
        
        private static void HandleInitialResponse(InitialResponse protocol)
        {
            // TODO: Migrate from Protocol.cs Processed()
        }
        
        private static void HandleHome(Home protocol)
        {
            // TODO: Migrate from Protocol.cs Processed()
        }
        
        private static void HandleScene(Scene protocol)
        {
            // TODO: Migrate from Protocol.cs Processed()
        }
        
        private static void HandleCharacters(Characters protocol)
        {
            // TODO: Migrate from Protocol.cs Processed()
        }
        
        private static void HandleBattleProgress(BattleProgress protocol)
        {
            // TODO: Migrate from Protocol.cs Processed()
        }
        
        private static void HandleInformation(Information protocol)
        {
            // TODO: Migrate from Protocol.cs Processed()
        }
        
        private static void HandleOption(Option protocol)
        {
            // TODO: Migrate from Protocol.cs Processed()
        }
        
        private static void HandleWorldMap(WorldMap protocol)
        {
            // TODO: Migrate from Protocol.cs Processed()
        }
        
        private static void HandleStory(Story protocol)
        {
            // TODO: Migrate from Protocol.cs Processed()
        }
        
        private static void HandleTutorial(Tutorial protocol)
        {
            // TODO: Migrate from Protocol.cs Processed()
        }
        
        private static void HandleUILock(UILock protocol)
        {
            // TODO: Migrate from Protocol.cs Processed()
        }
        
        #endregion
    }
}
