using System.Collections.Generic;
using Game.Data;
using UnityEngine;

namespace Game.Net.Protocol
{
    public class Option : Base
    {
        public class Item
        {
            public enum Type
            {
                Text,
                Button,
                TitleButton,
                TitleButtonWithProgress,
                IAPButton,
                Radar,
                ProgressWithValue,
                Progress,
                Slider,
                Input,
                Filter,
                ToggleGroup,
                Confirm,
                Amount,
            }
            public Type type;
            public Dictionary<string, string> data = new Dictionary<string, string>();
        }
        public List<Item> lefts;
        public List<Item> rights;
        public override void Processed()
        {
            var currentOption = DataManager.Instance.Option;
            var newOption = this.ToLogic();
            DataManager.Instance.Option = newOption;
        }

        private bool AreEqual(Option other)
        {
            if (other == null) return false;
            if ((lefts == null) != (other.lefts == null)) return false;
            if ((rights == null) != (other.rights == null)) return false;

            if (lefts != null)
            {
                if (lefts.Count != other.lefts.Count) return false;
                for (int i = 0; i < lefts.Count; i++)
                {
                    if (!ItemsEqual(lefts[i], other.lefts[i])) return false;
                }
            }

            if (rights != null)
            {
                if (rights.Count != other.rights.Count) return false;
                for (int i = 0; i < rights.Count; i++)
                {
                    if (!ItemsEqual(rights[i], other.rights[i])) return false;
                }
            }

            return true;
        }

        private bool ItemsEqual(Item a, Item b)
        {
            if (a.type != b.type) return false;
            if (a.data.Count != b.data.Count) return false;

            foreach (var kvp in a.data)
            {
                if (!b.data.TryGetValue(kvp.Key, out string value) || value != kvp.Value)
                    return false;
            }

            return true;
        }
        public bool Empty => lefts == null && rights == null;
    }

    public class OptionReturn : Base
    {
    }

    public class OptionButton : Base
    {
        public int index;
        public int side;

        public OptionButton(int index, int side)
        {
            this.index = index;
            this.side = side;
        }
    }

    public class OptionInput : Base
    {
        public string text;
        public OptionInput(string text)
        {
            this.text = text;
        }
    }

    public class OptionConfirm : Base
    {
        public int index;
        public int side;
        public OptionConfirm(int index, int side)
        {
            this.index = index;
            this.side = side;
        }
    }

    public class JsonPurchase : Base
    {
        public string receipt;
    }

    public class OptionSlider : Base
    {
        public int id;
        public int value;
        public OptionSlider(int id, int value)
        {
            this.id = id;
            this.value = value;
        }
    }

    public class OptionAmount : Base
    {
        public int amount;
        public OptionAmount(int amount)
        {
            this.amount = amount;
        }
    }

    public class OptionFilter : Base
    {
        public int id;
        public string text;
        public OptionFilter(int id, string text)
        {
            this.id = id;
            this.text = text;
        }
    }

    public class OptionToggle : Base
    {
        public string id;
        public bool value;
        public OptionToggle(string id, bool value)
        {
            this.id = id;
            this.value = value;
        }
    }

    public class AlipayOrder : Base
    {
        public string body;
        public override void Processed()
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject alipayClass = new AndroidJavaObject("com.alipay.sdk.app.PayTask", currentActivity))
            {
                string result = alipayClass.Call<string>("pay", body, true);
            }
        }
    }
}
