using System;
using System.Linq;
using TFT_Engine.Components;

namespace TFT_Engine
{
    public abstract class Effect
    {
        public int DurationCounter { get; set; }
        public double BaseDuration;
        public int MaxStack; //MaxStack = 0 <=> infinite stacks
        public Board Board;
        public Character Effector;
        public Character Effected;
        public Set EffectorSet;
        public bool IsLoop = false;
        public Position EffectedPosition;

        protected Effect(double duration, Character effector, Character effected, int stack = 0)
        {
            MaxStack = stack;
            BaseDuration = duration;
            DurationCounter = (int)(duration * effected.Board.DefaultTicksPerSec);
            Effector = effector;
            Effected = effected;
            effected.Board.AddRoundEvent(new RoundEvent(effected, EventType.Effects)
            {
                EffectName = GetType().Name,
                LinkedCharacters = new() { effector },
                EffectValue = true
            });
        }
        protected Effect(double duration, Set effector, Character effected, int stack = 0)
        {
            MaxStack = stack;
            BaseDuration = duration;
            DurationCounter = (int)(duration * effected.Board.DefaultTicksPerSec);
            EffectorSet = effector;
            Effected = effected;
            Board.AddRoundEvent(new RoundEvent(effected, EventType.Effects)
            {
                EffectName = GetType().Name,
                LinkedSet = effector,
                EffectValue = true
            });
        }
        protected Effect(double duration, Character effected, int stack = 0)
        {
            MaxStack = stack;
            BaseDuration = duration;
            DurationCounter = (int)(duration * effected.Board.DefaultTicksPerSec);
            Effected = effected;
            effected.Board.AddRoundEvent(new RoundEvent(effected, EventType.Effects)
            {
                EffectName = GetType().Name,
                EffectValue = true
            });
        }

        //Board to get default tick
        protected Effect(double duration, Character effector, Position pos, Board Board, int stack = 0)
        {
            MaxStack = stack;
            Effector = effector;
            BaseDuration = duration;
            DurationCounter = (int)(duration * Board.DefaultTicksPerSec);
            EffectedPosition = pos;
            Board.AddRoundEvent(new RoundEvent(effector, pos)
            {
                LinkedPositions = pos,
                EffectName = GetType().Name,
                EffectValue = true
            });
        }
        protected Effect(double duration, Set effector, Position pos, Board Board, int stack = 0)
        {
            MaxStack = stack;
            EffectorSet = effector;
            BaseDuration = duration;
            DurationCounter = (int)(duration * Board.DefaultTicksPerSec);
            EffectedPosition = pos;
            Board.AddRoundEvent(new RoundEvent(effector, pos)
            {
                LinkedPositions = pos,
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

        public void Refresh()
        {
            DurationCounter = (int)(BaseDuration * Board.DefaultTicksPerSec);
        }
        public virtual void OverrideOnTick() { }

        public virtual void OnElapsed()
        {
            //Loop
            if (IsLoop)
            {
                DurationCounter = (int)(Board.DefaultTicksPerSec * BaseDuration);
                return;
            }
            if (Effected != null)
                Board.AddRoundEvent(new RoundEvent(Effected, EventType.Effects)
                {
                    EffectName = GetType().Name,
                    LinkedCharacters = new() { Effector },
                    EffectValue = false
                });
            else if (EffectorSet != null)
                Board.AddRoundEvent(new RoundEvent(Effected, EventType.Effects)
                {
                    EffectName = GetType().Name,
                    LinkedSet = EffectorSet,
                    EffectValue = false
                });
            else
            {
                Board.AddRoundEvent(new RoundEvent(Effector, EffectedPosition)
                {
                    EffectName = GetType().Name,
                    EffectValue = false
                });
            }

        }
    }
}
