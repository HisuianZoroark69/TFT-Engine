using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TFT_Engine.Components;

namespace TFT_Engine
{
    class Main
    {
        public void Run()
        {
            Board board = new(Board.BoardType.HEXAGON, 7, 8);
            Guid team1 = Guid.NewGuid();
            Guid team2 = Guid.NewGuid();
            board.TickEvent += () => OnTick(board);
            
            //board.Cells[(0, 0)].Character = new DummyChar(50);
            //board.Cells[(1, 0)].Character = new DummyChar(100);
            board.AddCharacter(new Position(8, 8), new Character("Gwen", team1, new Statistics() { hp = 50, atk = 10, def = 7, attackSpeed = 1 ,attackRange=1}));
            board.AddCharacter(new Position(1, 2), new Character("Yuumi", team2, new Statistics() { hp = 10, atk = 8, def = 11, attackSpeed = 0.75 ,attackRange=3}));
            board.AddCharacter(new Position(3, 2), new Character("Yuumi 2", team2, new Statistics() { hp = 10, atk = 8, def = 11, attackSpeed = 1.5 ,attackRange=3}));
            board.PathFinding(new Position(8, 8), new Position(1, 2));
            board.Start();
            //while (true) { }
            Console.WriteLine("Turn end.");
            Console.ReadLine();
        }
        void OnTick(Board board)
        {
            foreach(Character c in board.Characters)
            {
                Console.Write($"{c.Name} hp: {c.currentStats.hp} pos {c.position.x} {c.position.y} ");
            }
            Console.WriteLine();
        }
    }
}
