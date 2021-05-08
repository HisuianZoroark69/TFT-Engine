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
                if (value > maxHp) _hp = maxHp;
            } 
        }
        public int atk;
        public int def;
        public int specialAtkPercentage;
        public int specialDef;
        public double attackSpeed;
        public int attackRange;
        public int maxMana;
        public int critRate;
        public int dodgeRate;
        public int shield;
    }
}
