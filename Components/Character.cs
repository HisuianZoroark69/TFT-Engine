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
            Find(x => x.Position == p && collision == x.Collision);

        public CharList this[Guid teamId] => FindAll(x => x.TeamId == teamId) as CharList;
    }

    public class Character
    {
        public delegate void HitEventHandler(Character attacker, DamageType damageType, bool isSpecial = false,
            bool isCrit = false, double amount = 0);

        public delegate void ManaChangeEventHandler(double manaChange);

        public bool CCed => (from x in Board.Effects where x.Effected == this && x is Effects.Sleep or Effects.Stun select x).Any();

        private Character _attackTarget;

        //Blind
        private bool _blind;

        //Burn
        //private bool _Burn;

        private bool _channeling;

        protected double mana;
        private double _oldMana;


        /// <summary>
        ///     Set to sleep and wakeup damage
        /// </summary>
        private bool _sleep;

        //Stun
        private bool _stun;

        private int _attackCounter;
        public Position BasePosition;
        public Statistics BaseStats;
        private int _blindDurationCounter;
        private Character _blindSetter;
        private Set _blindSetterSet;
        public Board Board;
        public float BonusIntakeDamage;
        public float BonusIntakeDamagePercentage;
        //protected double burnDamageEachSec;
        //private DamageType burnDamageType;
        //protected int burnDurationCounter;
        //protected Character BurnSetter;
        //protected Set burnSetterSet;
        public bool CanBeTargeted;
        private int _channelingDurationCounter;
        protected Character ChannelTarget;
        public bool Collision;
        public int Cost;
        public Statistics CurrentStats;
        protected bool Dead;
        private int _decreasedHealingDurationCounter;
        public float DefaultBonusCritDamage = 0.5f;
        public int FamilyId;

        private int _graduallyHealAmountPerTick;
        private int _graduallyHealDurationCounter;
        public bool ImmuneCc;
        public List<Item> Items;
        private byte _moveCounter;

        private bool _moving;

        public string Name;

        private Position _position;
        public Position Position
        {
            get => _position;
            set
            {
                _position = value;
                Board.AddRoundEvent(new RoundEvent(this, EventType.Move)
                {
                    LinkedPositions = value
                });
            }
        }

        public Random Rand = new();
        public List<Set> Set;
        protected int ShieldDurationCounter;
        public HashSet<Character> SpecialAttackAffected;
        public int Star;
        public Guid TeamId;

        public Character(string name, Guid teamId, Statistics baseStats, int familyId, int star, int cost,
            params Set[] s)
        {
            Star = star;
            Cost = cost;
            FamilyId = familyId;
            Name = name;
            TeamId = teamId;
            BaseStats = baseStats;
            HitEvent += OnHit;
            ManaChangeEvent += OnManaChange;
            Items = new List<Item>();
            Set = new List<Set>(s);
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
                    foreach (var c in Board.Characters)
                    {
                        var distance = Board.Distance(c.Position, Position);
                        if (c.CanBeTargeted && !c.Dead && c.TeamId != TeamId && distance < minDistance)
                        {
                            _attackTarget = c;
                            minDistance = distance;
                        }
                    }

                    OnFindNewTarget();
                }
                else
                {
                    _attackTarget = value;
                    OnFindNewTarget();
                }
            }
        }

        protected double ShieldDuration
        {
            get => ShieldDurationCounter / Board.DefaultTicksPerSec;
            set => ShieldDurationCounter = (int)(Board.DefaultTicksPerSec * value);
        }

        public double Mana
        {
            get => _oldMana;
            set
            {
                if (!Channeling)
                {
                    CurrentStats.Mana = value;
                    Board.AddRoundEvent(new RoundEvent(this, EventType.ManaChange)
                    {
                        Value = value - _oldMana
                    });
                    _oldMana = value;
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
                    return (int)(Board.DefaultTicksPerSec / CurrentStats.AttackSpeed);
                }
                catch (DivideByZeroException)
                {
                    return 0;
                }
            }
        }

        private int MovingSpeed => (int)(Board.DefaultTicksPerSec * CurrentStats.MovingSpeed);

        public bool Sleep
        {
            get => _sleep;
            protected set
            {
                if (value != _sleep)
                    Board.AddRoundEvent(new RoundEvent(this, EventType.Effects)
                    {
                        EffectName = "Sleep",
                        EffectValue = value
                    });

                _sleep = value;
            }
        }
        public bool Blind
        {
            get => _blind;
            protected set
            {
                if (value != _blind)
                    Board.AddRoundEvent(new RoundEvent(this, EventType.Effects)
                    {
                        EffectName = "Blind",
                        EffectValue = value
                    });
                _blind = value;
            }
        }

        private float BlindDuration
        {
            set => _blindDurationCounter = (int)(Board.DefaultTicksPerSec * value);
        }

        public bool Channeling
        {
            get => _channeling;
            protected set
            {
                if (value != _channeling)
                    Board.AddRoundEvent(new RoundEvent(this, EventType.Effects)
                    {
                        EffectName = "Channeling",
                        EffectValue = value
                    });
                _channeling = value;
            }
        }

        private double ChannelingDuration
        {
            set => _channelingDurationCounter = (int)(Board.DefaultTicksPerSec * value);
        }

        private float GraduallyHealDuration
        {
            //get { return GraduallyHealDurationCounter / board.defaultTicksPerSec; }
            set => _graduallyHealDurationCounter = (int)(Board.DefaultTicksPerSec * value);
        }

        public event HitEventHandler HitEvent;
        public event ManaChangeEventHandler ManaChangeEvent;

        //Physics things


        /// <summary>
        ///     This runs every ticks
        /// </summary>
        public virtual void OverrideOnTick()
        {
        }

        public void OnTick()
        {
            AttackTarget = null;
            if (CCed) Channeling = false;
            //Check if character is dead or stun or sleep
            if (!Dead && !CCed && !Channeling)
            {
                //Checking if existing target can be targeted
                if (AttackTarget is { CanBeTargeted: false }) AttackTarget = null;
                //Move to target if outside of attack range
                if (Board.Distance(AttackTarget.Position, Position) > CurrentStats.AttackRange && !_moving)
                {
                    Move();
                    _moving = true;
                    _moveCounter = 0;
                }

                if (_moving)
                {
                    _moveCounter++;
                    if (_moveCounter >= MovingSpeed)
                    {
                        _moveCounter = 0;
                        _moving = false;
                    }
                }

                //Attack
                if (Board.Distance(AttackTarget.Position, Position) <= CurrentStats.AttackRange &&
                    ++_attackCounter >= TicksPerAttack && TicksPerAttack > 0 && !_moving)
                {
                    Attack();
                    _attackCounter = 0;
                }
            }

            //Check shield duration
            if (ShieldDurationCounter > 0)
            {
                ShieldDurationCounter--;
                if (ShieldDurationCounter == 0)
                {
                    CurrentStats.Shield = 0;
                    Board.AddRoundEvent(new RoundEvent(this, EventType.Shield)
                    {
                        EffectValue = false,
                        Value = 0
                    });
                }
            }
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

            if (_decreasedHealingDurationCounter <= 0) CurrentStats.DecreasedHealing = 0;
            else --_decreasedHealingDurationCounter;

            /*if (stunDurationCounter <= 0) Stun = false;
            else stunDurationCounter--;*/

            if (_channelingDurationCounter > 0 && Channeling)
            {
                _channelingDurationCounter--;
                if (ChannelTarget != null && !Board.Characters.Contains(ChannelTarget))
                    _channelingDurationCounter = 0;
                if (_channelingDurationCounter == 0)
                    Channeling = false;
            }

            if (_blindDurationCounter <= 0) Blind = false;
            else _blindDurationCounter--;
            if (_graduallyHealDurationCounter > 0)
            {
                _graduallyHealDurationCounter--;
                Heal(_graduallyHealAmountPerTick);
            }
            OverrideOnTick();
        }

        public virtual void OnFindNewTarget()
        {
        }

        public virtual void OnStart()
        {
            Position = BasePosition;
            SpecialAttackAffected = new HashSet<Character>();
            Dead = false;
            _moving = false;
            Sleep = false;
            Blind = false;
            //Stun = false;
            Collision = true;
            CanBeTargeted = true;
            ImmuneCc = false;
            _attackCounter = 0;
            BonusIntakeDamage = 0;
            BonusIntakeDamagePercentage = 1;
            CurrentStats = BaseStats;
            CurrentStats.Hp = CurrentStats.MaxHp;
        }

        public virtual void Move()
        {
            Position = Board.PathFinding(Position, AttackTarget.Position)[1];
        }

        public virtual void Attack()
        {
            if (!Blind)
            {
                var crit = Rand.Next(1, 100) <= CurrentStats.CritRate;
                AttackTarget?.HitEvent.Invoke(this, DamageType.Physical, false, crit);
                Board.AddRoundEvent(new RoundEvent(this, EventType.BasicAttack)
                {
                    LinkedCharacters = new CharList { AttackTarget },
                    IsCrit = crit
                });
            }

            foreach (var i in Items) i.OnAttack(AttackTarget);
            mana += 10;
        }

        public virtual double PhysicalDamageCalculation(Character attacker, double amount)
        {
            var damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 / (100 + CurrentStats.Def);
            foreach (var i in Items) damage = i.OnDamageCalculation(damage);
            return damage;
        }

        public virtual double MagicDamageCalculation(Character attacker, double amount)
        {
            var damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 /
                         (100 + CurrentStats.SpecialDef);
            foreach (var i in Items) damage = i.OnMagicDamageCalculation(damage);
            return damage;
        }

        public virtual double PhysicalDamageCalculation(Set attacker, double amount)
        {
            var damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 / (100 + CurrentStats.Def);
            foreach (var i in Items) damage = i.OnDamageCalculation(damage);
            return damage;
        }

        public virtual double MagicDamageCalculation(Set attacker, double amount)
        {
            var damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 /
                         (100 + CurrentStats.SpecialDef);
            foreach (var i in Items) damage = i.OnMagicDamageCalculation(damage);
            return damage;
        }
        public virtual double PhysicalDamageCalculation(double amount)
        {
            var damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 / (100 + CurrentStats.Def);
            return damage;
        }

        public virtual double MagicDamageCalculation(double amount)
        {
            var damage = (amount + BonusIntakeDamage) * BonusIntakeDamagePercentage * 100 /
                         (100 + CurrentStats.SpecialDef);
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
                if (!(Rand.Next(1, 100) <= CurrentStats.DodgeRate))
                    damage = PhysicalDamageCalculation(attacker, attacker.CurrentStats.Atk);
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
                damage = amount != 0 ? amount : attacker.CurrentStats.Atk;
            }

            //Check if this is a critical hit
            if (isCrit) damage = damage * (1 + DefaultBonusCritDamage);

            Board.AddRoundEvent(new RoundEvent(this, EventType.Hitted)
            {
                DamageType = damageType,
                Value = damage,
                IsCrit = isCrit,
                LinkedCharacters = new CharList { attacker }
            });

            //Check sleep wakeup
            /*if (Sleep)
            {
                Sleep = false;
                OnHit(SleepSetter, DamageType.Magic, false, false, sleepWakeupDamage);
            }*/

            //Decreasing HP
            if (CurrentStats.Shield > damage)
            {
                CurrentStats.Shield -= damage;
            }
            else
            {
                CurrentStats.Hp -= damage - CurrentStats.Shield;
                CurrentStats.Shield = 0;
            }

            ;

            //Checking if the character is dead
            //The first conditions bracket can be used to revive the character
            if (CurrentStats.Hp <= 0 && !Dead) OnKilled(attacker);
            //If the character isn't revived then it is dead :D
            if (CurrentStats.Hp <= 0 && !Dead)
            {
                Dead = true;
                Board.AddRoundEvent(new RoundEvent(this, EventType.Dead));
                Board.CharCounter[TeamId]--;
                Board.Characters.Remove(this);
                foreach (var c in Board.Characters.Where(x => x.AttackTarget == this)) c.AttackTarget = null;
                return;
            }

            //Adding intake damage mana
            if (damageType == DamageType.Physical && !isSpecial)
                mana += attacker.CurrentStats.Atk / 100d + damage * 7d / 100d;
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
                if (!(Rand.Next(1, 100) <= CurrentStats.DodgeRate))
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
            if (isCrit) damage = damage * (1 + DefaultBonusCritDamage);

            Board.AddRoundEvent(new RoundEvent(this, EventType.Hitted)
            {
                DamageType = damageType,
                Value = damage,
                IsCrit = isCrit,
                LinkedSet = attacker
            });

            //Check sleep wakeup
            /*if (Sleep)
            {
                Sleep = false;
                OnHit(SleepSetter, DamageType.Magic, false, false, sleepWakeupDamage);
            }*/

            //Decreasing HP
            if (CurrentStats.Shield > damage)
            {
                CurrentStats.Shield -= damage;
            }
            else
            {
                CurrentStats.Hp -= damage - CurrentStats.Shield;
                CurrentStats.Shield = 0;
            }

            ;

            //Checking if the character is dead
            //The first conditions bracket can be used to revive the character
            if (CurrentStats.Hp <= 0 && !Dead) OnKilled(attacker);
            //If the character isn't revived then it is dead :D
            if (CurrentStats.Hp <= 0 && !Dead)
            {
                Dead = true;
                Board.AddRoundEvent(new RoundEvent(this, EventType.Dead));
                Board.CharCounter[TeamId]--;
                Board.Characters.Remove(this);
                foreach (var c in Board.Characters.Where(x => x.AttackTarget == this)) c.AttackTarget = null;
                return;
            }

            //Adding intake damage mana
            mana += amount / 100d + damage * 7d / 100d;
        }

        public virtual void OnManaChange(double manaChange)
        {
            if (mana >= CurrentStats.MaxMana && !Channeling && !(from x in Board.Effects where x.Effected == this && x is Effects.Sleep or Effects.Stun select x).Any())
            {
                mana = 0;
                CurrentStats.MaxMana = BaseStats.MaxMana; //Reset max mana if increased
                BaseSpecialMove();
            }
        }

        public virtual void SpecialMove()
        {
        }

        public void BaseSpecialMove()
        {
            SpecialAttackAffected.Clear();
            foreach (var i in Items) i.OnSpecialMove();
            Board.AddRoundEvent(new RoundEvent(this, EventType.SpecialAttack));
            SpecialMove();
        }

        public virtual void OnKilled(Character killer)
        {
        }

        public virtual void OnKilled(Set killer)
        {
        }

        public virtual void OnProjectileHit(Character hitted)
        {
        }

        public virtual void AddItem(Item item)
        {
            Board.TickEvent += item.OnTick;
            Board.StartEvent += item.OnStart;
            Board.EndEvent += item.OnEnd;
            HitEvent += item.OnHit;
            ManaChangeEvent += item.OnManaChange;

            item.Holder = this;
            Items.Add(item);
        }

        public virtual void RemoveItem(Item item)
        {
            Board.TickEvent -= item.OnTick;
            Board.StartEvent -= item.OnStart;
            Board.EndEvent -= item.OnEnd;
            HitEvent -= item.OnHit;
            ManaChangeEvent -= item.OnManaChange;

            item.Holder = null;
            Items.Remove(item);
        }

        public virtual void AddShield(int amount, double duration)
        {
            CurrentStats.Shield += amount;
            Board.AddRoundEvent(new RoundEvent(this, EventType.Shield)
            {
                EffectValue = true,
                Value = amount
            });
            ShieldDuration = duration;
        }

        public virtual void Heal(double amount)
        {
            Board.AddRoundEvent(new RoundEvent(this, EventType.Healing)
            {
                Value = amount
            });
        }

        public virtual void GraduallyHeal(int amount, float duration)
        {
            GraduallyHealDuration = duration;
            _graduallyHealAmountPerTick = amount / _graduallyHealDurationCounter;
        }

        public virtual void Cleanse()
        {
            foreach (var e in Board.Effects)
            {
                if (e is Stun or Effects.Sleep or Burn)
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
            if (!ImmuneCc)
            {
                Sleep sleep = new(duration, setter, this, wakeupDamage);
                Board.AddEffect(sleep);
                /*SleepSetter = setter;
                SleepDuration = duration;
                Sleep = true;
                sleepWakeupDamage = wakeupDamage;*/
            }
        }

        public virtual void SetBurn(Character setter, double duration, double burnDamage, DamageType burnType, float bonusIntakeDamage = 0)
        {
            //Burn = true;
            Effect burn = new Burn(duration, burnType, burnDamage, setter, this);
            Board.AddEffect(burn);
            /*BurnSetter = setter;
            burnDuration = duration;
            burnDamageEachSec = burnDamage / burnDurationCounter;
            burnDamageType = burnType;*/
            BonusIntakeDamagePercentage *= 1 + bonusIntakeDamage;
        }

        public virtual void SetBurn(Set setter, double duration, double burnDamage, DamageType burnType,
            float bonusIntakeDamage = 0)
        {
            Effect burn = new Burn(duration, burnType, burnDamage, setter, this);
            Board.AddEffect(burn);
            /*Burn = true;
            burnSetterSet = setter;
            burnDuration = duration;
            burnDamageEachSec = burnDamage / burnDurationCounter;
            burnDamageType = burnType;
            BonusIntakeDamagePercentage *= 1 + BonusIntakeDamage;*/
        }

        public virtual void SetStun(Character setter, float duration)
        {
            if (!ImmuneCc)
            {
                Stun stun = new(duration, setter, this);
                Board.AddEffect(stun);
                /*Stun = true;
                Stunner = setter;
                StunDuration = duration;*/
            }
        }

        public virtual void SetStun(Set setter, float duration)
        {
            if (!ImmuneCc)
            {
                Stun stun = new(duration, setter, this);
                Board.AddEffect(stun);
                /*Stun = true;
                stunnerSet = setter;
                StunDuration = duration;*/
            }
        }

        public virtual void SetBlind(Character setter, float duration)
        {
            Blind = true;
            _blindSetter = setter;
            BlindDuration = duration;
        }

        public virtual void SetBlind(Set setter, float duration)
        {
            Blind = true;
            _blindSetterSet = setter;
            BlindDuration = duration;
        }

        public virtual void SetChanneling(double duration, Character channelTarget = null)
        {
            Channeling = true;
            ChannelingDuration = duration;
            this.ChannelTarget = channelTarget;
        }
    }
}