namespace TFT_Engine.Components
{
    public class Statistics
    {
        private double _attackSpeed;
        private double _hp;
        public int atk;
        public int attackRange;
        public double attackSpeedCap = 5;
        public int critRate;
        public float decreasedHealing;
        public int def;
        public int dodgeRate;

        //These are for serialization only
        public double mana;
        public int maxHp;
        public int maxMana;
        public float movingSpeed = 0.75f;
        public double shield;
        public float specialAtkPercentage = 1;
        public int specialDef;

        public double hp
        {
            get => _hp;
            set
            {
                if (value - _hp > 0) value = (int) (value - (value - _hp) * decreasedHealing);
                if (value > maxHp) _hp = maxHp;
                else _hp = value;
            }
        }

        public double attackSpeed
        {
            get => _attackSpeed;
            set
            {
                if (value > attackSpeedCap) _attackSpeed = attackSpeedCap;
                else _attackSpeed = value;
            }
        }
    }
}