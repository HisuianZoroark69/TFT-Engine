using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
            if ((from x in Characters where x.position.Equals(pos) select x.position).Count() == 0)
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
        public List<Position> PathFinding(Position start, Position end)
        {
            Dictionary<Position, Position> cameFrom = new();
            Dictionary<Position, int> costSoFar = new();
            List<(int, Position)> frontier = new();
            frontier.Add((0, start));
            cameFrom.Add(start, start);
            costSoFar.Add(start, 0);
            while(frontier.Count > 0)
            {
                Position current = frontier.ElementAt(0).Item2;
                frontier.Remove(frontier.ElementAt(0));

                if (current.Equals(end))
                {
                    List<Position> ret = new();
                    //Make sure character dont step on each other
                    Position dummy = cameFrom[end];
                    while(dummy != start)
                    {
                        ret.Add(dummy);
                        dummy = cameFrom[dummy];
                    }
                    ret.Reverse();
                    return ret;
                }
                int[,] Neighbor;
                if (Shape == BoardType.HEXAGON) Neighbor = HexagonNeighbor;
                else Neighbor = RectangleNeighbor;

                for(int i = 0; i < 6; i++)
                {
                    var next = new Position(current.x + Neighbor[i, 0], current.y + Neighbor[i, 1]);
                    if ((from x in Characters where next.Equals(x.position) select x.position).Any())
                        continue;
                    int newCost = costSoFar[current] + 1;
                    if(!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        int priority = newCost + Distance(next, end);
                        frontier.Add((priority, next));
                        cameFrom[next] = current;
                    }
                }
                frontier = (from x in frontier orderby x.Item1 ascending select x).ToList();
            }
            return new();
        }
    }
}
