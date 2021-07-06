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
            route = creator.Board.GetLineAhead(from, to, out ExtraAffectedPositions);
            currentPosition = from;
            creator.Board.TickEvent += OnTick;
        }

        public void Destroy()
        {
            creator.Board.TickEvent -= OnTick;
        }

        public void OnTick()
        {
            if (velocityCounter == 0)
            {
                velocityCounter = (int)velocity * creator.Board.DefaultTicksPerSec;
                if (routeCounter >= route.Count)
                {
                    Destroy();
                    return;
                }
                currentPosition = route[routeCounter++];

                if (creator.Board.Characters[currentPosition] != null)
                    creator.OnProjectileHit(creator.Board.Characters[currentPosition]);
                if (ExtraAffectedPositions.ContainsKey(currentPosition) &&
                    creator.Board.Characters[ExtraAffectedPositions[currentPosition]] != null)
                    creator.OnProjectileHit(creator.Board.Characters[ExtraAffectedPositions[currentPosition]]);
            }
            else
            {
                velocityCounter--;
            }
        }
    }
}