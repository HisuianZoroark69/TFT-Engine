using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TFT_Engine.Components;

namespace TFT_Engine
{
    public abstract class Effect
    {
        public int DurationCounter { get; set; }
        public double BaseDuration;
        public int maxStack; //maxStack = 0 <=> infinite stacks
        public Board board;
        public Character Effector;
        public Character Effected;
        public Set EffectorSet;
        public bool IsLoop;
        public Position EffectedPosition;

        protected Effect(double duration, Character effector, Character effected, int stack = 0)
        {
            maxStack = stack;
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
        protected Effect(double duration, Set effector, Character effected, int stack = 0)
        {
            maxStack = stack;
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
        protected Effect(double duration, Character effected, int stack = 0)
        {
            maxStack = stack;
            BaseDuration = duration;
            DurationCounter = (int)(duration * effected.board.defaultTicksPerSec);
            Effected = effected;
            effected.board.AddRoundEvent(new RoundEvent(effected, EventType.Effects)
            {
                EffectName = GetType().Name,
                EffectValue = true
            });
        }

        //Board to get default tick
        protected Effect(double duration, Character effector, Position pos, Board board, int stack = 0)
        {
            maxStack = stack;
            Effector = effector;
            BaseDuration = duration;
            DurationCounter = (int)(duration * board.defaultTicksPerSec);
            EffectedPosition = pos;
            board.AddRoundEvent(new RoundEvent(effector, pos)
            {
                linkedPositions = pos,
                EffectName = GetType().Name,
                EffectValue = true
            });
        }
        protected Effect(double duration, Set effector, Position pos, Board board, int stack = 0)
        {
            maxStack = stack;
            EffectorSet = effector;
            BaseDuration = duration;
            DurationCounter = (int)(duration * board.defaultTicksPerSec);
            EffectedPosition = pos;
            board.AddRoundEvent(new RoundEvent(effector, pos)
            {
                linkedPositions = pos,
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
            DurationCounter = (int)(BaseDuration * board.defaultTicksPerSec);
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
            if(Effected != null)
                board.AddRoundEvent(new RoundEvent(Effected, EventType.Effects)
                {
                    EffectName = GetType().Name,
                    linkedCharacters = new() { Effector },
                    EffectValue = false
                });
            else if(EffectorSet != null)
                board.AddRoundEvent(new RoundEvent(Effected, EventType.Effects)
                {
                    EffectName = GetType().Name,
                    linkedSet = EffectorSet,
                    EffectValue = false
                });
            else
            {
                board.AddRoundEvent(new RoundEvent(Effector ,EffectedPosition)
                {
                    EffectName = GetType().Name,
                    EffectValue = false
                });
            }
            
        }
    }
}
