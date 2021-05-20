using System;
using System.Collections.Generic;

namespace TFT_Engine.Components
{
    public class Set
    {
        public Board board;
        public Dictionary<Guid, List<Character>> characterWithType = new();
        public int id;
        public string Name;

        public Set(string Name)
        {
            this.Name = Name;
        }

        public virtual void OnStart()
        {
        }

        public virtual void OnTick()
        {
        }

        public virtual void OnEnd()
        {
        }
    }
}