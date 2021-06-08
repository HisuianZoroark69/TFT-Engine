namespace TFT_Engine.Components
{
    public class Item
    {
        public Character Holder;
        public int id;
        public string name;

        public virtual void OnStart()
        {
        }

        public virtual void OnTick()
        {
        }

        public virtual void OnEnd()
        {
        }

        public virtual void OnSpecialMove()
        {
        }

        public virtual void OnHit(Character attacker, DamageType damageType, bool isSpecial, bool isCrit, double amount)
        {
        }

        public virtual void OnAttack(Character target)
        {
        }

        public virtual void OnManaChange(double ManaChangeAmount)
        {
        }

        public virtual double OnDamageCalculation(double damage)
        {
            return damage;
        }

        public virtual double OnMagicDamageCalculation(double damage)
        {
            return damage;
        }
    }
}