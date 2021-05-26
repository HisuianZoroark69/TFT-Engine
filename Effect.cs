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
        public double BaseDuration;
        public Board board;
        public Character Effector;
        public Character Effected;
        public Set EffectorSet;
        public bool IsLoop;
        public Position EffectedPosition;

        protected Effect(double duration, Character effector, Character effected)
        {
            BaseDuration = duration;
            DurationCounter = (int)(duration * effected.board.defaultTicksPerSec);
            Effector = effector;
            Effected = effected;
            effected.board.AddRoundEvent(new RoundEvent(effected,EventType.Effects)
            {
                EffectName = GetType().Name,
                linkedCharacters = new(){effector},
                EffectValue = true
            });
        }
        protected Effect(double duration, Set effector, Character effected)
        {
            BaseDuration = duration;
            DurationCounter = (int)(duration * effected.board.defaultTicksPerSec);
            EffectorSet = effector;
            Effected = effected;
            board.AddRoundEvent(new RoundEvent(effected, EventType.Effects)
            {
                EffectName = GetType().Name,
                linkedSet = effector,
                EffectValue = true
            });
        }
        protected Effect(double duration, Character effected)
        {
            BaseDuration = duration;
            DurationCounter = (int)(duration * effected.board.defaultTicksPerSec);
            Effected = effected;
            board.AddRoundEvent(new RoundEvent(effected, EventType.Effects)
            {
                EffectName = GetType().Name,
                EffectValue = true
            });
        }

        //Board to get default tick
        protected Effect(double duration, Position pos, Board board)
        {
            BaseDuration = duration;
            DurationCounter = (int)(duration * board.defaultTicksPerSec);
            EffectedPosition = pos;
            board.AddRoundEvent(new RoundEvent(pos)
            {
                EffectName = GetType().Name,
                EffectValue = true
            });
        }
        public Effect SetLoop(bool loop)
        {
            IsLoop = loop;
            return this;
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
            //Loop
            if (IsLoop)
            {
                DurationCounter = (int)(board.defaultTicksPerSec * BaseDuration);
                return;
            }
            if(Effector != null)
                board.AddRoundEvent(new RoundEvent(Effected, EventType.Effects)
                {
                    EffectName = GetType().Name,
                    linkedCharacters = new() { Effector },
                    EffectValue = false
                });
            else
                board.AddRoundEvent(new RoundEvent(Effected, EventType.Effects)
                {
                    EffectName = GetType().Name,
                    linkedSet = EffectorSet,
                    EffectValue = false
                });
        }
    }
}
