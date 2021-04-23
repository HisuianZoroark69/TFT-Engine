using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFT_Engine.Components
{
    class Position
    {
        public int x;
        public int y;
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Position(Position p)
        {
            x = p.x;
            y = p.y;
        }
        public override int GetHashCode()
        {
            unchecked // integer overflows are accepted here
            {
                int hashCode = 0;
                hashCode = (hashCode * 397) ^ x.GetHashCode();
                hashCode = (hashCode * 397) ^ y.GetHashCode();
                return hashCode;
            }
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Position);
        }
        public bool Equals(Position pos)
        {
            return pos.x == x && pos.y == y;
        }
    }
}
