using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TFT_Engine.Characters;

namespace TFT_Engine.Components
{
    class Cell
    {
        /// <summary>
        /// Use axial coordination system if use hexagon, use offset if use rectangle
        /// </summary>
        public int x;
        public int y;
        int z;

        /// <summary>
        /// List of cells neighbor
        /// </summary>
        public List<Cell> neighbor;

        public Character Character;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
            z = 0 - x - y;
            Character = new DummyChar();
        }
        public void OnTick()
        {
            Character.OnTick();
        }
        public void OnStart()
        {
            Character.OnStart();
        }
    }
}
