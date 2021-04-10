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
        public override int GetHashCode()
        {
            return int.Parse(x.ToString() + y.ToString());
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
