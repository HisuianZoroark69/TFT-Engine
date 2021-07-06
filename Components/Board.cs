using System;
using System.Collections.Generic;
using System.Linq;
using Timer = System.Timers.Timer;

namespace TFT_Engine.Components
{
    class Node<T>
    {
        public T value;
        public int priority;
        public Node<T> PrevNode;
        public Node<T> NextNode;
    }
    class PriorityQueue<T>
    {
        Node<T> head;
        public int Count
        {
            get
            {
                int c = 0;
                var ptr = head;
                while(ptr != null)
                {
                    c++;
                    ptr = ptr.NextNode;
                }
                return c;
            }
        }
        public Node<T> GetHead()
        {
            return head;
        }
        public void Add(T obj, int priority)
        {
            Node<T> ptr = head;
            if (head == null)
            {
                head = new Node<T> { value = obj, priority = priority };
                return;
            }
            do
            {
                if (ptr.priority > priority)
                {
                    var target = new Node<T> { value = obj, priority = priority};
                    target.NextNode = ptr;
                    ptr.PrevNode.NextNode = target;
                    target.PrevNode = ptr.PrevNode;
                }
                ptr = ptr.NextNode;
            } while (ptr != null);
        }
        public T Pop()
        {
            var tmp = head;
            head = head.NextNode;
            head.PrevNode = null;
            return tmp.value;
        }
        public T Peek()
        {
            return head.value;
        }
    }
    public class Board
    {
        public delegate void CharacterStartDelegate();

        public delegate void SetStartDelegate();

        public delegate void EndEventHandler();

        /// <summary>
        ///     Trigger once on Start()
        /// </summary>
        public delegate void StartEventHandler();

        /// <summary>
        ///     Trigger every tick
        /// </summary>
        public delegate void TickEventHandler();

        /// <summary>
        ///     Specify the type of board
        /// </summary>
        public enum BoardType
        {
            Rectangle,
            Hexagon
        }

        private readonly bool _border;
        private readonly int _height;
        private readonly int[,] _hexagonNeighbor = { { 0, 1 }, { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, 0 }, { -1, 1 } };

        /// <summary>
        ///     Neighbor coordination
        /// </summary>
        private readonly int[,] _rectangleNeighbor =
            {{-1, 1}, {0, 1}, {1, 1}, {1, 0}, {1, -1}, {0, -1}, {-1, -1}, {-1, 0}};

        private readonly BoardType _shape;

        /// <summary>
        ///     The timer of the board
        /// </summary>
        private readonly Timer _timer = new() { AutoReset = true };

        /// <summary>
        ///     Board dimensions
        /// </summary>
        private readonly int _width;

        /// <summary>
        ///     Specify ticks per second
        /// </summary>
        private float _ticksPerSecond;

        /// <summary>
        ///     List of cells in the board
        /// </summary>
        public CharList BaseCharacters = new();

        public CharList Characters;
        public CharacterStartDelegate CharacterStart;
        public Dictionary<Guid, int> CharCounter;

        public int CurrentTick;
        public int DefaultTicksPerSec = 20;
        public List<Position> PositionList;
        public RoundEventDictionary RoundLog;

        public List<Set> Sets;
        public List<Effect> Effects = new();

        /// <summary>
        /// The size of the cell
        /// </summary>
        public int TileSize;

        public int CharacterSize;

        /// <summary>
        ///     Initialize a new board
        /// </summary>
        /// <param name="type">The shape of the board</param>
        /// <param name="height">The height of the board</param>
        /// <param name="width">The width of the board</param>
        public Board(BoardType type, int width, int height, int tileSize = 100, int characterSize = 60, bool border = true, params Set[] s)
        {
            _timer.Elapsed += (o, e) => TickEvent?.Invoke();
            this._width = width;
            this._height = height;
            _shape = type;
            this._border = border;
            Sets = new List<Set>(s);
            foreach (var set in Sets)
            {
                SetStartEvent += set.OnStart;
                TickEvent += set.OnTick;
                EndEvent += set.OnEnd;
                set.Board = this;
            }


            TileSize = tileSize;
            CharacterSize = characterSize;
            PositionList = GetPositionList().ToList();
        }

        public float TicksPerSecond
        {
            get => _ticksPerSecond;
            set
            {
                _ticksPerSecond = value;
                try
                {
                    _timer.Interval = 1000 / value; //min 10
                    //timer.Enabled = true;
                }
                catch (Exception)
                {
                    //timer.Enabled = false;
                }

                ;
            }
        }

        public event StartEventHandler StartEvent;
        public event TickEventHandler TickEvent;
        public event EndEventHandler EndEvent;
        public event SetStartDelegate SetStartEvent;

        /// <summary>
        ///     Start the turn
        /// </summary>
        public Guid Start(float tickPerSec = 20)
        {
            Effects.Clear();
            Characters = new CharList(BaseCharacters);
            CharCounter = new Dictionary<Guid, int>();
            foreach (var c in Characters)
                try
                {
                    CharCounter[c.TeamId]++;
                }
                catch (KeyNotFoundException)
                {
                    CharCounter.Add(c.TeamId, 0);
                    CharCounter[c.TeamId]++;
                }
            foreach (var s in Sets)
            {
                //Reset type counter
                s.CharacterWithType.Clear();

                //Add character to set
                s.CharacterWithType = (from x in Characters
                                       where x.Set.Contains(s)
                                       group x by x.TeamId).ToDictionary(g => g.Key, g => g.ToList());
            }

            if (CharCounter.Keys.Count < 2) return CharCounter.Keys.ToArray()[0];
            TicksPerSecond = tickPerSec; //Set default ticks
            CurrentTick = 0;
            RoundLog = new();
            CharacterStart?.Invoke();
            SetStartEvent?.Invoke();
            StartEvent?.Invoke();
            while ((from x in CharCounter where x.Value == 0 select x).Count() < CharCounter.Keys.Count - 1)
            {
                CurrentTick++;
                TickEvent?.Invoke();
                foreach (Effect e in new List<Effect>(Effects))
                {
                    e.OnTick();
                    if (e.DurationCounter == 0)
                        Effects.Remove(e);
                }
                //Thread.Sleep((int) (TicksPerSecond > 0 ? 1000 / TicksPerSecond : 0));
            }

            //await Task.Run(() => { while (!charCounter.Values.Contains(0)){} });
            _timer.Stop();
            EndEvent.Invoke();
            return (from x in CharCounter where x.Value != 0 select x.Key).ToList()[0];
        }

        public bool CheckBoundary(Position pos)
        {
            if (_shape == BoardType.Hexagon &&
                (pos.X > _width - 1 - pos.Z || pos.X < -pos.Z / 2 || pos.Z > 0 || pos.Z < -_height + 1)) return true;
            if (_shape == BoardType.Rectangle &&
                (pos.X < 0 || pos.X > _width - 1 || pos.Y < 0 || pos.Y > _height - 1)) return true;

            return false;
        }

        public void AddCharacter(Position pos, Character character)
        {
            if (_border && CheckBoundary(pos))
            {
                Console.WriteLine("Out side border");
                return;
            }

            if (!(from x in BaseCharacters where x.BasePosition == pos select x).Any())
            {
                character.Board = this;
                character.BasePosition = pos;
                BaseCharacters.Add(character);
                TickEvent += character.OnTick;
                CharacterStart += character.OnStart;
            }
            else
            {
                BaseCharacters.Remove(BaseCharacters.Single(c => c.Position == pos));
            }
        }

        public void RemoveCharacter(Character character)
        {
            BaseCharacters.Remove(character);
            TickEvent -= character.OnTick;
            CharacterStart -= character.OnStart;
        }

        public int Distance(Position pos1, Position pos2)
        {
            if (_shape == BoardType.Hexagon)
                return (int)Math.Max(Math.Abs(pos1.X - pos2.X),
                    Math.Max(Math.Abs(pos1.Y - pos2.Y), Math.Abs(pos1.Z - pos2.Z)));
            if (_shape == BoardType.Rectangle) return (int)(Math.Abs(pos1.X - pos2.X) + Math.Abs(pos1.Y - pos2.Y));
            return 0;
        }

        public List<Position> PathFinding(Position start, Position end)
        {
            Dictionary<Position, Position> cameFrom = new();
            Dictionary<Position, int> costSoFar = new();
            PriorityQueue<Position> frontier = new();
            int[,] neighbor;
            if (_shape == BoardType.Hexagon) neighbor = _hexagonNeighbor;
            else neighbor = _rectangleNeighbor;
            frontier.Add(start,0);
            cameFrom.Add(start, start);
            costSoFar.Add(start, 0);
            while (frontier.Count > 0)
            {
                var current = frontier.Pop();

                if (current.Equals(end))
                {
                    List<Position> ret = new();
                    ret.Add(end);
                    //Make sure character dont step on each other
                    var dummy = cameFrom[end];
                    //Position dummy = end;
                    while (dummy != start)
                    {
                        ret.Add(dummy);
                        dummy = cameFrom[dummy];
                    }

                    ret.Add(start);
                    ret.Reverse();
                    return ret;
                }

                //for(int i = 0; i < Neighbor.Length / 2;i++)
                foreach (var next in GetAdjacentPositions(current))
                {
                    //var next = new Position { x = current.x + Neighbor[i, 0], y = current.y + Neighbor[i, 1] };
                    var t = from x in Characters where next == x.Position && x.Collision select x.Position;
                    if (t.Any())
                    {
                        if (t.Contains(end))
                        {
                            frontier.Add(end,0);
                            cameFrom[end] = current;
                            break;
                        }

                        continue;
                    }

                    /*if (border)
                        if(Shape == BoardType.RECTANGLE)
                        {
                            if (next.x > width - 1 || next.x < 0 || next.y < 0 || next.y > height - 1)
                                continue;
                        }
                        else if(Shape == BoardType.HEXAGON)
                        {
                            if (next.x - next.y < 0 || next.x > width - 1 || -next.z < 0 || -next.z > height - 1)
                                continue;
                        }*/
                    var newCost = costSoFar[current] + 1;
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        var priority = newCost + Distance(next, end);
                        frontier.Add(next,priority);
                        cameFrom[next] = current;
                    }
                }

                //frontier = (from x in frontier orderby x.Item1 select x).ToList();
            }

            return new List<Position>();
        }

        public List<Character> GetAdjacentCharacter(Character c)
        {
            int[,] neighbor;
            var current = c.Position;
            if (_shape == BoardType.Hexagon) neighbor = _hexagonNeighbor;
            else neighbor = _rectangleNeighbor;
            List<Character> ret = new();
            for (var i = 0; i < neighbor.Length / 2; i++)
            {
                var next = new Position { X = current.X + neighbor[i, 0], Y = current.Y + neighbor[i, 1] };
                ret.AddRange(from x in Characters where next == x.Position select x);
            }

            return ret;
        }

        public List<Character> GetAdjacentCharacter(Position current)
        {
            int[,] neighbor;
            if (_shape == BoardType.Hexagon) neighbor = _hexagonNeighbor;
            else neighbor = _rectangleNeighbor;
            List<Character> ret = new();
            for (var i = 0; i < neighbor.Length / 2; i++)
            {
                var next = new Position { X = current.X + neighbor[i, 0], Y = current.Y + neighbor[i, 1] };
                if (Characters[next] != null) ret.Add(Characters[next]);
            }

            return ret;
        }

        public List<Position> GetAdjacentPositions(Position current)
        {
            int[,] neighbor;
            if (_shape == BoardType.Hexagon) neighbor = _hexagonNeighbor;
            else neighbor = _rectangleNeighbor;
            List<Position> ret = new();
            for (var i = 0; i < neighbor.Length / 2; i++)
            {
                var next = new Position { X = current.X + neighbor[i, 0], Y = current.Y + neighbor[i, 1] };
                if (_border)
                    if (_shape == BoardType.Hexagon &&
                        (next.X > _width - 1 - Math.Round(next.Z / 2f, MidpointRounding.AwayFromZero) ||
                         next.X < -next.Z / 2 || next.Z > 0 || next.Z < -_height + 1))
                        continue;
                    else if (_shape == BoardType.Rectangle &&
                             (next.X < 0 || next.X > _width - 1 || next.Y < 0 || next.Y > _height - 1)) continue;
                ret.Add(next);
            }

            return ret;
        }

        public List<Position> DrawLine(Position start, Position end,
            out Dictionary<Position, Position> affectedPositions)
        {
            HashSet<Position> ret = new();
            float n = Distance(start, end);
            start = start + new Position { X = 1e-6, Y = 1e-6, Z = 1e-6 };
            end = end + new Position { X = 1e-6, Y = 1e-6, Z = 1e-6 };
            Position temp1;
            for (var step = 0; step <= n; step++)
            {
                var t = n == 0 ? 0f : step / n;
                temp1 = new Position { X = 0, Z = 0 };
                temp1 = LerpPoint(start, end, t);
                temp1.Round();
                ret.Add(temp1);
            }

            affectedPositions = ExtraAffectedLine(start, end);
            return ret.ToList();
        }

        public List<Position> DrawLine(Position start, Position end, bool extraAffectedPos = false)
        {
            HashSet<Position> ret = new();
            float n = Distance(start, end);
            start = start + new Position { X = 1e-6, Y = 1e-6, Z = 1e-6 };
            end = end + new Position { X = 1e-6, Y = 1e-6, Z = 1e-6 };
            for (var step = 0; step <= n; step++)
            {
                var t = n == 0 ? 0f : step / n;
                Position temp1 = new() { X = 0, Z = 0 };
                temp1 = LerpPoint(start, end, t);
                temp1.Round();
                ret.Add(temp1);
            }

            if (extraAffectedPos) ExtraAffectedLine(start, end).Values.ToList().ForEach(x => ret.Add(x));
            return ret.ToList();
        }

        private Dictionary<Position, Position> ExtraAffectedLine(Position start, Position end)
        {
            var baseLine = DrawLine(start, end);
            Dictionary<Position, Position> ret = new();
            float n = Distance(start, end);
            start -= new Position { X = 1e-6, Y = 1e-6, Z = 1e-6 };
            end -= new Position { X = 1e-6, Y = 1e-6, Z = 1e-6 };
            for (var step = 0; step <= n; step++)
            {
                var t = n == 0 ? 0f : step / n;
                Position temp1;
                temp1 = LerpPoint(start, end, t);
                temp1.Round();
                if (temp1 != baseLine[step]) ret.Add(baseLine[step], temp1);
            }

            return ret;
        }

        private Position LerpPoint(Position p1, Position p2, double t)
        {
            return new()
            {
                X = Lerp(p1.X, p2.X, t),
                Y = Lerp(p1.Y, p2.Y, t)
            };
        }

        private double Lerp(double a, double b, double t)
        {
            //var fuck = a + t * (b - a);
            return a + t * (b - a);
        }

        public List<Position> GetLineAhead(Position start, Position direction,
            int length = 0, bool extraAffectedPosition = false)
        {
            List<Position> ret = new();
            ret.AddRange(DrawLine(start, direction));
            var increase = 1d / Distance(start, direction);
            var counter = 1;
            var current = ret[^1];
            while (!CheckBoundary(current) && (length <= 0 || ret.Count <= length + 1))
            {
                current = LerpPoint(start, direction, 1 + increase * counter);
                current.Round();
                ret.Add(current);
                counter++;
            }

            //current = LerpPoint(start, direction, 1 + increase * (counter - 1));
            //current.Round();
            ret = DrawLine(start, current, extraAffectedPosition);

            if (length == 0) return ret.GetRange(1, ret.Count - 1);
            return ret.GetRange(1, length);
        }

        public List<Position> GetLineAhead(Position start, Position direction,
            out Dictionary<Position, Position> extraAffectedPositions, int length = 0)
        {
            List<Position> ret = new();
            ret.AddRange(DrawLine(start, direction));
            var increase = 1d / Distance(start, direction);
            var counter = 1;
            var current = ret[^1];
            while (!CheckBoundary(current) && (length <= 0 || ret.Count <= length + 1))
            {
                current = LerpPoint(start, direction, 1 + increase * counter);
                current.Round();
                ret.Add(current);
                counter++;
            }

            //current = LerpPoint(start, direction, 1 + increase * (counter - 1));
            //current.Round();
            extraAffectedPositions = ExtraAffectedLine(start, current);
            ret = DrawLine(start, current);

            if (length == 0) return ret.GetRange(1, ret.Count - 1);
            return ret.GetRange(1, length);
        }

        private IEnumerable<Position> GetPositionList()
        {
            if (_shape == BoardType.Rectangle)
                for (var x = 0; x < _width; x++)
                    for (var y = 0; y < _height; y++)
                        yield return new Position { X = x, Y = y };
            else if (_shape == BoardType.Hexagon)
                for (var z = 0; -z < _height; z--)
                    for (var x = (int)Math.Round(-z / 2f, MidpointRounding.AwayFromZero); x < _width; x++)
                        yield return new Position { X = x, Z = z };
        }

        public List<Position> GetRange(Position current, int radius)
        {
            List<Position> ret = new();
            for (var x = -radius; x <= radius; x++)
                for (var y = Math.Max(-radius, -x - radius); y <= Math.Min(radius, -x + radius); y++)
                    ret.Add(new Position { X = x, Y = y });
            return ret;
        }

        public void AddRoundEvent(RoundEvent e)
        {
            if (!RoundLog.ContainsKey(CurrentTick))
                RoundLog[CurrentTick] = new List<RoundEvent> { e };
            else RoundLog[CurrentTick].Add(e);
        }

        public void AddEffect(Effect e)
        {
            if (e.MaxStack > 0 &&
                (from x in Effects where x.GetType() == e.GetType() select x).Count() > e.MaxStack) return;
            e.Board = this;
            Effects.Add(e);
        }
    }
}