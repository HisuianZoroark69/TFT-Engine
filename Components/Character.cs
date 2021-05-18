using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace TFT_Engine.Components
{
    public enum DamageType { Physical, Magic, True};
    public class Character
    {
        
        public Random rand = new();
        public Position position;
        public Position basePosition;
        
        public string Name;
        public Guid teamID;
        public Board board;
        public List<Set> set;
        public List<Item> items;
        Character _attackTarget;
        public Character AttackTarget
        {
            get => _attackTarget;
            set
            {
                //Finding target
                if (value == null)
                {
                    int minDistance = int.MaxValue;
                    foreach (Character c in board.Characters)
                    {
                        int distance = board.Distance(c.position, position);
                        if (c.canBeTargeted && !c.Dead && c.teamID != teamID && distance < minDistance)
                        {
                            _attackTarget = c;
                            minDistance = distance;
                        }
                    }
                    OnFindNewTarget();
                }
            }
        }
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
            get => shieldDurationCounter/board.defaultTicksPerSec;
            set => shieldDurationCounter = (int)(board.defaultTicksPerSec * value);
        }

        protected int _mana;
        public int mana { 
            get => _mana;
            set {
                if (!Channeling)
                {
                    _mana = value;
                    ManaChangeEvent.Invoke(value - _mana);
                }
            } 
        }
        public delegate void HitEventHandler(Character Attacker, DamageType damageType, bool isSpecial = false, bool isCrit = false, float amount = 0);
        public event HitEventHandler HitEvent;

        public delegate void ManaChangeEventHandler(int ManaChange);
        public event ManaChangeEventHandler ManaChangeEvent;

        int TicksPerAttack { 
            get {
                try { return (int)(board.defaultTicksPerSec / currentStats.attackSpeed); }
                catch (DivideByZeroException) { return 0; }
            }
        }
        int movingSpeed => (int)(board.defaultTicksPerSec * currentStats.movingSpeed);
        protected bool Dead;


        /// <summary>
        /// Set to sleep and wakeup damage
        /// </summary>
        private bool _Sleep;
        public bool Sleep
        {
            get => _Sleep;
            protected set
            {
                if (value != _Sleep)
                {
                    board.AddRoundEvent(new RoundEvent(this, EventType.StatusChanges)
                    {
                        statusType = StatusType.Sleep,
                        statusValue = value
                    });
                }
            }
        }
        Character SleepSetter;
        int sleepWakeupDamage;
        int sleepDurationCounter;
        float SleepDuration
        {
            //get => sleepDurationCounter / board.defaultTicksPerSec;
            set => sleepDurationCounter = (int)(board.defaultTicksPerSec * value);
        }

        //Stun
        private bool _Stun;
        public bool Stun
        {
            get => _Stun;
            protected set
            {
                if (value != _Stun)
                    board.AddRoundEvent(new RoundEvent(this, EventType.StatusChanges)
                    {
                        statusType = StatusType.Stun,
                        statusValue = value
                    });
            }
        }
        Character Stunner;
        int stunDurationCounter;
        protected float StunDuration
        {
            get => stunDurationCounter / board.defaultTicksPerSec;
            set => stunDurationCounter = (int)(board.defaultTicksPerSec * value);
        }

        //Burn
        private bool _Burn;
        public bool Burn
        {
            get => _Burn;
            protected set
            {
                if (value != _Burn)
                    board.AddRoundEvent(new RoundEvent(this, EventType.StatusChanges)
                    {
                        statusType = StatusType.Burn,
                        statusValue = value
                    });
            }
        }
        protected int burnDamageEachSec;
        protected int burnDurationCounter;
        protected Character BurnSetter;
        float burnDuration
        {
            set => burnDurationCounter = (int)(board.defaultTicksPerSec * value);
        }
        public float BonusIntakeDamage;
        public float BonusIntakeDamagePercentage;
        DamageType burnDamageType;
        int DecreasedHealingDurationCounter;

        //Blind
        private bool _Blind;
        public bool Blind
        {
            get => _Blind;
            protected set
            {
                if (value != _Blind)
                    board.AddRoundEvent(new RoundEvent(this, EventType.StatusChanges)
                    {
                        statusType = StatusType.Blind,
                        statusValue = value
                    });
            }
        }
        Character BlindSetter;
        int BlindDurationCounter;
        float BlindDuration
        {
            get => BlindDurationCounter / board.defaultTicksPerSec;
            set => BlindDurationCounter = (int)(board.defaultTicksPerSec * value);
        }

        private bool _Channeling;
        public bool Channeling
        {
            get => _Channeling;
            protected set
            {
                if (value != _Channeling)
                    board.AddRoundEvent(new RoundEvent(this, EventType.StatusChanges)
                    {
                        statusType = StatusType.Channeling,
                        statusValue = value
                    });
            }
        }
        int ChannelingDurationCounter;
        protected Character channelTarget;
        float ChannelingDuration
        {
/*
            get { return ChannelingDurationCounter / board.defaultTicksPerSec; }
*/
            set => ChannelingDurationCounter = (int)(board.defaultTicksPerSec * value);
        }

        int GraduallyHealAmountPerTick;
        int GraduallyHealDurationCounter;
        float GraduallyHealDuration
        {
            //get { return GraduallyHealDurationCounter / board.defaultTicksPerSec; }
            set => GraduallyHealDurationCounter = (int)(board.defaultTicksPerSec * value);
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
                if(AttackTarget is {canBeTargeted: false})
                {
                    AttackTarget = null;
                }
                //Trigger property
                if (AttackTarget == null) AttackTarget = null;
                //Move to target if outside of attack range
                if (AttackTarget != null && board.Distance(AttackTarget.position, position) > currentStats.attackRange && !Moving)
                {
                    Moving = true;
                    moveCounter = 0;
                }
                if (Moving)
                {
                    moveCounter++;
                    if (moveCounter >= movingSpeed)
                    {
                        Move();
                        Moving = false;
                    }
                }
                //Attack
                if (++attackCounter >= TicksPerAttack && TicksPerAttack > 0 && !Moving)
                {
                    Attack();
                    attackCounter = 0;
                }                
            }
            //Check shield duration
            if (--shieldDurationCounter <= 0)
            {
                currentStats.shield = 0;
            }
            if (Sleep)
            {
                if(--sleepDurationCounter <= 0)
                {
                    Sleep = false;
                }
            }

            if (burnDurationCounter <= 0)
            {
                BonusIntakeDamage = 0;
                Burn = false;
            }
            else
            {
                burnDurationCounter--;
                OnHit(BurnSetter, burnDamageType, false, false, burnDamageEachSec);
            }
            if (DecreasedHealingDurationCounter <= 0) currentStats.decreasedHealing = 0;
            else --DecreasedHealingDurationCounter;

            if (stunDurationCounter <= 0) Stun = false;
            else stunDurationCounter--;

            if (ChannelingDurationCounter > 0)
            {
                ChannelingDurationCounter--;
                if (channelTarget != null && !board.Characters.Contains(channelTarget))
                {
                    ChannelingDurationCounter = 0;
                }
                if (ChannelingDurationCounter == 0)
                {
                    Channeling = false;
                }
            }

            if (BlindDurationCounter <= 0) Blind = false;
            else BlindDurationCounter--;
            if (GraduallyHealDurationCounter > 0)
            {
                GraduallyHealDurationCounter--;
                Heal(GraduallyHealAmountPerTick);
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
            BonusIntakeDamage = 0;
            BonusIntakeDamagePercentage = 1;
            currentStats = baseStats;
            currentStats.hp = currentStats.maxHp;
        }
        public virtual void Move()
        {
            position = board.PathFinding(position, AttackTarget.position)[1];
            board.AddRoundEvent(new RoundEvent(this,EventType.Move){
                linkedPositions = new(){position}
            });
        }
        public virtual void Attack()
        {
            if (!Blind)
            {
                bool crit = rand.Next(1, 100) <= currentStats.critRate;
                AttackTarget?.HitEvent.Invoke(this, DamageType.Physical, false, crit);
                board.AddRoundEvent(new RoundEvent(this,EventType.BasicAttack)
                {
                    linkedCharacters = new(){AttackTarget},
                    isCrit = crit
                });
            }

            foreach (Item i in items) i.OnAttack(AttackTarget);
            mana += 10;
        }
        public virtual int DamageCalculation(Character Attacker, float amount)
        {
            float damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 / (100 + currentStats.def); 
            foreach (Item i in items) damage = i.OnDamageCalculation((int)damage);
            return (int)damage;
        }
        public virtual int MagicDamageCalculation(Character Attacker, float amount)
        {
            float damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 / (100 + currentStats.specialDef);       
            foreach (Item i in items) damage = i.OnMagicDamageCalculation((int)damage);
            return (int)damage;
        }
        public virtual void OnHit(Character attacker, DamageType damageType, bool isSpecial = false, bool isCrit = false, float amount = 0)
        {
            int damage = 0;

            //Checking what type of damage is this
            //Basic attack
            if (damageType == DamageType.Physical && !isSpecial)
            {
                if(!(rand.Next(1,100) <= currentStats.dodgeRate))
                    damage = DamageCalculation(attacker, attacker.currentStats.atk);
            }
            //Other
            else if (damageType == DamageType.Physical)
            {
                damage = DamageCalculation(attacker, amount);
            }
            else if (damageType == DamageType.Magic)
            {
                damage = MagicDamageCalculation(attacker, (int)amount);
                //if(isSpecial) attacker.SpecialAttackAffected.Add(this);
            }
            else if(damageType == DamageType.True)
            {
                damage = amount != 0 ? (int)amount : attacker.currentStats.atk;
            }
            //Check if this is a critical hit
            if (isCrit) damage = (int)(damage * (1 + defaultBonusCritDamage));

            board.AddRoundEvent(new RoundEvent(this,EventType.Hitted)
            {
                damageType = damageType,
                value = damage,
                isCrit = isCrit,
                linkedCharacters = new(){attacker}
            });

            //Check sleep wakeup
            if (Sleep)
            {
                Sleep = false;
                OnHit(SleepSetter, DamageType.Magic, false, false, sleepWakeupDamage);
            }

            //Decreasing HP
            if (currentStats.shield > damage) currentStats.shield -= damage;
            else { currentStats.hp -= damage - currentStats.shield; currentStats.shield = 0; };

            //Checking if the character is dead
            //The first conditions bracket can be used to revive the character
            if (currentStats.hp <= 0 && !Dead)
            {
                OnKilled(attacker);                
            }
            //If the character isn't revived then it is dead :D
            if(currentStats.hp <= 0 && !Dead)
            {  
                Dead = true;
                board.charCounter[teamID]--;
                board.Characters.Remove(this);
                foreach (Character c in board.Characters.Where(x => x.AttackTarget == this))
                {
                    c.AttackTarget = null;
                }
                return;
            }

            //Adding intake damage mana
            if (damageType == DamageType.Physical) mana += attacker.currentStats.atk / 100 + damage * 7 / 100;
            else if (damageType == DamageType.Magic) mana += (int)amount / 100 + damage * 7 / 100;
        }
        public virtual void OnManaChange(int manaChange)
        {
            if(mana >= currentStats.maxMana && !Sleep && !Stun)
            {
                mana = 0;
                currentStats.maxMana = baseStats.maxMana; //Reset max mana if increased
                SpecialMove();
            }
        }
        public virtual void SpecialMove() {
            SpecialAttackAffected.Clear();
            foreach (Item i in items) i.OnSpecialMove();
            board.AddRoundEvent(new RoundEvent(this, EventType.SpecialAttack));
            //foreach (Set s in set) s.OnSpecialMove();
        }
        public virtual void OnKilled(Character Killer) { }
        public virtual void OnProjectileHit(Character Hitted){ }
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

        public virtual void Heal(int amount)
        {
            board.AddRoundEvent(new RoundEvent(this, EventType.Healing)
            {
                value = amount
            });
        }
        public virtual void GraduallyHeal(int amount, float duration)
        {
            GraduallyHealDuration = duration;
            GraduallyHealAmountPerTick = amount / GraduallyHealDurationCounter;
        }
        public virtual void Cleanse()
        {
            Stun = false;
            Sleep = false;
            Blind = false;
            Burn = false;
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
        public virtual void SetBurn(Character setter, float duration, int burnDamage, DamageType burnType, float BonusIntakeDamage = 0)
        {
            Burn = true;
            BurnSetter = setter;
            burnDuration = duration;
            burnDamageEachSec = burnDamage / burnDurationCounter;
            burnDamageType = burnType;
            BonusIntakeDamagePercentage *= 1+BonusIntakeDamage;
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
            Blind = true;
            BlindSetter = setter;
            BlindDuration = duration;
        }
        public virtual void SetChanneling(float duration, Character channelTarget = null)
        {
            Channeling = true;
            ChannelingDuration = duration;
            this.channelTarget = channelTarget;
        }
    }
}
