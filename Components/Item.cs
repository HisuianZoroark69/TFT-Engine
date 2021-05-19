using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TFT_Engine.Components.Character;

namespace TFT_Engine.Components
{
    public class Item
    {
        public Character Holder;
        public string name;
        public int id;
        public virtual void OnStart() { }
        public virtual void OnTick() { }
        public virtual void OnEnd() { }
        public virtual void OnSpecialMove() { }
        public virtual void OnHit(Character attacker, DamageType damageType, bool isSpecial, bool isCrit, double amount) { }
        public virtual void OnAttack(Character target) { }
        public virtual void OnManaChange(double ManaChangeAmount) { }
        public virtual double OnDamageCalculation(double damage) { return damage; }
        public virtual double OnMagicDamageCalculation(double damage) { return damage; }
    }
}
