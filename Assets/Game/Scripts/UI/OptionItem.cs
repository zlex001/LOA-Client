using Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class OptionItem : UI.Core
    {
        public int index;
        protected int side;

        public virtual void SetSide(int side)
        {
            this.side = side;
        }

    }
}