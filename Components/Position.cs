using System;

namespace TFT_Engine.Components
{
    public class Position
    {
        public double X;
        public double Y;

        public Position(Position p)
        {
            X = p.X;
            Y = p.Y;
        }

        public Position()
        {
        }

        public double Z
        {
            get => 0 - X - Y;
            set => Y = 0 - value - X;
        }

        public void Round()
        {
            X = Math.Round(X);
            Y = Math.Round(Y);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
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
            return pos.X == X && pos.Y == Y;
        }

        public static Position operator +(Position p1, Position p2)
        {
            return new()
            {
                X = p1.X + p2.X,
                Y = p1.Y + p2.Y
            };
        }

        public static Position operator -(Position p1, Position p2)
        {
            return new()
            {
                X = p1.X - p2.X,
                Y = p1.Y - p2.Y
            };
        }
    }
}