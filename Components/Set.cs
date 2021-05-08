using System;
using System.Collections.Generic;

namespace TFT_Engine.Components
{
    public class Set
    {
        public int id;
        public Dictionary<Guid,List<Character>> characterWithType = new();
        public Board board;

        public virtual void OnStart() { }
        public virtual void OnTick() { }
        public virtual void OnEnd() { }

    }
}
