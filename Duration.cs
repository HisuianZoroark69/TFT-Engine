using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFT_Engine.Components;

namespace TFT_Engine
{
    public class Duration
    {
        public delegate void ActionDelegate(Dictionary<string, object> values);
        public ActionDelegate OnStart;
        public ActionDelegate OnHit;
        public ActionDelegate OnTick;
        public ActionDelegate OnEnd;

        public Dictionary<string, object> values;
        public string name;

        private Character character;

        public int durationCounter;
        float duration
        {
            get => durationCounter / character.board.defaultTicksPerSec;
            set => durationCounter = (int)(character.board.defaultTicksPerSec * value);
        }

        public Duration(string name, float duration, Character c)
        {
            values = new();
            this.duration = duration;
            character = c;
        }
        public void _OnTick()
        {
            durationCounter--;
            OnTick(values);
            if (durationCounter == 0)
            {
                OnEnd(values);
            }
        }

        public void _OnHit()
        {
            OnHit(values);
        }
    }
}
