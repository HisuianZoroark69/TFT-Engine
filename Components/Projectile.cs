using System.Collections.Generic;

namespace TFT_Engine.Components
{
    public class Projectile
    {
        private readonly List<Position> route;
        private readonly float velocity;
        public Character creator;
        public Position currentPosition;
        public Dictionary<Position, Position> ExtraAffectedPositions;
        public Position position;
        private int routeCounter;
        private int velocityCounter;

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
                if (routeCounter >= route.Count)
                {
                    Destroy();
                    return;
                }
                currentPosition = route[routeCounter++];
                
                if (creator.board.Characters[currentPosition] != null)
                    creator.OnProjectileHit(creator.board.Characters[currentPosition]);
                if (ExtraAffectedPositions.ContainsKey(currentPosition) &&
                    creator.board.Characters[ExtraAffectedPositions[currentPosition]] != null)
                    creator.OnProjectileHit(creator.board.Characters[ExtraAffectedPositions[currentPosition]]);
            }
            else
            {
                velocityCounter--;
            }
        }
    }
}