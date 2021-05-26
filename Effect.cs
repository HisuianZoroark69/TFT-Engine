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

        protected Effect(double duration, Character effector, Character effected)
        {
            DurationCounter = (int)(duration * effected.board.defaultTicksPerSec);
            Effector = effector;
            Effected = effected;
            effected.board.AddRoundEvent(new RoundEvent(effected,EventType.Effects)
            {
                statusTypeName = GetType().Name,
                linkedCharacters = new(){effector},
                statusValue = true
            });
        }
        protected Effect(double duration, Set effector, Character effected)
        {
            DurationCounter = (int)(duration * effected.board.defaultTicksPerSec);
            EffectorSet = effector;
            Effected = effected;
            board.AddRoundEvent(new RoundEvent(effected, EventType.Effects)
            {
                statusTypeName = GetType().Name,
                linkedSet = effector,
                statusValue = true
            });
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

        public void Abort()
        {
            DurationCounter = 0;
        }
        public virtual void OverrideOnTick() { }

        public virtual void OnElapsed()
        {
            if(Effector != null)
                board.AddRoundEvent(new RoundEvent(Effected, EventType.Effects)
                {
                    statusTypeName = GetType().Name,
                    linkedCharacters = new() { Effector },
                    statusValue = false
                });
            else
                board.AddRoundEvent(new RoundEvent(Effected, EventType.Effects)
                {
                    statusTypeName = GetType().Name,
                    linkedSet = EffectorSet,
                    statusValue = false
                });
        }
    }
}
