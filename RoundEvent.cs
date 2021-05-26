using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TFT_Engine;
using TFT_Engine.Components;

namespace TFT_Engine
{
    public enum EventType
    {
        Null,
        BasicAttack,
        Hitted,
        Move,
        SpecialAttack,
        Healing,
        ManaChange,
        Effects,
        Dead
    }

    public class RoundEventDictionary : Dictionary<int, List<RoundEvent>>
    {
        public new List<RoundEvent> this[int t]
        {
            get
            {
                if (ContainsKey(t))
                {
                    return base[t];
                }
                return new();
            }
            set => base[t] = value;
            
        }
    }

    public class RoundEvent
    {
        public DamageType damageType;
        public EventType eventType;
        public bool isCrit = false;
        public CharList linkedCharacters;
        public Position linkedPositions;
        public Set linkedSet;
        public Character main;
        public string statusTypeName;
        public bool statusValue;
        public double value;

        public RoundEvent(Character main, EventType et)
        {
            this.main = main;
            eventType = et;
        }

        public RoundEvent()
        {
        }
    }
}