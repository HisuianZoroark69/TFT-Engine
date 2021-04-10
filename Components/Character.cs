using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFT_Engine.Components
{
    class Character
    {
        //Todo: Time attack and move according to ticks
        public Position position;
        byte attackCounter;
        public string Name;
        public Guid teamID;
        public Board board;
        Character AttackTarget;
        Statistics baseStats;
        public Statistics currentStats;
        bool Dead;

        public Character(string name, Guid teamId, Statistics baseStats)
        {
            Name = name;
            teamID = teamId;
            this.baseStats = baseStats;
        }
        public virtual void OnTick()
        {
            if (!Dead)
            {
                //Finding target
                if (AttackTarget == null)
                {
                    int minDistance = int.MaxValue;
                    foreach (Character c in board.Characters)
                    {
                        if (!c.Dead && c.teamID != teamID && board.Distance(c.position, position) < minDistance)
                        {
                            AttackTarget = c;
                        }
                    }
                }
                if (++attackCounter >= currentStats.attackSpeed && currentStats.attackSpeed > 0)
                {
                    Attack();
                    attackCounter = 0;
                }
            }
        }
        public virtual void OnStart()
        {
            Dead = false;
            attackCounter = 0;
            currentStats = baseStats;          
        }

        public virtual void Move()
        {

        }


        //Todo: add some stats then atk
        public virtual void Attack()
        {
            
            AttackTarget.OnHit(this);
        }
        public virtual void OnHit(Character attacker) 
        {
            currentStats.hp -= Math.Max(attacker.currentStats.atk - currentStats.def, 1);
            if(currentStats.hp <= 0)
            {
                attacker.AttackTarget = null;
                Dead = true;
                board.charCounter[teamID]--;
            }
        }

        //Todo: add mana and special moves

    }
}
