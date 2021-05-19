using System;
using System.Collections.Generic;

namespace TFT_Engine.Components
{
    public class Set
    {
        public int id;
        public Dictionary<Guid,List<Character>> characterWithType = new();
        public Board board;
        public string Name;

        public Set(string Name)
        {
            this.Name = Name;
        }

        public virtual void OnStart() { }
        public virtual void OnTick() { }
        public virtual void OnEnd() { }
    }
}
