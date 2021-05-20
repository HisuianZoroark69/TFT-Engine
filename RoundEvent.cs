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
        StatusChanges,
        ManaChange,
        Dead
    }

    public enum StatusType
    {
        Null,
        Stun,
        Sleep,
        Burn,
        Blind,
        Channeling
    }

    public class RoundEvent
    {
        public DamageType damageType;
        public EventType eventType;
        public bool isCrit = false;
        public CharList linkedCharacters;
        public List<Position> linkedPositions;
        public Set linkedSet;
        public Character main;
        public StatusType statusType;
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