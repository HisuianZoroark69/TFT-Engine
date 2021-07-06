using System.Collections.Generic;
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
        Shield,
        ManaChange,
        Effects,
        PositionEffects,
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
        public DamageType DamageType;
        public EventType EventType;
        public bool IsCrit = false;
        public CharList LinkedCharacters;
        public Position LinkedPositions;
        public Set LinkedSet;
        public Character Main;
        public string EffectName;
        public bool EffectValue;
        public double Value;

        public RoundEvent(Character main, EventType et)
        {
            Main = main;
            EventType = et;
        }

        public RoundEvent(Character main, Position pos)
        {
            this.Main = main;
            LinkedPositions = pos;
            EventType = EventType.PositionEffects;
        }
        public RoundEvent(Set main, Position pos)
        {
            LinkedSet = main;
            LinkedPositions = pos;
            EventType = EventType.PositionEffects;
        }

        public RoundEvent()
        {
        }
    }
}