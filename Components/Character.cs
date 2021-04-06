using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFT_Engine.Components
{
    class Position
    {
        public int x { get; set; }
        public int y { get; set; }
        public Position(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }
    }
    abstract class Character
    {
        //Todo: Position
        Position Position;
        //Todo: Time attack and move according to ticks
        /// <summary>
        /// The number of ticks per attack
        /// </summary>
        protected int baseAttackSpeed;
        protected int currentAttackSpeed;
        byte attackCounter;

        Character AttackTarget;
        public Character(Position Pos)
        {
            Position = Pos;
        }
        public Character()
        {
            Position = new(0, 0);
        }
        public virtual void OnTick()
        {
            if(++attackCounter >= currentAttackSpeed && currentAttackSpeed > 0)
            {
                Attack();
                attackCounter = 0;
            }
        }
        public virtual void OnStart()
        {
            attackCounter = 0;
        }

        public virtual void Move(Cell cell)
        {

        }


        //Todo: add some stats then atk
        public virtual void Attack()
        {

        }

        //Todo: add mana and special moves

    }
}
