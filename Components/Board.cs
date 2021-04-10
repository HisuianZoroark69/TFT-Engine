using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TFT_Engine.Components
{
    class HexagonMap
    {

    }
    class Board
    {
        /// <summary>
        /// Board dimensions
        /// </summary>
        int width;
        int height;

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
        public List<Character> Characters = new();
        public List<Character> currentCharacters;
        public Dictionary<Guid, int> charCounter;

        /// <summary>
        /// Initialize a new board
        /// </summary>
        /// <param name="type">The shape of the board</param>
        /// <param name="height">The height of the board</param>
        /// <param name="width">The width of the board</param>
        public Board(BoardType type, int width, int height)
        {
            timer.Elapsed += (object o, ElapsedEventArgs e) => TickEvent?.Invoke();
            this.width = width;
            this.height = height;
            Shape = type;
        }

        /// <summary>
        /// Start the turn
        /// </summary>
        public void Start(float tickPerSec = 200)
        {
            currentCharacters = new(Characters);
            charCounter = new();
            foreach (Character c in Characters) {
                try { charCounter[c.teamID]++; }
                catch (KeyNotFoundException) { charCounter.Add(c.teamID, 0); charCounter[c.teamID]++; }                
            }
            TicksPerSecond = tickPerSec; //Set default ticks
            timer.Start();
            StartEvent?.Invoke();
            while (!charCounter.Values.Contains(0)) { }
            //await Task.Run(() => { while (!charCounter.Values.Contains(0)){} });
            timer.Stop();

        }

        public void AddCharacter(Position pos, Character character)
        {
            if ((from x in Characters where x.position.x == pos.x && x.position.y == pos.y select x.position).Count() == 0)
            {
                character.board = this;
                character.position = pos;
                Characters.Add(character);
                TickEvent += character.OnTick;
                StartEvent += character.OnStart;
            }
            else
            {
                Console.WriteLine("There's already a character in this cell.");
            }
        }

        public int Distance(Position pos1, Position pos2)
        {
            if (Shape == BoardType.HEXAGON)
            {
                int pos1z = 0 - pos1.x - pos1.y;
                int pos2z = 0 - pos2.x - pos2.y;
                return Math.Max(Math.Abs(pos1.x - pos2.x), Math.Max(Math.Abs(pos1.y - pos2.y), Math.Abs(pos1z - pos2z)));
            }
            else if (Shape == BoardType.RECTANGLE)
            {
                return Math.Abs(pos1.x - pos2.x) + Math.Abs(pos1.y - pos2.y);
            }
            else return 0;
        }
    }
}
