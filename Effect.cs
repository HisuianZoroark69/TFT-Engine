using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFT_Engine.Components;

namespace TFT_Engine
{
    public abstract class Effect
    {
        public int DurationCounter { get; set; }
        public Board board;
        public Character Effector;
        public Character Effected;
        public Set EffectorSet;

        protected Effect(int Duration, Character effector, Character effected = null)
        {
            DurationCounter = Duration;
            Effector = effector;
            Effected = effected;
        }
        protected Effect(int Duration, Set effector, Character effected = null)
        {
            DurationCounter = Duration;
            EffectorSet = effector;
            Effected = effected;
        }

        public void OnTick()
        {
            DurationCounter--;
            OverrideOnTick();
            if (DurationCounter == 0)
            {
                OnElapsed();
            }
        }

        public virtual void OverrideOnTick() { }
        public virtual void OnElapsed(){}
    }
}
