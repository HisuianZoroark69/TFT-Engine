using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFT_Engine.Components;

namespace TFT_Engine.Effects
{
    class Burn : Effect
    {
        private DamageType damageType;
        private double AmountPerTick;

        public Burn(double Duration, DamageType type, double amount, Character effector, Character effected = null) : base(
            Duration, effector, effected)
        {
            damageType = type;
            AmountPerTick = amount / Duration;
        }

        public Burn(double Duration, DamageType type, double amount, Set effector, Character effected = null) : base(
            Duration, effector, effected)
        {
            damageType = type;
            AmountPerTick = amount / Duration;
        }

        public override void OverrideOnTick()
        {
            if (Effector != null) Effected.OnHit(Effector, damageType, false, false, AmountPerTick);
            else Effected.OnHit(EffectorSet, damageType, AmountPerTick);
            base.OverrideOnTick();
        }
    }
}
