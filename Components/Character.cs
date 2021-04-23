using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TFT_Engine.Components
{
    class Character
    {
        //Todo: Time attack and move according to ticks
        public Position position;
        byte attackCounter;
        byte moveCounter;
        public string Name;
        public Guid teamID;
        public Board board;
        Character AttackTarget;
        Statistics baseStats;
        public Statistics currentStats;
        int ticksPerAttack { 
            get {
                try { return (int)(board.TicksPerSecond / currentStats.attackSpeed); }
                catch (DivideByZeroException) { return 0; }
            }
        }
        bool Dead;
        bool Moving;
        public Character(string name, Guid teamId, Statistics baseStats)
        {
            Name = name;
            teamID = teamId;
            this.baseStats = baseStats;
        }
        /// <summary>
        /// This runs every ticks
        /// </summary>
        public virtual void OnTick()
        {
            //Check if character is dead
            if (!Dead)
            {
                //Finding target
                if (AttackTarget == null)
                {
                    int minDistance = int.MaxValue;
                    foreach (Character c in board.Characters)
                    {
                        int distance = board.Distance(c.position, position);
                        if (!c.Dead && c.teamID != teamID && distance < minDistance)
                        {
                            AttackTarget = c;
                            minDistance = distance;
                        }
                    }                   
                }
                //Move to target if outside of attack range
                if (board.Distance(AttackTarget.position, position) > currentStats.attackRange && !Moving)
                {
                    moveCounter = 0;
                    Moving = true;
                    position = board.PathFinding(position, AttackTarget.position)[1];
                }
                if (Moving)
                {
                    moveCounter++;
                    if (moveCounter == 20) Moving = false;
                }
                //Attack
                if (++attackCounter >= ticksPerAttack && ticksPerAttack > 0 && !Moving)
                {
                    Attack();
                    attackCounter = 0;
                }
            }
        }
        public virtual void OnStart()
        {
            Dead = false;
            Moving = false;
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
