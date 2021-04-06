using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TFT_Engine.Characters;
using TFT_Engine.Components;

namespace TFT_Engine
{
    class Main
    {
        public void Run()
        {
            Board board = new(Board.BoardType.HEXAGON, 7, 8);
            //board.TickEvent += () => OnTick(board);
            board.Cells[(0, 0)].Character = new DummyChar(50);
            board.Cells[(1, 0)].Character = new DummyChar(100);
            board.Start();
            while (true) { }
            
            board.End();
        }
    }
}
