using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFT_Engine.Components
{
    public class Projectile
    {
        public Character creator;
        public Position position;
        int velocityCounter;
        private float velocity;
        private List<Position> route;
        public Position currentPosition;
        public Dictionary<Position, Position> ExtraAffectedPositions;
        private int routeCounter;

        public Projectile(Character creator, Position from, Position to, float velocity = 0.2f)
        {
            this.creator = creator;
            routeCounter = 0;
            this.velocity = velocity;
            route = creator.board.GetLineAhead(from, to, out ExtraAffectedPositions);
            currentPosition = from;
            creator.board.TickEvent += OnTick;
        }

        public void Destroy()
        {
            creator.board.TickEvent -= OnTick;
        }
        public void OnTick()
        {
            if (velocityCounter == 0)
            {
                velocityCounter = (int) velocity * creator.board.defaultTicksPerSec;
                if (++routeCounter >= route.Count)
                {
                    Destroy();
                    return;
                }
                currentPosition = route[routeCounter];
                if(creator.board.Characters[currentPosition] != null)
                    creator.OnProjectileHit(creator.board.Characters[currentPosition]);
                if (ExtraAffectedPositions.ContainsKey(currentPosition) && 
                    creator.board.Characters[ExtraAffectedPositions[currentPosition]] != null)
                {
                    creator.OnProjectileHit(creator.board.Characters[ExtraAffectedPositions[currentPosition]]);
                }
            }
            else velocityCounter--;
        }
    }
}
