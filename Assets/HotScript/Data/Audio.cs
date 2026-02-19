using Framework;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    public class Audio : Singleton<Audio>
    {
        private Dictionary<string, AudioSource> bgmPool = new(); // �������ֳ�
        private List<AudioSource> sfxPool = new();               // ��Ч��
        private Dictionary<string, AudioClip> clipCache = new(); // ��Ƶ����


        public void Init()
        {
            DataManager.Instance.befor.Register(DataManager.Type.Option, OnBeforOptionChanged);
        }
        public void Play((string, string, bool, bool) config)
        {
            string path = config.Item1;
            string name = config.Item2;
            bool isBgm = config.Item3;
            bool loop = config.Item4;
            
            if (!isBgm && !DataManager.Instance.User.UISoundEnabled)
            {
                return;
            }
            
            var clip = GetClip(path, name);
            if (clip != null)
            {
                if (isBgm)
                {
                    if (!bgmPool.ContainsKey(name))
                    {
                        var source = CreateSource($"BGM_{name}");
                        source.clip = clip;
                        source.loop = loop;
                        source.Play();
                        bgmPool[name] = source;
                    }
                    else
                    {
                        var source = bgmPool[name];
                        if (!source.isPlaying) { source.Play(); }

                    }
                }
                else
                {
                    var source = CreateSource($"SFX_{name}");
                    source.clip = clip;
                    source.loop = loop;
                    source.Play();
                    sfxPool.Add(source);
                    Destroy(source.gameObject, clip.length + 0.1f);
                }
            }
        }

        public void Stop((string, string, bool, bool) config)
        {
            string name = config.Item2;
            bool isBgm = config.Item3;

            if (isBgm && bgmPool.TryGetValue(name, out var source))
                source.Stop();
        }

        public void StopAll()
        {
            foreach (var s in bgmPool.Values) s.Stop();
            foreach (var s in sfxPool) s.Stop();
        }

        public void Mute(bool mute)
        {
            foreach (var s in bgmPool.Values) s.mute = mute;
            foreach (var s in sfxPool) s.mute = mute;
        }

        public void SetVolume(float bgmVol, float sfxVol)
        {
            foreach (var s in bgmPool.Values) s.volume = bgmVol;
            foreach (var s in sfxPool) s.volume = sfxVol;
        }

        private AudioSource CreateSource(string objName)
        {
            var go = new GameObject(objName);
            go.transform.SetParent(transform, false);
            return go.AddComponent<AudioSource>();
        }

        private AudioClip GetClip(string path, string name)
        {
            string key = $"{path}/{name}";
            if (clipCache.TryGetValue(key, out var clip)) { return clip; }
            clip = AssetManager.Instance.LoadAudioClip(path, name);
            if (clip != null) { clipCache[key] = clip; }
            return clip;
        }
        private void OnBeforOptionChanged(params object[] args)
        {
            OptionData o = (OptionData)args[0];
            OptionData v = (OptionData)args[1];
            if (o == null || o.Empty)
            {
                Play(Config.Audio.OptionOpen);
            }
            else if (v.Empty)
            {
                Play(Config.Audio.OptionClose);
            }
        }
    }
}
