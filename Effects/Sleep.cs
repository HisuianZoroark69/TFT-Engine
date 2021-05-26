using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if(board.roundLog.ContainsKey(board.CurrentTick))
                foreach (RoundEvent re in new List<RoundEvent>(board.roundLog[board.CurrentTick]))
                {
                    if (re.eventType == EventType.Hitted && re.main == Effected)
                    {
                        //Apply wakeup damage and wake up when hit
                        if(Effector != null) Effected.OnHit(Effector,DamageType.Magic,false,false,WakeupDamage);
                        else Effected.OnHit(EffectorSet, DamageType.Magic, WakeupDamage);
                        Abort();
                    }
                }
            base.OverrideOnTick();
        }
    }
}
