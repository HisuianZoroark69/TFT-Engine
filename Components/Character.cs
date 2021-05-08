using System;
using System.Collections.Generic;

namespace TFT_Engine.Components
{
    public class Character
    {
        public enum DamageType { Physical, Magic};
        //Todo: Time attack and move according to ticks
        public Position position;
        
        public string Name;
        public Guid teamID;
        public Board board;
        public List<Set> set;
        List<Item> items;
        Character AttackTarget;
        Statistics baseStats;
        public Statistics currentStats;

        int attackCounter;
        byte moveCounter;

        int shieldDurationCounter;
        float shieldDuration
        {
            get { return board.defaultTicksPerSec/shieldDurationCounter; }
            set
            {
                shieldDurationCounter = (int)(board.defaultTicksPerSec / value);
            }
        }

        int _mana;
        public int mana { 
            get { return _mana; } 
            set {
                ManaChangeEvent.Invoke(value - _mana);
                _mana = value;                
            } 
        }
        public delegate void HitEventHandler(Character Attacker, DamageType damageType);
        public event HitEventHandler HitEvent;

        public delegate void ManaChangeEventHandler(int ManaChange);
        public event ManaChangeEventHandler ManaChangeEvent;

        int ticksPerAttack { 
            get {
                try { return (int)(board.defaultTicksPerSec / currentStats.attackSpeed); }
                catch (DivideByZeroException) { return 0; }
            }
        }
        bool Dead;

        /// <summary>
        /// Set to sleep and wakeup damage
        /// Stun = Sleep but 0 wakeup damage
        /// </summary>
        bool Sleep;
        int sleepWakeupDamage;
        int sleepDurationCounter;
        float sleepDuration
        {
            get { return board.defaultTicksPerSec / sleepDurationCounter; }
            set
            {
                sleepDurationCounter = (int)(board.defaultTicksPerSec / value);
            }
        }

        bool Moving;
        public Character(string name, Guid teamId, Statistics baseStats,params Set[] s)
        {
            Name = name;
            teamID = teamId;
            this.baseStats = baseStats;
            HitEvent += OnHit;
            ManaChangeEvent += OnManaChange;
            items = new();
            set = new(s);
        }
        /// <summary>
        /// This runs every ticks
        /// </summary>
        public virtual void OverrideOnTick()
        {

        }
        public void OnTick()
        {
            //Check if character is dead or stun or sleep
            if (!Dead && !Sleep)
            {
                //Finding target
                if (AttackTarget == null)
                {
                    int minDistance = int.MaxValue;
                    foreach (Character c in board.Characters)
                    {
                        int distance = board.Distance(c.position, position);
                        if (!c.Dead && c.teamID != teamID && distance < minDistance)
                        {
                            AttackTarget = c;
                            minDistance = distance;
                        }
                    }
                }
                //Move to target if outside of attack range
                if (AttackTarget != null && board.Distance(AttackTarget.position, position) > currentStats.attackRange && !Moving)
                {
                    Move();
                }
                if (Moving)
                {
                    moveCounter++;
                    if (moveCounter >= board.defaultTicksPerSec/4) Moving = false;
                }
                //Check shield duration
                if(--shieldDurationCounter <= 0)
                {
                    currentStats.shield = 0;
                }
                //Attack
                if (++attackCounter >= ticksPerAttack && ticksPerAttack > 0 && !Moving)
                {
                    Attack();
                    attackCounter = 0;
                }
                OverrideOnTick();
            }
            if (Sleep)
            {
                if(--sleepDurationCounter <= 0)
                {
                    Sleep = false;
                }
            }
        }
        public virtual void OnStart()
        {
            Dead = false;
            Moving = false;
            Sleep = false;
            attackCounter = 0;
            currentStats = baseStats;
        }

        public virtual void Move()
        {
            moveCounter = 0;
            Moving = true;
            position = board.PathFinding(position, AttackTarget.position)[1];
        }
        public virtual void Attack()
        {
            AttackTarget.HitEvent.Invoke(this, DamageType.Physical);
            foreach (Item i in items) i.OnAttack(AttackTarget);
            mana += 10;
        }
        public virtual int DamageCalculation(Character Attacker)
        {
            int damage = Attacker.currentStats.atk * 5 / (5 + currentStats.def);
            foreach (Item i in items) damage = i.OnDamageCalculation(damage);
            return damage;
        }
        public virtual void OnHit(Character attacker, DamageType damageType)
        {
            int damage = DamageCalculation(attacker);
            if (currentStats.shield > damage) currentStats.shield -= damage;
            else { currentStats.hp -= damage - currentStats.shield; currentStats.shield = 0; };

            if (currentStats.hp <= 0)
            {
                attacker.AttackTarget = null;
                Dead = true;
                board.charCounter[teamID]--;
                return;
            }
            mana += attacker.currentStats.atk / 100 + damage * 7 / 100;
        }
        public virtual void OnManaChange(int manaChange)
        {
            if(mana >= currentStats.maxMana)
            {
                _mana = 0;
                SpecialMove();
            }
        }
        public virtual void SpecialMove() {
            foreach (Item i in items) i.OnSpecialMove();
        }
        public virtual void AddItem(Item item)
        {
            board.TickEvent += item.OnTick;
            board.StartEvent += item.OnStart;
            board.EndEvent += item.OnEnd;
            HitEvent += item.OnHit;
            ManaChangeEvent += item.OnManaChange;

            item.Holder = this;
            items.Add(item);
        }
        public virtual void RemoveItem(Item item)
        {
            board.TickEvent -= item.OnTick;
            board.StartEvent -= item.OnStart;
            board.EndEvent -= item.OnEnd;
            HitEvent -= item.OnHit;
            ManaChangeEvent -= item.OnManaChange;

            item.Holder = null;
            items.Remove(item);
        }
        public virtual void AddShield(int amount, float duration)
        {
            currentStats.shield += amount;
            shieldDuration = duration;
        }
    }
}
