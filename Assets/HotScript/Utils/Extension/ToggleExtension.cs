using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game.Utils
{
    public static class ToggleExtension
    {
        static EventTrigger GetTrigger(GameObject go)
        {
            EventTrigger trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
            if (trigger.triggers == null)
            {
                trigger.triggers = new List<EventTrigger.Entry>();
            }

            return trigger;
        }

        static EventTrigger.Entry AddEventEntry(Toggle btn, EventTriggerType type,
        Action<Toggle> callFunc)
        {
            EventTrigger.TriggerEvent e = new EventTrigger.TriggerEvent();
            e.AddListener(f =>
            {
                callFunc(btn);
            });
            EventTrigger trigger = GetTrigger(btn.gameObject);
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback = e;
            trigger.triggers.Add(entry);

            return entry;
        }
        public static EventTrigger.Entry AddOnPointerClick(this Toggle btn, Action<Toggle> callFunc)
        {
            return AddEventEntry(btn, EventTriggerType.PointerClick, callFunc);
        }
    }
}