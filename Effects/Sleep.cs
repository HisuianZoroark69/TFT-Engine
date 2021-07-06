using System;
using System.Collections.Generic;
using System.Linq;
using TFT_Engine.Components;

namespace TFT_Engine.Effects
{
    class Sleep : Effect
    {
        private double WakeupDamage;
        public Sleep(double Duration, Character effector, Character effected, double wakeupDamage) : base(Duration, effector, effected)
        {
            WakeupDamage = wakeupDamage;
        }

        public Sleep(double Duration, Set effector, Character effected, double wakeupDamage) : base(Duration, effector, effected)
        {
            WakeupDamage = wakeupDamage;
        }

        public override void OverrideOnTick()
        {
            if (Board.RoundLog.ContainsKey(Board.CurrentTick))
                foreach (RoundEvent re in new List<RoundEvent>(Board.RoundLog[Board.CurrentTick]))
                {
                    if (re.EventType == EventType.Hitted && re.Main == Effected)
                    {
                        //Apply wakeup damage and wake up when hit
                        if (Effector != null) Effected.OnHit(Effector, DamageType.Magic, false, false, WakeupDamage);
                        else Effected.OnHit(EffectorSet, DamageType.Magic, WakeupDamage);
                        Abort();
                    }
                }
            base.OverrideOnTick();
        }
    }
}
