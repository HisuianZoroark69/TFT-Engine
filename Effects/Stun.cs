using System;
using System.Linq;
using TFT_Engine.Components;

namespace TFT_Engine.Effects
{
    class Stun : Effect
    {
        public Stun(double Duration, Character effector, Character effected) : base(Duration, effector, effected)
        {
        }

        public Stun(double Duration, Set effector, Character effected) : base(Duration, effector, effected)
        {
        }
    }
}
