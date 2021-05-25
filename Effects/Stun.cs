using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFT_Engine.Components;

namespace TFT_Engine.Effects
{
    class Stun : Effect
    {
        public Stun(int Duration, Character effector, Character effected) : base(Duration, effector, effected)
        {
        }

        public Stun(int Duration, Set effector, Character effected) : base(Duration, effector, effected)
        {
        }
    }
}
