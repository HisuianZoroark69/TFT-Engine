using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFT_Engine.Components;

namespace TFT_Engine
{
    public enum EventType {
        BasicAttack,
        Hitted,
        Move,
        SpecialAttack,
        Healing,
        StatusChanges
    }

    public enum StatusType
    {
        Stun,
        Sleep,
        Burn,
        Blind,
        Channeling
    }
    public class RoundEvent
    {
        public Character main;
        public EventType eventType;
        public StatusType statusType;
        public bool statusValue;
        public bool isCrit = false;
        public int value;
        public DamageType damageType;
        public List<Position> linkedPositions;
        public CharList linkedCharacters;

        public RoundEvent(Character main, EventType et)
        {
            this.main = main;
            eventType = et;
        }
    }
}
