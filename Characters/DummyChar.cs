using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFT_Engine.Components;

namespace TFT_Engine.Characters
{
    class DummyChar : Character
    {
        public DummyChar(int baseAttackSpeed):base()
        {
            this.baseAttackSpeed = baseAttackSpeed;
        }
        public DummyChar():base()
        {
            //baseAttackSpeed = 0;
        }
        public DummyChar(Position p) : base(p) {

        }
        public override void Attack()
        {
            base.Attack();
            Console.WriteLine("Dummy attacking");
        }
    }
}
