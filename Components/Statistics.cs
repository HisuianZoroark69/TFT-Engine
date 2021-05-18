namespace TFT_Engine.Components
{
    public class Statistics
    {
        public int maxHp;
        int _hp;
        public int hp { 
            get {
                return _hp;
            } 
            set {
                if (value - _hp > 0) value = (int)(value - (value - _hp) * decreasedHealing);
                if (value > maxHp) _hp = maxHp;
                else _hp = value;
            } 
        }
        public int atk;
        public int def;
        public float specialAtkPercentage = 1;
        public int specialDef;
        double _attackSpeed;
        public double attackSpeed
        {
            get { return _attackSpeed; }
            set
            {
                if (value > attackSpeedCap) _attackSpeed = attackSpeedCap;
                else _attackSpeed = value;
            }
        }
        public int attackRange;
        public int maxMana;
        public int critRate;
        public int dodgeRate;
        public int shield;
        public float decreasedHealing;
        public double attackSpeedCap = 5;
        public float movingSpeed = 0.75f;
    }
}
