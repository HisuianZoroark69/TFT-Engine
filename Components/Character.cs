using System;
using System.Collections.Generic;

namespace TFT_Engine.Components
{
    public class Character
    {
        public enum DamageType { Physical, Magic, True};
        public Random rand = new();
        public Position position;
        public Position basePosition;
        
        public string Name;
        public Guid teamID;
        public Board board;
        public List<Set> set;
        public List<Item> items;
        public Character AttackTarget;
        public HashSet<Character> SpecialAttackAffected;
        Statistics baseStats;
        public Statistics currentStats;
        public float defaultBonusCritDamage = 0.5f;
        public bool canBeTargeted;
        public bool collision;
        public bool ImmuneCC;

        int attackCounter;
        byte moveCounter;

        protected int shieldDurationCounter;
        protected float shieldDuration
        {
            get { return shieldDurationCounter/board.defaultTicksPerSec; }
            set
            {
                shieldDurationCounter = (int)(board.defaultTicksPerSec * value);
            }
        }

        protected int _mana;
        public int mana { 
            get { return _mana; } 
            set {
                _mana = value;
                ManaChangeEvent.Invoke(value - _mana);             
            } 
        }
        public delegate void HitEventHandler(Character Attacker, DamageType damageType, bool isSpecial = false, bool isCrit = false, int amount = 0);
        public event HitEventHandler HitEvent;

        public delegate void ManaChangeEventHandler(int ManaChange);
        public event ManaChangeEventHandler ManaChangeEvent;

        int TicksPerAttack { 
            get {
                try { return (int)(board.defaultTicksPerSec / currentStats.attackSpeed); }
                catch (DivideByZeroException) { return 0; }
            }
        }
        int movingSpeed
        {
            get
            {
                return board.defaultTicksPerSec * (int)currentStats.movingSpeed;
            }
        }
        protected bool Dead;
        

        /// <summary>
        /// Set to sleep and wakeup damage
        /// </summary>
        bool Sleep;
        public bool IsSleep { get { return Sleep; } }
        Character SleepSetter;
        int sleepWakeupDamage;
        int sleepDurationCounter;
        float SleepDuration
        {
            get { return sleepDurationCounter / board.defaultTicksPerSec; }
            set
            {
                sleepDurationCounter = (int)(board.defaultTicksPerSec * value);
            }
        }

        bool Stun;
        public bool IsStun { get { return Stun; } }
        Character Stunner;
        int stunDurationCounter;
        protected float StunDuration
        {
            get { return stunDurationCounter / board.defaultTicksPerSec; }
            set
            {
                stunDurationCounter = (int)(board.defaultTicksPerSec * value);
            }
        }

        int burnDamageEachSec;
        int burnDurationCounter;
        Character BurnSetter;
        float burnDuration
        {
            get { return burnDurationCounter / board.defaultTicksPerSec; }
            set
            {
                burnDurationCounter = (int)(board.defaultTicksPerSec * value);
            }
        }
        public float IncreasedIntakeDamage;
        public float IncreasedIntakeDamagePercentage;
        DamageType burnDamageType;

        int DecreasedHealingDurationCounter;
        float DecreasedHealingDuration
        {
            get { return DecreasedHealingDurationCounter / board.defaultTicksPerSec; }
            set
            {
                DecreasedHealingDurationCounter = (int)(board.defaultTicksPerSec * value);
            }
        }

        bool Blind;
        bool IsBlind { get { return Blind; } }
        Character BlindSetter;
        int BlindDurationCounter;
        float BlindDuration
        {
            get { return BlindDurationCounter / board.defaultTicksPerSec; }
            set
            {
                BlindDurationCounter = (int)(board.defaultTicksPerSec * value);
            }
        }

        bool Channeling;
        int ChannelingDurationCounter;
        float ChannelingDuration
        {
            get { return ChannelingDurationCounter / board.defaultTicksPerSec; }
            set
            {
                ChannelingDurationCounter = (int)(board.defaultTicksPerSec * value);
            }
        }

        int GraduallyHealAmountPerTick;
        int GraduallyHealDurationCounter;
        float GraduallyHealDuration
        {
            get { return GraduallyHealDurationCounter / board.defaultTicksPerSec; }
            set
            {
                GraduallyHealDurationCounter = (int)(board.defaultTicksPerSec * value);
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
        public virtual void OverrideOnTick() { }
        public void OnTick()
        {
            //Check if character is dead or stun or sleep
            if (!Dead && !Sleep && !Stun && !Channeling)
            {
                //Checking if existing target can be targeted
                if(AttackTarget != null && !AttackTarget.canBeTargeted)
                {
                    AttackTarget = null;
                }
                //Finding target
                if (AttackTarget == null)
                {
                    int minDistance = int.MaxValue;
                    foreach (Character c in board.Characters)
                    {
                        int distance = board.Distance(c.position, position);
                        if (c.canBeTargeted && !c.Dead && c.teamID != teamID && distance < minDistance)
                        {
                            AttackTarget = c;
                            minDistance = distance;
                        }
                    }
                    OnFindNewTarget();
                }
                //Move to target if outside of attack range
                if (AttackTarget != null && board.Distance(AttackTarget.position, position) > currentStats.attackRange && !Moving)
                {
                    Move();
                }
                if (Moving)
                {
                    moveCounter++;
                    if (moveCounter >= movingSpeed) Moving = false;
                }
                //Check shield duration
                if(--shieldDurationCounter <= 0)
                {
                    currentStats.shield = 0;
                }
                //Attack
                if (++attackCounter >= TicksPerAttack && TicksPerAttack > 0 && !Moving)
                {
                    Attack();
                    attackCounter = 0;
                }                
            }
            if (Sleep)
            {
                if(--sleepDurationCounter <= 0)
                {
                    Sleep = false;
                }
            }
            if(burnDurationCounter <= 0) IncreasedIntakeDamage = 0;
            else
            {
                burnDurationCounter--;
                OnHit(BurnSetter, burnDamageType, false, false, burnDamageEachSec);
            }
            if (DecreasedHealingDurationCounter <= 0) currentStats.decreasedHealing = 0;
            else --DecreasedHealingDurationCounter;

            if (stunDurationCounter <= 0) Stun = false;
            else stunDurationCounter--;

            if (ChannelingDurationCounter <= 0) Channeling = false;
            else ChannelingDurationCounter--;

            if (BlindDurationCounter <= 0) Blind = false;
            else BlindDurationCounter--;
            if (GraduallyHealDurationCounter > 0)
            {
                GraduallyHealDurationCounter--;
                currentStats.hp += GraduallyHealAmountPerTick;
            }

            OverrideOnTick();
        }
        public virtual void OnFindNewTarget() { }
        public virtual void OnStart()
        {
            position = basePosition;
            SpecialAttackAffected = new();
            Dead = false;
            Moving = false;
            Sleep = false;
            Blind = false;
            Stun = false;
            collision = true;
            canBeTargeted = true;
            ImmuneCC = false;
            attackCounter = 0;
            IncreasedIntakeDamage = 0;
            IncreasedIntakeDamagePercentage = 1;
            currentStats = baseStats;
            currentStats.hp = currentStats.maxHp;
        }
        public virtual void Move()
        {
            moveCounter = 0;
            Moving = true;
            position = board.PathFinding(position, AttackTarget.position)[1];
        }
        public virtual void Attack()
        {
            if (!Blind)
                AttackTarget?.HitEvent.Invoke(this, DamageType.Physical, false, rand.Next(1, 100) <= currentStats.critRate);
            foreach (Item i in items) i.OnAttack(AttackTarget);
            mana += 10;
        }
        public virtual int DamageCalculation(Character Attacker)
        {
            float damage = (Attacker.currentStats.atk + IncreasedIntakeDamage) * IncreasedIntakeDamagePercentage * 100 / (100 + currentStats.def); 
            foreach (Item i in items) damage = i.OnDamageCalculation((int)damage);
            return (int)damage;
        }
        public virtual int MagicDamageCalculation(Character Attacker, int amount)
        {
            float damage = (amount + IncreasedIntakeDamage) * IncreasedIntakeDamagePercentage * Attacker.currentStats.specialAtkPercentage * 100 / (100 + currentStats.specialDef);       
            foreach (Item i in items) damage = i.OnMagicDamageCalculation((int)damage);
            return (int)damage;
        }
        public virtual void OnHit(Character attacker, DamageType damageType, bool isSpecial = false, bool isCrit = false, int amount = 0)
        {
            int damage = 0;
            if (damageType == DamageType.Physical)
            {
                if(!(rand.Next(1,100) <= currentStats.dodgeRate))damage = amount != 0 ? amount : DamageCalculation(attacker);
            }
            else if (damageType == DamageType.Magic)
            {
                damage = MagicDamageCalculation(attacker, amount);
                //if(isSpecial) attacker.SpecialAttackAffected.Add(this);
            }
            else if(damageType == DamageType.True)
            {
                damage = amount != 0 ? amount : attacker.currentStats.atk;
            }
            if (isCrit) damage = (int)(damage * (1 + defaultBonusCritDamage));

            if (Sleep)
            {
                Sleep = false;
                OnHit(SleepSetter, DamageType.Magic, false, false, sleepWakeupDamage);
            }
            if (currentStats.shield > damage) currentStats.shield -= damage;
            else { currentStats.hp -= damage - currentStats.shield; currentStats.shield = 0; };

            if (currentStats.hp <= 0)
            {
                attacker.AttackTarget = null;
                Dead = true;
                OnKilled(attacker);
                board.charCounter[teamID]--;
                return;
            }
            if (damageType == DamageType.Physical) mana += attacker.currentStats.atk / 100 + damage * 7 / 100;
            else if (damageType == DamageType.Magic) mana += amount / 100 + damage * 7 / 100;
        }
        public virtual void OnManaChange(int manaChange)
        {
            if(mana >= currentStats.maxMana)
            {
                mana = 0;
                currentStats.maxMana = baseStats.maxMana; //Reset max mana if increased
                SpecialMove();
            }
        }
        public virtual void SpecialMove() {
            SpecialAttackAffected.Clear();
            foreach (Item i in items) i.OnSpecialMove();
            //foreach (Set s in set) s.OnSpecialMove();
        }
        public virtual void OnKilled(Character Killer) { }
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
        public virtual void GraduallyHeal(int amount, float duration)
        {
            GraduallyHealDuration = duration;
            GraduallyHealAmountPerTick = amount / GraduallyHealDurationCounter;
        }
        public virtual void SetSleep(Character setter, float duration, int wakeupDamage)
        {
            if (!ImmuneCC)
            {
                SleepSetter = setter;
                SleepDuration = duration;
                Sleep = true;
                sleepWakeupDamage = wakeupDamage;
            }
        }
        public virtual void SetBurn(Character setter, float duration, int burnDamage, DamageType burnType, float increasedIntakeDamage = 1)
        {
            BurnSetter = setter;
            burnDuration = duration;
            burnDamageEachSec = burnDamage / burnDurationCounter;
            burnDamageType = burnType;
            IncreasedIntakeDamagePercentage *= increasedIntakeDamage;
        }
        public virtual void SetStun(Character setter, float duration)
        {
            if (!ImmuneCC)
            {
                Stun = true;
                Stunner = setter;
                StunDuration = duration;
            }
        }
        public virtual void SetBlind(Character setter, float duration)
        {
            BlindSetter = setter;
            BlindDuration = duration;
        }
        public virtual void SetChanneling(float duration)
        {
            Channeling = true;
            ChannelingDuration = duration;
        }
    }
}
