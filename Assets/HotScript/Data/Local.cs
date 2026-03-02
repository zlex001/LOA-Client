using Framework;
using Google.Protobuf;
using System;
using System.IO;
using UnityEngine;


namespace Game.Data
{
    public class Local : Singleton<Local>
    {

        public void Save<T>(T data) where T : IMessage<T> => System.IO.File.WriteAllBytes(System.IO.Path.Combine(Application.persistentDataPath, typeof(T).Name + ".pb"), Serialize(data));


        public T Load<T>() where T : IMessage<T>, new() => System.IO.File.Exists(System.IO.Path.Combine(Application.persistentDataPath, typeof(T).Name + ".pb")) ? Deserialize<T>(System.IO.File.ReadAllBytes(System.IO.Path.Combine(Application.persistentDataPath, typeof(T).Name + ".pb"))) : default;


        private byte[] Serialize<T>(T data) where T : IMessage<T>
        {
            using (MemoryStream stream = new MemoryStream())
            {
                if (data is IMessage protobuf)
                {
                    protobuf.WriteTo(stream);
                    return stream.ToArray();
                }
                else
                {
                    throw new ArgumentException("Data must implement IMessage interface for Protocol Buffers serialization.");
                }
            }
        }

        private T Deserialize<T>(byte[] data) where T : IMessage<T>, new()
        {
            T instance = new T();
            instance.MergeFrom(data);
            return instance;
        }


    }
}