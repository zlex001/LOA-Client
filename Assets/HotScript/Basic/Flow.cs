using System;
using System.Collections.Generic;

namespace Game.Basic
{
    public class Flow<T> where T : Enum
    {
        public T Current { get; private set; }

        private Dictionary<T, Action> stepHandlers = new();
        public Action<T>? OnStepChanged;

        public void Register(T step, Action handler) => stepHandlers[step] = handler;

        public void Start(T first)
        {
            Current = first;
            OnStepChanged?.Invoke(Current);
            stepHandlers[Current]?.Invoke();
        }

        public void Next(T next)
        {
            Current = next;
            OnStepChanged?.Invoke(Current);
            stepHandlers[Current]?.Invoke();
        }

        public void Goto(T step) => Next(step);
    }
}
