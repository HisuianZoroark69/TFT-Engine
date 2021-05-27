using System;
using System.Collections.Generic;
using System.Linq;
using TFT_Engine.Effects;

namespace TFT_Engine.Components
{
    public enum DamageType
    {
        Physical,
        Magic,
        True
    }

    public class CharList : List<Character>
    {
        public CharList(CharList cl) : base(cl)
        {
        }

        public CharList()
        {
        }

        public Character this[Character c] => Find(x => x == c);

        public Character this[Position p, bool collision = true] =>
            Find(x => x.position == p && collision == x.collision);

        public CharList this[Guid teamId] => FindAll(x => x.teamID == teamId) as CharList;
    }

    public class Character
    {
        public delegate void HitEventHandler(Character Attacker, DamageType damageType, bool isSpecial = false,
            bool isCrit = false, double amount = 0);

        public delegate void ManaChangeEventHandler(double ManaChange);

        


        private Character _attackTarget;

        //Blind
        private bool _Blind;

        //Burn
        //private bool _Burn;

        private bool _Channeling;

        protected double _mana;
        private double _oldMana;


        /// <summary>
        ///     Set to sleep and wakeup damage
        /// </summary>
        private bool _Sleep;

        //Stun
        private bool _Stun;

        private int attackCounter;
        public Position basePosition;
        public Statistics baseStats;
        private int BlindDurationCounter;
        private Character BlindSetter;
        private Set blindSetterSet;
        public Board board;
        public float BonusIntakeDamage;
        public float BonusIntakeDamagePercentage;
        //protected double burnDamageEachSec;
        //private DamageType burnDamageType;
        //protected int burnDurationCounter;
        //protected Character BurnSetter;
        //protected Set burnSetterSet;
        public bool canBeTargeted;
        private int ChannelingDurationCounter;
        protected Character channelTarget;
        public bool collision;
        public int cost;
        public Statistics currentStats;
        protected bool Dead;
        private int DecreasedHealingDurationCounter;
        public float defaultBonusCritDamage = 0.5f;
        public int FamilyId;

        private int GraduallyHealAmountPerTick;
        private int GraduallyHealDurationCounter;
        public bool ImmuneCC;
        public List<Item> items;
        private byte moveCounter;

        private bool Moving;

        public string Name;

        private Position _position;
        public Position position
        {
            get => _position;
            set
            {
                _position = value;
                board.AddRoundEvent(new RoundEvent(this, EventType.Move)
                {
                    linkedPositions = value
                });
            }
        }

        public Random rand = new();
        public List<Set> set;
        protected int shieldDurationCounter;
        //private int sleepDurationCounter;
        //private Character SleepSetter;
        //private Set sleepSetterSet;
        //private double sleepWakeupDamage;
        public HashSet<Character> SpecialAttackAffected;
        public int star;
        //private int stunDurationCounter;
        //private Character Stunner;
        //private Set stunnerSet;
        public Guid teamID;

        public Character(string name, Guid teamId, Statistics baseStats, int familyId, int star, int cost,
            params Set[] s)
        {
            this.star = star;
            this.cost = cost;
            FamilyId = familyId;
            Name = name;
            teamID = teamId;
            this.baseStats = baseStats;
            HitEvent += OnHit;
            ManaChangeEvent += OnManaChange;
            items = new List<Item>();
            set = new List<Set>(s);
        }

        public Character AttackTarget
        {
            get => _attackTarget;
            set
            {
                //Finding target
                if (value == null)
                {
                    var minDistance = int.MaxValue;
                    foreach (var c in board.Characters)
                    {
                        var distance = board.Distance(c.position, position);
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

        protected float shieldDuration
        {
            get => shieldDurationCounter / board.defaultTicksPerSec;
            set => shieldDurationCounter = (int) (board.defaultTicksPerSec * value);
        }

        public double mana
        {
            get => _mana;
            set
            {
                if (!Channeling)
                {
                    currentStats.mana = value;
                    board.AddRoundEvent(new RoundEvent(this, EventType.ManaChange)
                    {
                        value = value - _mana
                    });
                    _mana = value;
                    ManaChangeEvent?.Invoke(value - _oldMana);
                    _oldMana = value;
                }
            }
        }

        private int TicksPerAttack
        {
            get
            {
                try
                {
                    return (int) (board.defaultTicksPerSec / currentStats.attackSpeed);
                }
                catch (DivideByZeroException)
                {
                    return 0;
                }
            }
        }

        private int movingSpeed => (int) (board.defaultTicksPerSec * currentStats.movingSpeed);

        public bool Sleep
        {
            get => _Sleep;
            protected set
            {
                if (value != _Sleep)
                    board.AddRoundEvent(new RoundEvent(this, EventType.Effects)
                    {
                        EffectName = "Sleep",
                        EffectValue = value
                    });

                _Sleep = value;
            }
        }

        /*private float SleepDuration
        {
            //get => sleepDurationCounter / board.defaultTicksPerSec;
            set => sleepDurationCounter = (int) (board.defaultTicksPerSec * value);
        }*/

        /*public bool Stun
        {
            get => _Stun;
            protected set
            {
                if (value != _Stun)
                    board.AddRoundEvent(new RoundEvent(this, EventType.Effects)
                    {
                        EffectName = "Stun",
                        EffectValue = value
                    });
                _Stun = value;
            }
        }*/

        /*protected float StunDuration
        {
            get => stunDurationCounter / board.defaultTicksPerSec;
            set => stunDurationCounter = (int) (board.defaultTicksPerSec * value);
        }*/

        /*public bool Burn
        {
            get => _Burn;
            protected set
            {
                if (value != _Burn)
                    board.AddRoundEvent(new RoundEvent(this, EventType.StatusChanges)
                    {
                        statusType = StatusType.Burn,
                        EffectValue = value
                    });
                _Burn = value;
            }
        }*/

        /*private float burnDuration
        {
            set => burnDurationCounter = (int) (board.defaultTicksPerSec * value);
        }*/

        public bool Blind
        {
            get => _Blind;
            protected set
            {
                if (value != _Blind)
                    board.AddRoundEvent(new RoundEvent(this, EventType.Effects)
                    {
                        EffectName = "Blind",
                        EffectValue = value
                    });
                _Blind = value;
            }
        }

        private float BlindDuration
        {
            get => BlindDurationCounter / board.defaultTicksPerSec;
            set => BlindDurationCounter = (int) (board.defaultTicksPerSec * value);
        }

        public bool Channeling
        {
            get => _Channeling;
            protected set
            {
                if (value != _Channeling)
                    board.AddRoundEvent(new RoundEvent(this, EventType.Effects)
                    {
                        EffectName = "Channeling",
                        EffectValue = value
                    });
                _Channeling = value;
            }
        }

        private float ChannelingDuration
        {
/*
            get { return ChannelingDurationCounter / board.defaultTicksPerSec; }
*/
            set => ChannelingDurationCounter = (int) (board.defaultTicksPerSec * value);
        }

        private float GraduallyHealDuration
        {
            //get { return GraduallyHealDurationCounter / board.defaultTicksPerSec; }
            set => GraduallyHealDurationCounter = (int) (board.defaultTicksPerSec * value);
        }

        public event HitEventHandler HitEvent;
        public event ManaChangeEventHandler ManaChangeEvent;

        /// <summary>
        ///     This runs every ticks
        /// </summary>
        public virtual void OverrideOnTick()
        {
        }
        public void OnTick()
        {
            AttackTarget = null;
            bool cc = (from x in board.effects where x.Effected == this && x is Effects.Sleep or Effects.Stun select x).Any();
            if (cc) Channeling = false;
            //Check if character is dead or stun or sleep
            if (!Dead && !cc && !Channeling)
            {
                //Checking if existing target can be targeted
                if (AttackTarget is {canBeTargeted: false}) AttackTarget = null;
                //Move to target if outside of attack range
                if (board.Distance(AttackTarget.position, position) > currentStats.attackRange && !Moving)
                {
                    Move();
                    Moving = true;
                    moveCounter = 0;
                }

                if (Moving)
                {
                    moveCounter++;
                    if (moveCounter >= movingSpeed)
                    {
                        moveCounter = 0;
                        Moving = false;
                    }
                }

                //Attack
                if (board.Distance(AttackTarget.position, position) <= currentStats.attackRange && ++attackCounter >= TicksPerAttack && TicksPerAttack > 0 && !Moving)
                {
                    Attack();
                    attackCounter = 0;
                }
            }

            //Check shield duration
            if (--shieldDurationCounter <= 0) currentStats.shield = 0;

            /*if (Sleep)
                if (--sleepDurationCounter <= 0)
                    Sleep = false;*/

            /*if (burnDurationCounter <= 0)
            {
                BonusIntakeDamage = 0;
                BurnSetter = null;
                burnSetterSet = null;
                Burn = false;
            }
            else
            {
                burnDurationCounter--;
                if (BurnSetter != null) OnHit(BurnSetter, burnDamageType, false, false, burnDamageEachSec);
                if (burnSetterSet != null) OnHit(burnSetterSet, burnDamageType, burnDamageEachSec);
            }*/

            if (DecreasedHealingDurationCounter <= 0) currentStats.decreasedHealing = 0;
            else --DecreasedHealingDurationCounter;

            /*if (stunDurationCounter <= 0) Stun = false;
            else stunDurationCounter--;*/

            if (ChannelingDurationCounter > 0 && Channeling)
            {
                ChannelingDurationCounter--;
                if (channelTarget != null && !board.Characters.Contains(channelTarget))
                    ChannelingDurationCounter = 0;
                if (ChannelingDurationCounter == 0) 
                    Channeling = false;
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

        public virtual void OnFindNewTarget()
        {
        }

        public virtual void OnStart()
        {
            position = basePosition;
            SpecialAttackAffected = new HashSet<Character>();
            Dead = false;
            Moving = false;
            Sleep = false;
            Blind = false;
            //Stun = false;
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
        }

        public virtual void Attack()
        {
            if (!Blind)
            {
                var crit = rand.Next(1, 100) <= currentStats.critRate;
                AttackTarget?.HitEvent.Invoke(this, DamageType.Physical, false, crit);
                board.AddRoundEvent(new RoundEvent(this, EventType.BasicAttack)
                {
                    linkedCharacters = new CharList {AttackTarget},
                    isCrit = crit
                });
            }

            foreach (var i in items) i.OnAttack(AttackTarget);
            mana += 10;
        }

        public virtual double PhysicalDamageCalculation(Character Attacker, double amount)
        {
            var damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 / (100 + currentStats.def);
            foreach (var i in items) damage = i.OnDamageCalculation(damage);
            return damage;
        }

        public virtual double MagicDamageCalculation(Character Attacker, double amount)
        {
            var damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 /
                         (100 + currentStats.specialDef);
            foreach (var i in items) damage = i.OnMagicDamageCalculation(damage);
            return damage;
        }

        public virtual double PhysicalDamageCalculation(Set Attacker, double amount)
        {
            var damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 / (100 + currentStats.def);
            foreach (var i in items) damage = i.OnDamageCalculation(damage);
            return damage;
        }

        public virtual double MagicDamageCalculation(Set Attacker, double amount)
        {
            var damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 /
                         (100 + currentStats.specialDef);
            foreach (var i in items) damage = i.OnMagicDamageCalculation(damage);
            return damage;
        }
        public virtual double PhysicalDamageCalculation(double amount)
        {
            var damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 / (100 + currentStats.def);
            return damage;
        }

        public virtual double MagicDamageCalculation(double amount)
        {
            var damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 /
                         (100 + currentStats.specialDef);
            return damage;
        }

        public virtual void OnHit(Character attacker, DamageType damageType, bool isSpecial = false,
            bool isCrit = false, double amount = 0)
        {
            double damage = 0;

            //Checking what type of damage is this
            //Basic attack
            if (damageType == DamageType.Physical && !isSpecial)
            {
                if (!(rand.Next(1, 100) <= currentStats.dodgeRate))
                    damage = PhysicalDamageCalculation(attacker, attacker.currentStats.atk);
            }
            //Other
            else if (damageType == DamageType.Physical)
            {
                damage = PhysicalDamageCalculation(attacker, amount);
            }
            else if (damageType == DamageType.Magic)
            {
                damage = MagicDamageCalculation(attacker, amount);
                //if(isSpecial) attacker.SpecialAttackAffected.Add(this);
            }
            else if (damageType == DamageType.True)
            {
                damage = amount != 0 ? amount : attacker.currentStats.atk;
            }

            //Check if this is a critical hit
            if (isCrit) damage = damage * (1 + defaultBonusCritDamage);

            board.AddRoundEvent(new RoundEvent(this, EventType.Hitted)
            {
                damageType = damageType,
                value = damage,
                isCrit = isCrit,
                linkedCharacters = new CharList {attacker}
            });

            //Check sleep wakeup
            /*if (Sleep)
            {
                Sleep = false;
                OnHit(SleepSetter, DamageType.Magic, false, false, sleepWakeupDamage);
            }*/

            //Decreasing HP
            if (currentStats.shield > damage)
            {
                currentStats.shield -= damage;
            }
            else
            {
                currentStats.hp -= damage - currentStats.shield;
                currentStats.shield = 0;
            }

            ;

            //Checking if the character is dead
            //The first conditions bracket can be used to revive the character
            if (currentStats.hp <= 0 && !Dead) OnKilled(attacker);
            //If the character isn't revived then it is dead :D
            if (currentStats.hp <= 0 && !Dead)
            {
                Dead = true;
                board.AddRoundEvent(new RoundEvent(this, EventType.Dead));
                board.charCounter[teamID]--;
                board.Characters.Remove(this);
                foreach (var c in board.Characters.Where(x => x.AttackTarget == this)) c.AttackTarget = null;
                return;
            }

            //Adding intake damage mana
            if (damageType == DamageType.Physical && !isSpecial)
                mana += attacker.currentStats.atk / 100d + damage * 7d / 100d;
            else mana += amount / 100d + damage * 7d / 100d;
        }

        public virtual void OnHit(Set attacker, DamageType damageType, double amount, bool isSpecial = false,
            bool isCrit = false)
        {
            double damage = 0;

            //Checking what type of damage is this
            //Basic attack
            if (damageType == DamageType.Physical && !isSpecial)
            {
                if (!(rand.Next(1, 100) <= currentStats.dodgeRate))
                    damage = PhysicalDamageCalculation(attacker, amount);
            }
            //Other
            else if (damageType == DamageType.Physical)
            {
                damage = PhysicalDamageCalculation(attacker, amount);
            }
            else if (damageType == DamageType.Magic)
            {
                damage = MagicDamageCalculation(attacker, amount);
                //if(isSpecial) attacker.SpecialAttackAffected.Add(this);
            }
            else if (damageType == DamageType.True)
            {
                damage = amount;
            }

            //Check if this is a critical hit
            if (isCrit) damage = damage * (1 + defaultBonusCritDamage);

            board.AddRoundEvent(new RoundEvent(this, EventType.Hitted)
            {
                damageType = damageType,
                value = damage,
                isCrit = isCrit,
                linkedSet = attacker
            });

            //Check sleep wakeup
            /*if (Sleep)
            {
                Sleep = false;
                OnHit(SleepSetter, DamageType.Magic, false, false, sleepWakeupDamage);
            }*/

            //Decreasing HP
            if (currentStats.shield > damage)
            {
                currentStats.shield -= damage;
            }
            else
            {
                currentStats.hp -= damage - currentStats.shield;
                currentStats.shield = 0;
            }

            ;

            //Checking if the character is dead
            //The first conditions bracket can be used to revive the character
            if (currentStats.hp <= 0 && !Dead) OnKilled(attacker);
            //If the character isn't revived then it is dead :D
            if (currentStats.hp <= 0 && !Dead)
            {
                Dead = true;
                board.AddRoundEvent(new RoundEvent(this, EventType.Dead));
                board.charCounter[teamID]--;
                board.Characters.Remove(this);
                foreach (var c in board.Characters.Where(x => x.AttackTarget == this)) c.AttackTarget = null;
                return;
            }

            //Adding intake damage mana
            mana += amount / 100d + damage * 7d / 100d;
        }

        public virtual void OnManaChange(double manaChange)
        {
            if (mana >= currentStats.maxMana && !Channeling && !(from x in board.effects where x.Effected == this && x is Effects.Sleep or Effects.Stun select x).Any())
            {
                mana = 0;
                currentStats.maxMana = baseStats.maxMana; //Reset max mana if increased
                BaseSpecialMove();
            }
        }

        public virtual void SpecialMove()
        {
        }

        public void BaseSpecialMove()
        {
            SpecialAttackAffected.Clear();
            foreach (var i in items) i.OnSpecialMove();
            board.AddRoundEvent(new RoundEvent(this, EventType.SpecialAttack));
            SpecialMove();
        }

        public virtual void OnKilled(Character Killer)
        {
        }

        public virtual void OnKilled(Set Killer)
        {
        }

        public virtual void OnProjectileHit(Character Hitted)
        {
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

        public virtual void Heal(double amount)
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
            foreach (Effect e in board.effects)
            {
                if (e is Effects.Stun or Effects.Sleep)
                {
                    e.Abort();
                }
            }
            //Stun = false;
            Sleep = false;
            Blind = false;
            //Burn = false;
        }

        public virtual void SetSleep(Character setter, float duration, double wakeupDamage)
        {
            if (!ImmuneCC)
            {
                Sleep sleep = new(duration,setter,this,wakeupDamage);
                board.AddEffect(sleep);
                /*SleepSetter = setter;
                SleepDuration = duration;
                Sleep = true;
                sleepWakeupDamage = wakeupDamage;*/
            }
        }

        public virtual void SetBurn(Character setter, float duration, double burnDamage, DamageType burnType, float BonusIntakeDamage = 0)
        {
            //Burn = true;
            Effect burn = new Burn(duration, burnType, burnDamage, setter, this);
            board.AddEffect(burn);
            /*BurnSetter = setter;
            burnDuration = duration;
            burnDamageEachSec = burnDamage / burnDurationCounter;
            burnDamageType = burnType;*/
            BonusIntakeDamagePercentage *= 1 + BonusIntakeDamage;
        }

        public virtual void SetBurn(Set setter, float duration, double burnDamage, DamageType burnType,
            float BonusIntakeDamage = 0)
        {
            Effect burn = new Burn(duration, burnType, burnDamage, setter, this);
            board.AddEffect(burn);
            /*Burn = true;
            burnSetterSet = setter;
            burnDuration = duration;
            burnDamageEachSec = burnDamage / burnDurationCounter;
            burnDamageType = burnType;
            BonusIntakeDamagePercentage *= 1 + BonusIntakeDamage;*/
        }

        public virtual void SetStun(Character setter, float duration)
        {
            if (!ImmuneCC)
            {
                Stun stun = new(duration, setter, this);
                board.AddEffect(stun);
                /*Stun = true;
                Stunner = setter;
                StunDuration = duration;*/
            }
        }

        public virtual void SetStun(Set setter, float duration)
        {
            if (!ImmuneCC)
            {
                Stun stun = new(duration, setter, this);
                board.AddEffect(stun);
                /*Stun = true;
                stunnerSet = setter;
                StunDuration = duration;*/
            }
        }

        public virtual void SetBlind(Character setter, float duration)
        {
            Blind = true;
            BlindSetter = setter;
            BlindDuration = duration;
        }

        public virtual void SetBlind(Set setter, float duration)
        {
            Blind = true;
            blindSetterSet = setter;
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