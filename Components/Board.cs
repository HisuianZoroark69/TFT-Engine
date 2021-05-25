using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Timer = System.Timers.Timer;

namespace TFT_Engine.Components
{
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
            RECTANGLE,
            HEXAGON
        }

        private readonly bool border;
        private readonly int height;
        private readonly int[,] HexagonNeighbor = {{0, 1}, {1, 0}, {1, -1}, {0, -1}, {-1, 0}, {-1, 1}};

        /// <summary>
        ///     Neighbor coordination
        /// </summary>
        private readonly int[,] RectangleNeighbor =
            {{-1, 1}, {0, 1}, {1, 1}, {1, 0}, {1, -1}, {0, -1}, {-1, -1}, {-1, 0}};

        private readonly BoardType Shape;

        /// <summary>
        ///     The timer of the board
        /// </summary>
        private readonly Timer timer = new() {AutoReset = true};

        /// <summary>
        ///     Board dimensions
        /// </summary>
        private readonly int width;

        /// <summary>
        ///     Specify ticks per second
        /// </summary>
        private float _TicksPerSecond;

        /// <summary>
        ///     List of cells in the board
        /// </summary>
        public CharList BaseCharacters = new();

        public CharList Characters;
        public CharacterStartDelegate CharacterStart;
        public Dictionary<Guid, int> charCounter;

        public int CurrentTick;
        public int defaultTicksPerSec = 20;
        public List<Position> positionList;
        public Dictionary<int, List<RoundEvent>> roundLog;

        public List<Set> sets;
        public List<Effect> effects = new();

        /// <summary>
        ///     Initialize a new board
        /// </summary>
        /// <param name="type">The shape of the board</param>
        /// <param name="height">The height of the board</param>
        /// <param name="width">The width of the board</param>
        public Board(BoardType type, int width, int height, bool border = true, params Set[] s)
        {
            timer.Elapsed += (o, e) => TickEvent?.Invoke();
            this.width = width;
            this.height = height;
            Shape = type;
            this.border = border;
            sets = new List<Set>(s);
            foreach (var set in sets)
            {
                SetStartEvent += set.OnStart;
                TickEvent += set.OnTick;
                EndEvent += set.OnEnd;
                set.board = this;
            }

            positionList = getPositionList().ToList();
        }

        public float TicksPerSecond
        {
            get => _TicksPerSecond;
            set
            {
                _TicksPerSecond = value;
                try
                {
                    timer.Interval = 1000 / value; //min 10
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
            effects.Clear();
            Characters = new CharList(BaseCharacters);
            charCounter = new Dictionary<Guid, int>();
            foreach (var c in Characters)
                try
                {
                    charCounter[c.teamID]++;
                }
                catch (KeyNotFoundException)
                {
                    charCounter.Add(c.teamID, 0);
                    charCounter[c.teamID]++;
                }
            foreach (var s in sets)
            {
                //Reset type counter
                s.characterWithType.Clear();

                //Add character to set
                s.characterWithType = (from x in Characters
                    where x.set.Contains(s)
                    group x by x.teamID).ToDictionary(g => g.Key, g => g.ToList());
            }

            if (charCounter.Keys.Count < 2) return charCounter.Keys.ToArray()[0];
            TicksPerSecond = tickPerSec; //Set default ticks
            CurrentTick = 0;
            roundLog = new Dictionary<int, List<RoundEvent>>();
            CharacterStart?.Invoke();
            SetStartEvent?.Invoke();
            StartEvent?.Invoke();
            while ((from x in charCounter where x.Value == 0 select x).Count() < charCounter.Keys.Count - 1)
            {
                CurrentTick++;
                TickEvent?.Invoke();
                foreach (Effect e in new List<Effect>(effects))
                {
                    e.OnTick();
                    if (e.DurationCounter == 0) 
                        effects.Remove(e);
                }
                //Thread.Sleep((int) (TicksPerSecond > 0 ? 1000 / TicksPerSecond : 0));
            }

            //await Task.Run(() => { while (!charCounter.Values.Contains(0)){} });
            timer.Stop();
            EndEvent.Invoke();
            return (from x in charCounter where x.Value != 0 select x.Key).ToList()[0];
        }

        public bool CheckBoundary(Position pos)
        {
            if (Shape == BoardType.HEXAGON &&
                (pos.x > width - 1 - pos.z || pos.x < -pos.z / 2 || pos.z > 0 || pos.z < -height + 1)) return true;
            if (Shape == BoardType.RECTANGLE &&
                (pos.x < 0 || pos.x > width - 1 || pos.y < 0 || pos.y > height - 1)) return true;

            return false;
        }

        public void AddCharacter(Position pos, Character character)
        {
            if (border && CheckBoundary(pos))
            {
                Console.WriteLine("There's already a character on this tile");
                return;
            }

            if (!(from x in BaseCharacters where x.basePosition == pos select x).Any())
            {
                character.board = this;
                character.basePosition = pos;
                BaseCharacters.Add(character);
                TickEvent += character.OnTick;
                CharacterStart += character.OnStart;
            }
            else
            {
                BaseCharacters.Remove(BaseCharacters.Single(c => c.position == pos));
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
            if (Shape == BoardType.HEXAGON)
                return (int) Math.Max(Math.Abs(pos1.x - pos2.x),
                    Math.Max(Math.Abs(pos1.y - pos2.y), Math.Abs(pos1.z - pos2.z)));
            if (Shape == BoardType.RECTANGLE) return (int) (Math.Abs(pos1.x - pos2.x) + Math.Abs(pos1.y - pos2.y));
            return 0;
        }

        public List<Position> PathFinding(Position start, Position end)
        {
            Dictionary<Position, Position> cameFrom = new();
            Dictionary<Position, int> costSoFar = new();
            List<(int, Position)> frontier = new();
            int[,] Neighbor;
            if (Shape == BoardType.HEXAGON) Neighbor = HexagonNeighbor;
            else Neighbor = RectangleNeighbor;
            frontier.Add((0, start));
            cameFrom.Add(start, start);
            costSoFar.Add(start, 0);
            while (frontier.Count > 0)
            {
                var current = frontier.ElementAt(0).Item2;
                frontier.Remove(frontier.ElementAt(0));

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
                    var t = from x in Characters where next == x.position && x.collision select x.position;
                    if (t.Any())
                    {
                        if (t.Contains(end))
                        {
                            frontier.Add((0, end));
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
                        frontier.Add((priority, next));
                        cameFrom[next] = current;
                    }
                }

                frontier = (from x in frontier orderby x.Item1 select x).ToList();
            }

            return new List<Position>();
        }

        public List<Character> GetAdjacentCharacter(Character c)
        {
            int[,] Neighbor;
            var current = c.position;
            if (Shape == BoardType.HEXAGON) Neighbor = HexagonNeighbor;
            else Neighbor = RectangleNeighbor;
            List<Character> ret = new();
            for (var i = 0; i < Neighbor.Length / 2; i++)
            {
                var next = new Position {x = current.x + Neighbor[i, 0], y = current.y + Neighbor[i, 1]};
                ret.AddRange(from x in Characters where next == x.position select x);
            }

            return ret;
        }

        public List<Character> GetAdjacentCharacter(Position current)
        {
            int[,] Neighbor;
            if (Shape == BoardType.HEXAGON) Neighbor = HexagonNeighbor;
            else Neighbor = RectangleNeighbor;
            List<Character> ret = new();
            for (var i = 0; i < Neighbor.Length / 2; i++)
            {
                var next = new Position {x = current.x + Neighbor[i, 0], y = current.y + Neighbor[i, 1]};
                ret.Add(Characters[next]);
            }

            return ret;
        }

        public List<Position> GetAdjacentPositions(Position current)
        {
            int[,] Neighbor;
            if (Shape == BoardType.HEXAGON) Neighbor = HexagonNeighbor;
            else Neighbor = RectangleNeighbor;
            List<Position> ret = new();
            for (var i = 0; i < Neighbor.Length / 2; i++)
            {
                var next = new Position {x = current.x + Neighbor[i, 0], y = current.y + Neighbor[i, 1]};
                if (border)
                    if (Shape == BoardType.HEXAGON &&
                        (next.x > width - 1 - Math.Round(next.z / 2f, MidpointRounding.AwayFromZero) ||
                         next.x < -next.z / 2 || next.z > 0 || next.z < -height + 1))
                        continue;
                    else if (Shape == BoardType.RECTANGLE &&
                             (next.x < 0 || next.x > width - 1 || next.y < 0 || next.y > height - 1)) continue;
                ret.Add(next);
            }

            return ret;
        }

        public List<Position> DrawLine(Position start, Position end,
            out Dictionary<Position, Position> affectedPositions)
        {
            HashSet<Position> ret = new();
            float N = Distance(start, end);
            start = start + new Position {x = 1e-6, y = 1e-6, z = 1e-6};
            end = end + new Position {x = 1e-6, y = 1e-6, z = 1e-6};
            Position temp1;
            for (var step = 0; step <= N; step++)
            {
                var t = N == 0 ? 0f : step / N;
                temp1 = new Position {x = 0, z = 0};
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
            float N = Distance(start, end);
            start = start + new Position {x = 1e-6, y = 1e-6, z = 1e-6};
            end = end + new Position {x = 1e-6, y = 1e-6, z = 1e-6};
            for (var step = 0; step <= N; step++)
            {
                var t = N == 0 ? 0f : step / N;
                Position temp1 = new() {x = 0, z = 0};
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
            float N = Distance(start, end);
            start -= new Position {x = 1e-6, y = 1e-6, z = 1e-6};
            end -= new Position {x = 1e-6, y = 1e-6, z = 1e-6};
            for (var step = 0; step <= N; step++)
            {
                var t = N == 0 ? 0f : step / N;
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
                x = Lerp(p1.x, p2.x, t),
                y = Lerp(p1.y, p2.y, t)
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

        private IEnumerable<Position> getPositionList()
        {
            if (Shape == BoardType.RECTANGLE)
                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    yield return new Position {x = x, y = y};
            else if (Shape == BoardType.HEXAGON)
                for (var z = 0; -z < height; z--)
                for (var x = (int) Math.Round(-z / 2f, MidpointRounding.AwayFromZero); x < width; x++)
                    yield return new Position {x = x, z = z};
        }

        public List<Position> GetRange(Position current, int radius)
        {
            List<Position> ret = new();
            for (var x = -radius; x <= radius; x++)
            for (var y = Math.Max(-radius, -x - radius); y <= Math.Min(radius, -x + radius); y++)
                ret.Add(new Position {x = x, y = y});
            return ret;
        }

        public void AddRoundEvent(RoundEvent e)
        {
            if (!roundLog.ContainsKey(CurrentTick))
                roundLog[CurrentTick] = new List<RoundEvent> {e};
            else roundLog[CurrentTick].Add(e);
        }

        public void AddEffect(Effect e)
        {
            e.board = this;
            effects.Add(e);
        }
    }
}