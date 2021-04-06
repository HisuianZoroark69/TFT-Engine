using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TFT_Engine.Components
{
    class Board
    {
        /// <summary>
        /// Turn end flag
        /// </summary>
        bool endTurn;

        /// <summary>
        /// The timer of the board
        /// </summary>
        Timer timer = new() { AutoReset = true };

        /// <summary>
        /// Specify ticks per second
        /// </summary>
        float _TicksPerSecond;
        public float TicksPerSecond { 
            get { return _TicksPerSecond; }
            set {
                _TicksPerSecond = value;
                timer.Interval = 1000 / value;
            }
        }

        /// <summary>
        /// Trigger once on Start()
        /// </summary>
        public delegate void StartEventHandler();
        public event StartEventHandler StartEvent;

        /// <summary>
        /// Trigger every tick
        /// </summary>
        public delegate void TickEventHandler();
        public event TickEventHandler TickEvent;

        /// <summary>
        /// Specify the type of board
        /// </summary>
        public enum BoardType { RECTANGLE, HEXAGON };
        BoardType Shape;

        //public enum BoardDirections { FORWARD_LEFT, FORWARD, FORWARD_RIGHT, RIGHT, BACKWARD_RIGHT, BACKWARD, BACKWARD_LEFT, LEFT };
        /// <summary>
        /// Neighbor coordination
        /// </summary>
        readonly int[,] RectangleNeighbor = { { -1, 1 }, { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, -1 }, { -1, 0 } };
        readonly int[,] HexagonNeighbor = {{ 0, 1 },{ 1, 0 },{ 1, -1 },{ 0, -1 },{ -1, 0 },{ -1, 1 }};

        /// <summary>
        /// List of cells in the board
        /// </summary>
        //List<Cell> Cells = new();
        public Dictionary<(int, int), Cell> Cells = new();

        /// <summary>
        /// Initialize a new board
        /// </summary>
        /// <param name="type">The shape of the board</param>
        /// <param name="height">The height of the board</param>
        /// <param name="width">The width of the board</param>
        public Board(BoardType type, int width, int height)
        {
            /*Shape = type;
            timer.Elapsed += (object o,ElapsedEventArgs e) => { TickEvent?.Invoke(); };

            if(type == BoardType.RECTANGLE)
            {
                for(int x = 0;x < width; x++)
                {
                    for(int y = 0; y < height; y++)
                    {
                        Cells.Add((x, y), new Cell(x, y));
                    }
                }
                foreach(Cell cell in Cells.Values)
                {
                    TickEvent += cell.OnTick;
                    StartEvent += cell.OnStart;
                    cell.neighbor = new();
                    for(int i = 0; i < 8; i++)
                    {
                        try {
                            //cell.neighbor.Add(Cells.Find(c => c.x == cell.x - RectangleNeighbor[i, 0] && c.y == cell.y - RectangleNeighbor[i, 1]));
                            cell.neighbor.Add(Cells[(cell.x - RectangleNeighbor[i, 0], cell.y - RectangleNeighbor[i, 1])]);
                        }
                        catch (IndexOutOfRangeException) { }
                    }
                }
            }
            if(type == BoardType.HEXAGON)
            {
                for(int q = 0; q < width; q++)
                {
                    for(int r = 0 - (int)Math.Floor((decimal)q/2); r < height - Math.Floor((decimal)q / 2); r++)
                    {
                        Cells.Add((q, r), new Cell(q, r));
                    }
                }
                foreach (Cell cell in Cells.Values)
                {
                    TickEvent += cell.OnTick;
                    StartEvent += cell.OnStart;
                    cell.neighbor = new();
                    for (int i = 0; i < 6; i++)
                    {
                        try
                        {
                            //cell.neighbor.Add(Cells.Find(c => c.x == cell.x - HexagonNeighbor[i, 0] && c.y == cell.y - HexagonNeighbor[i, 1]));
                            cell.neighbor.Add(Cells[(cell.x - HeNeighbor[i, 0], cell.y - RectangleNeighbor[i, 1])]);
                        }
                        catch (KeyNotFoundException) { }
                    }
                }
            }
            foreach(Cell c in Cells.Values)
            {
                TickEvent += c.Character.OnTick;
            }*/
        }
        /// <summary>
        /// Start the turn
        /// </summary>
        public async void Start(float tickPerSec = 100)
        {
            
            TicksPerSecond = tickPerSec; //Set default ticks
            timer.Start();
            StartEvent?.Invoke();
            endTurn = true;
            await Task.Run(() => { while (endTurn){} });
            timer.Stop();
        }

        public void End()
        {
            endTurn = !endTurn;
        }
    }
}
