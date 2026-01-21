using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Game
{
    public class EventTriggerUtils
    {
        public static EventTrigger GetTrigger(GameObject go)
        {
            EventTrigger trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
            if (trigger.triggers == null)
            {
                trigger.triggers = new List<EventTrigger.Entry>();
            }

            return trigger;
        }

        public static EventTrigger.Entry AddEventEntry(GameObject obj, EventTriggerType type,
            Action<GameObject, string, BaseEventData> callAction, string tag)
        {
            EventTrigger.TriggerEvent e = new EventTrigger.TriggerEvent();
            e.AddListener(f =>
            {
                callAction(obj, tag, f);
            });
            EventTrigger trigger = GetTrigger(obj);
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback = e;
            trigger.triggers.Add(entry);

            return entry;
        }

        /// <summary>
        /// ɾ������
        /// </summary>
        /// <param name="btn"></param>
        public static void RemoveListener(GameObject obj)
        {
            EventTrigger trigger = GetTrigger(obj);
            trigger.triggers.Clear();
        }

    }
}