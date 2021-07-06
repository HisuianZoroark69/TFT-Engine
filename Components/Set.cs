using System;
using System.Collections.Generic;

namespace TFT_Engine.Components
{
    public class Set
    {
        public Board Board;
        public Dictionary<Guid, List<Character>> CharacterWithType = new();
        public int Id;
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