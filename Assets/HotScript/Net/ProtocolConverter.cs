using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Net.Protocol;

namespace Game.Net
{
    /// <summary>
    /// Protocol to Logic data model converter
    /// Handles conversion from network Protocol types to Logic data models
    /// </summary>
    public static class ProtocolConverter
    {
        #region Map Conversion
        
        public static MapData ToLogic(this Protocol.Map protocol)
        {
            if (protocol == null) return null;
            return new MapData
            {
                pos = protocol.pos,
                name = protocol.name,
                color = protocol.color
            };
        }

        #endregion

        #region Scene Conversion
        
        public static SceneData ToLogic(this Protocol.Scene protocol)
        {
            if (protocol == null) return null;
            return new SceneData
            {
                pos = protocol.pos,
                maps = protocol.maps?.Select(m => m.ToLogic()).ToList(),
                sceneName = protocol.sceneName,
                sortedMaps = protocol.sortedMaps?.Select(m => m.ToLogic()).ToList()
            };
        }

        #endregion

        #region Character Conversion
        
        public static CharacterData ToLogic(this Protocol.Characters.CharacterData protocol)
        {
            if (protocol == null) return null;
            return new CharacterData
            {
                name = protocol.name,
                progress = protocol.progress,
                hash = protocol.hash,
                configId = protocol.configId
            };
        }

        #endregion

        #region Home Conversion
        
        public static HomeData ToLogic(this Protocol.Home protocol)
        {
            if (protocol == null) return null;
            return new HomeData
            {
                scene = protocol.scene?.ToLogic(),
                characters = protocol.characters?.content?.Select(c => c.ToLogic()).ToList(),
                resouse = protocol.resouse?.ToDictionary(
                    kv => kv.Key,
                    kv => new ResourceInfo
                    {
                        currentValue = kv.Value.CurrentValue,
                        maxValue = kv.Value.MaxValue,
                        color = kv.Value.Color
                    }
                ),
                area = protocol.area,
                ui = protocol.ui != null ? new HomeUITexts
                {
                    resourceLabels = protocol.ui.resourceLabels,
                    channels = protocol.ui.channels,
                    chatPlaceholder = protocol.ui.chatPlaceholder
                } : null
            };
        }

        #endregion

        #region Initialize Conversion
        
        public static InitializeData ToLogic(this Protocol.Initialize protocol)
        {
            if (protocol == null) return null;
            return new InitializeData
            {
                description = protocol.description,
                grade = protocol.grade,
                ui = protocol.ui != null ? new InitializeUITexts
                {
                    namePlaceholder = protocol.ui.namePlaceholder,
                    randomButton = protocol.ui.randomButton,
                    confirmButton = protocol.ui.confirmButton,
                    errorNameEmpty = protocol.ui.errorNameEmpty,
                    errorNameUnsafe = protocol.ui.errorNameUnsafe
                } : null
            };
        }

        #endregion

        #region Option Conversion
        
        public static OptionData ToLogic(this Protocol.Option protocol)
        {
            if (protocol == null) return null;
            return new OptionData
            {
                lefts = protocol.lefts?.Select(item => new OptionItemData
                {
                    type = (OptionItemType)(int)item.type,
                    data = item.data
                }).ToList(),
                rights = protocol.rights?.Select(item => new OptionItemData
                {
                    type = (OptionItemType)(int)item.type,
                    data = item.data
                }).ToList()
            };
        }

        #endregion

        #region WorldMap Conversion
        
        public static WorldMapData ToLogic(this Protocol.WorldMap protocol)
        {
            if (protocol == null) return null;
            return new WorldMapData
            {
                scenes = protocol.scenes?.Select(scene => new WorldMapSceneInfo
                {
                    pos = scene.pos,
                    sceneName = scene.sceneName,
                    color = scene.color,
                    type = scene.type
                }).ToList()
            };
        }

        #endregion

        #region UILock Conversion
        
        public static UILockData ToLogic(this Protocol.UILock protocol)
        {
            if (protocol == null) return null;
            return new UILockData
            {
                unlockedPanels = protocol.unlockedPanels
            };
        }

        #endregion

        #region Tutorial Conversion
        
        public static TutorialData ToLogic(this Protocol.Tutorial protocol)
        {
            if (protocol == null) return null;
            return new TutorialData
            {
                step = protocol.step,
                targetType = protocol.targetType,
                targetId = protocol.targetId,
                targetPath = protocol.targetPath,
                targetPos = protocol.targetPos,
                hint = protocol.hint
            };
        }

        #endregion

        #region Story Conversion
        
        public static DialogueData ToLogic(this Protocol.Story.Dialogue protocol)
        {
            if (protocol == null) return null;
            return new DialogueData
            {
                character = protocol.character,
                words = protocol.words
            };
        }

        #endregion

        #region Information Conversion
        
        public static InformationData ToLogic(this Protocol.Information protocol)
        {
            if (protocol == null) return null;
            return new InformationData
            {
                message = protocol.message,
                channel = (InformationChannel)(int)protocol.channel
            };
        }

        #endregion
    }
}
