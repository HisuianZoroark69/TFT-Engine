using System;

namespace TFT_Engine.Components
{
    public class Position
    {
        public int x;
        public int y;
        public int z { get { return 0 - x - y; } set { y = 0 - value - x; } }
        public Position(Position p)
        {
            x = p.x;
            y = p.y;
        }
        public Position()
        {
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Position);
        }
        public static bool operator ==(Position p1, Position p2)
        {
            return p1.Equals(p2);
        }
        public static bool operator !=(Position p1, Position p2)
        {
            return !p1.Equals(p2);
        }
        public bool Equals(Position pos)
        {
            return pos.x == x && pos.y == y;
        }
    }
}
