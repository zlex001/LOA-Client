using System;
using System.Collections.Generic;

namespace Game.Basic
{
    public class Monitor
    {
        public delegate void Function(params object[] args);
        public delegate bool Condtion(params object[] args);
        public Dictionary<Enum, Function> function = new Dictionary<Enum, Function>();
        public Dictionary<Enum, Condtion> condition = new Dictionary<Enum, Condtion>();
        public void Register(Enum key, Function e)
        {
            if (function.ContainsKey(key))
            {
                function[key] += e;
            }
            else
            {
                function[key] = e;
            }
        }
        public void Unregister(Enum key, Function e)
        {
            if (function.ContainsKey(key))
            {
                function[key] -= e;
                if (function[key] == null)
                {
                    function.Remove(key);
                }
            }
        }
        public void Fire(Enum key, params object[] args)
        {
            if (function.TryGetValue(key, out var current) &&
                (!condition.TryGetValue(key, out var conditionFunc) || (conditionFunc != null && conditionFunc(args))))
            {
                current(args);
            }
        }
    }
}


