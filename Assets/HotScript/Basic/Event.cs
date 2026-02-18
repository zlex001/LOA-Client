using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Basic
{
    public class Event
    {
        private static Event instance;
        public static Event Instance => instance ??= new Event();

        private readonly Dictionary<object, EventData> table = new();

        public bool Add(object key, Action<object[]> handler)
        {
            if (!table.TryGetValue(key, out var data))
            {
                data = new EventData(key);
                table[key] = data;
            }
            return data.AddHandler(handler);
        }

        public void Remove(object key, Action<object[]> handler)
        {
            if (table.TryGetValue(key, out var data))
            {
                data.RemoveHandler(handler);
            }
        }

        public void Fire(object key, params object[] args)
        {
            string keyStr = key is Enum ? $"{key.GetType().Name}.{key}" : key.ToString();
            if (table.TryGetValue(key, out var data))
            {
                data.Invoke(args);
            }
        }

        class EventData
        {
            private readonly object key;
            private readonly List<Delegate> handlers = new();
            private readonly List<Delegate> addQueue = new();
            private readonly List<Delegate> removeQueue = new();
            private bool isInvoking = false;
            private bool dirty = false;

            public EventData(object key) => this.key = key;

            public bool AddHandler(Delegate handler)
            {
                if (handlers.Contains(handler)) return false;

                if (isInvoking)
                {
                    dirty = true;
                    addQueue.Add(handler);
                }
                else
                {
                    handlers.Add(handler);
                }

                return true;
            }

            public void RemoveHandler(Delegate handler)
            {
                if (isInvoking)
                {
                    dirty = true;
                    removeQueue.Add(handler);
                }
                else
                {
                    handlers.Remove(handler);
                }
            }

            public void Invoke(params object[] args)
            {
                isInvoking = true;
                foreach (var handler in handlers)
                {
                    if (handler is Action<object[]> action)
                        action.Invoke(args);
                }
                isInvoking = false;

                if (dirty)
                {
                    foreach (var h in addQueue) handlers.Add(h);
                    foreach (var h in removeQueue) handlers.Remove(h);
                    addQueue.Clear();
                    removeQueue.Clear();
                    dirty = false;
                }
            }
        }
    }
}
