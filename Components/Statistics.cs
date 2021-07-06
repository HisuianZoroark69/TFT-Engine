namespace TFT_Engine.Components
{
    public class Statistics
    {
        private double _attackSpeed;
        private double _hp;
        public int Atk;
        public int AttackRange;
        public double AttackSpeedCap = 5;
        public int CritRate;
        public float DecreasedHealing;
        public int Def;
        public int DodgeRate;

        //These are for serialization only
        public double Mana;
        public int MaxHp;
        public int MaxMana;
        public double MovingSpeed = 0.75;

        public double Shield;
        public double SpecialAtkPercentage = 1;
        public int SpecialDef;

        public double Hp
        {
            get => _hp;
            set
            {
                if (value - _hp > 0) value = (int)(value - (value - _hp) * DecreasedHealing);
                if (value > MaxHp) _hp = MaxHp;
                else _hp = value;
            }
        }

        public double AttackSpeed
        {
            get => _attackSpeed;
            set
            {
                if (value > AttackSpeedCap) _attackSpeed = AttackSpeedCap;
                else _attackSpeed = value;
            }
        }
    }
}