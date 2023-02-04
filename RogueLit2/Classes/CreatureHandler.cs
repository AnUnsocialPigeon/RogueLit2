using RogueLit2.Classes.Controllers;
using System.Numerics;

namespace RogueLit2.Classes {
    internal class CreatureHandler {
        private GameMaster GameMaster { get; set; }
        internal double Currency { get; set; } = 7;
        internal int GameTickTime { get; set; }
        internal double ChanceForSpawnPerTick { get; set; }
        internal double UnitBucketSize { get; set; } = 45;
        internal double CurrencyPerTick { get; set; } = 1;
        internal double CreatureSpeedMultiplier { get; set; } = 1;

        private readonly Random rnd = new();

        private readonly List<CreatureObject> Creatures = new();

        private readonly Dictionary<Creature, int> Cost = new() {
            { Creature.FalseTorch, 10 },
            { Creature.FalseUnlitTorch, 10 },
            { Creature.Chaser, 13 },
            { Creature.Ghost, 15 }
        };
        private readonly Creature[] CreatureSelection = new Creature[] {
            Creature.FalseTorch,
            Creature.FalseTorch,
            Creature.FalseTorch,
            Creature.FalseTorch,
            Creature.FalseTorch,
            Creature.FalseUnlitTorch,
            Creature.FalseUnlitTorch,
            Creature.Chaser,
            Creature.Chaser,
            Creature.Ghost,
            Creature.Ghost,
            Creature.Ghost,
        };

        private DateTime LastGhostScream = DateTime.MinValue;


        public CreatureHandler(GameMaster GameMaster, int GameTickTime, int ChanceForSpawnPerTick) {
            this.GameMaster = GameMaster;
            this.GameTickTime = GameTickTime;
            this.ChanceForSpawnPerTick = ChanceForSpawnPerTick;
        }


        public Task Start() {
            if (GameMaster.CameraHandler == null) throw new("CameraHandler has not been set");
            Task t = new(() => {
                while (true) {
                    Thread.Sleep(GameTickTime);
                    HandleEnemySpawns();
                    Currency += CurrencyPerTick;
                }
            });
            t.Start();
            return t;
        }

        private void HandleEnemySpawns() {
            if (rnd.Next((int)(ChanceForSpawnPerTick * 100)) >= 100) return;

            try {
                Creature newCreature = CreatureSelection[rnd.Next(CreatureSelection.Length)];
                if (Currency < Cost[newCreature] || Creatures.Select(x => x.UnitSize).Sum() > UnitBucketSize) return;

                // Spawn in
                Point p = GameMaster.Level.GetRandomFreeSpace();
                if (GameMaster.Level.GetTile(p).LightLevel == 0)
                    SpawnCreature(p, newCreature);
                Currency -= Cost[newCreature];
            }
            catch {
                return;
            }
        }

        private void SpawnCreature(Point p, Creature newCreature) {
            if (GameMaster.Level.SpawnCreature(newCreature, p) != null) return;

            CreatureObject creature = GetCreatureObjectInformation(newCreature, p);

            if (creature.Property.Creature == Creature.Ghost)
                GameMaster.PlayAudio(SFX.Whoosh1);

            Creatures.Add(creature);
            Task t = Task.Run(() => {
                bool destroy = false;
                while (!destroy) {
                    try {
                        creature = HandleCreatureMovement(creature);
                        Thread.Sleep((int)(creature.Slowness / CreatureSpeedMultiplier));
                    }
                    catch (Exception e) {
                        destroy = true;
                    }
                }
            });
            creature.Handler = t;
        }

        private CreatureObject GetCreatureObjectInformation(Creature creature, Point position) {
            // Obtain creature properties

            if (creature == Creature.FalseUnlitTorch)
                return new(position, PropertyGetter.CreatureToCreatureProperties[creature],
                    Slowness: 500,
                    UnitSize: 3,
                    Aggression: Aggression.Trap,
                    PlayerViscinityThreshold: 6);
            if (creature == Creature.FalseTorch)
                return new(position, PropertyGetter.CreatureToCreatureProperties[creature],
                    Slowness: 500,
                    UnitSize: 3,
                    Aggression: Aggression.Trap,
                    PlayerViscinityThreshold: 6);

            if (creature == Creature.Chaser)
                return new(position, PropertyGetter.CreatureToCreatureProperties[creature],
                    Slowness: 350,
                    UnitSize: 2,
                    Aggression: Aggression.Aggressive,
                    PlayerViscinityThreshold: 8);
            if (creature == Creature.Ghost) {
                return new(position, PropertyGetter.CreatureToCreatureProperties[creature],
                    Slowness: 700,
                    UnitSize: 5,
                    Aggression: Aggression.Aggressive,
                    PlayerViscinityThreshold: 10000);
            }

            throw new Exception($"Creature {creature} does not exist");
        }


        /// <summary>
        /// Handles all craeture movement
        /// </summary>
        /// <param name="creature">the creature to move?</param>
        /// <returns>a new craeture object, incase some inherent properties have been changed</returns>
        private CreatureObject HandleCreatureMovement(CreatureObject creature) {
            int distance = GameMaster.Level.DistanceToPlayer(creature.Position);
            bool seePlayer = distance < creature.PlayerViscinityThreshold;

            if (creature.Aggression == Aggression.Trap && !seePlayer)
                return creature;

            // Fear of light
            if (creature.Aggression != Aggression.FearOfLight && GameMaster.Level.GetAllLightSources(false)
                        .Where(x => GameMaster.Level.DistanceBetweenPoints(x.Position, creature.Position) < 4).Any()) {
                creature.Aggression = Aggression.FearOfLight;
                GameMaster.PlayAudio(SFX.Whimper1);
            }

            if (creature.Aggression == Aggression.FearOfLight) {
                RunAway(creature);

                // Lose fear of light
                if (!GameMaster.Level.GetAllLightSources(false)
                        .Where(x => GameMaster.Level.DistanceBetweenPoints(x.Position, creature.Position) < 8)
                        .Any()) {
                    creature.Aggression = creature.Property.Creature == Creature.Ghost ? Aggression.Aggressive : Aggression.Roam;
                }
                return creature;
            }

            // Looses Aggression
            if (creature.Aggression == Aggression.Aggressive && !seePlayer) {
                creature.Aggression = Aggression.Roam;
                GameMaster.PlayAudio(SFX.LoseAggression1);
            }

            // Gain Aggression
            if (creature.Aggression == Aggression.Roam && seePlayer) {
                creature.Aggression = Aggression.Aggressive;
                GameMaster.PlayAudio(SFX.Aggression1);
            }

            // Roam 
            if (creature.Aggression == Aggression.Roam) {
                if (rnd.Next(4) == 0)
                    MoveRandomDirection(creature);
                return creature;
            }

            // Triggered a trap
            if (creature.Aggression == Aggression.Trap && seePlayer) {
                creature = GetCreatureObjectInformation(Creature.Chaser, creature.Position);
                GameMaster.Level.DeleteCreature(creature.Position);
                GameMaster.Level.SpawnCreature(creature.Property.Creature, creature.Position, true);

                if (!GameMaster.IsAudioPlaying(SFX.Aggression1))
                    GameMaster.PlayAudio(SFX.Aggression1);
            }

            // Ghost scream
            if (creature.Property.Creature == Creature.Ghost && distance < 7 && (DateTime.Now - LastGhostScream).TotalSeconds > 15) {
                GameMaster.PlayAudio(SFX.CreepyBreath1);
                LastGhostScream = DateTime.Now;
            }

            // Else, aggressive
            MoveTowardsPlayer(creature);
            return creature;
        }

        private void RunAway(CreatureObject creature) {
            bool[] availableTiles = GetAvailableTiles(creature);

            // Swaps the points to move in the opposite direction
            LightSource v = GameMaster.Level.GetAllLightSources(false)
                .OrderBy(x => GameMaster.Level.DistanceBetweenPoints(x.Position, creature.Position))
                .First();


            Point t = GetTileToMoveAwayTo(creature.Position, v.Position, availableTiles);

            if (BoundCheck(t) && BoundCheck(creature.Position) && t != creature.Position && (t.x != GameMaster.Player.Position.x || t.y != GameMaster.Player.Position.y) && GameMaster.Level.MoveCreature(creature.Position, t)) {
                creature.Position = t;
                if (GameMaster.CameraHandler.IsPointInView(creature.Position))
                    GameMaster.CameraHandler.QueueUpdate();
            }
        }


        /// <summary>
        /// Moves in random direction
        /// </summary>
        /// <param name="creature"></param>
        private void MoveRandomDirection(CreatureObject creature) {
            for (int i = 0; i < 3; i++) {
                bool m = rnd.Next(0, 2) == 0;
                bool n = rnd.Next(0, 2) == 0;
                Point moveTo = new(
                    creature.Position.x + ((m ? 0 : 1) * (n ? 1 : -1)),
                    creature.Position.y + ((m ? 1 : 0) * (n ? 1 : -1))
                );

                if (BoundCheck(moveTo) && BoundCheck(creature.Position) && CanCreatureStepOnTile(GameMaster.Level.GetTile(moveTo), creature) && GameMaster.Level.MoveCreature(creature.Position, moveTo)) {
                    creature.Position = moveTo;
                    GameMaster.CameraHandler.QueueUpdate();
                    return;
                }
            }
        }

        /// <summary>
        /// Moves towards the player if possible
        /// </summary>
        /// <param name="creature"></param>
        private void MoveTowardsPlayer(CreatureObject creature) {
            //Point difference = new(creature.Position.x - GameMaster.Player.Position.x, creature.Position.y - GameMaster.Player.Position.y);

            bool[] availableTiles = GetAvailableTiles(creature);

            Point t = GetTileToMoveTo(creature.Position, GameMaster.Player.Position, availableTiles);

            // Kill
            if (t.x == GameMaster.Player.Position.x && t.y == GameMaster.Player.Position.y) {
                GameMaster.PlayAudio(SFX.BodyFall1);
                GameMaster.PlayAudio(SFX.Grunt1);
                GameMaster.Reset();
                GameMaster.CameraHandler.QueueUpdate();
                return;
            }

            if (BoundCheck(t) && BoundCheck(creature.Position) && t != creature.Position && GameMaster.Level.MoveCreature(creature.Position, t)) {
                creature.Position = t;
                if (GameMaster.CameraHandler.IsPointInView(creature.Position))
                    GameMaster.CameraHandler.QueueUpdate();
            }
        }


        private bool[] GetAvailableTiles(CreatureObject creature) => 
            GameMaster.Level.GetTilesAdjascent(creature.Position).Select(x => x != null && CanCreatureStepOnTile(x, creature)).ToArray();

        public Point GetTileToMoveTo(Point creaturePoint, Point playerPoint, bool[] availableTiles) {
            int xDiff = playerPoint.x - creaturePoint.x;
            int yDiff = playerPoint.y - creaturePoint.y;
            List<Point> validMoves = new();

            // CBA TBH
            if (xDiff > 0 && availableTiles[2] == true)
                validMoves.Add(new(creaturePoint.x + 1, creaturePoint.y));
            if (xDiff < 0 && availableTiles[1] == true)
                validMoves.Add(new(creaturePoint.x - 1, creaturePoint.y));
            if (yDiff > 0 && availableTiles[3] == true)
                validMoves.Add(new(creaturePoint.x, creaturePoint.y + 1));
            if (yDiff < 0 && availableTiles[0] == true)
                validMoves.Add(new(creaturePoint.x, creaturePoint.y - 1));

            if (validMoves.Count == 0)
                return creaturePoint;

            return validMoves[rnd.Next(validMoves.Count)];
        }
        private Point GetTileToMoveAwayTo(Point creaturePoint, Point playerPoint, bool[] availableTiles) {
            int xDiff = playerPoint.x - creaturePoint.x;
            int yDiff = playerPoint.y - creaturePoint.y;
            List<Point> validMoves = new();

            // CBA TBH
            if (xDiff < 0 && availableTiles[2] == true)
                validMoves.Add(new(creaturePoint.x + 1, creaturePoint.y));
            if (xDiff > 0 && availableTiles[1] == true)
                validMoves.Add(new(creaturePoint.x - 1, creaturePoint.y));
            if (yDiff < 0 && availableTiles[3] == true)
                validMoves.Add(new(creaturePoint.x, creaturePoint.y + 1));
            if (yDiff > 0 && availableTiles[0] == true)
                validMoves.Add(new(creaturePoint.x, creaturePoint.y - 1));

            if (validMoves.Count == 0)
                return creaturePoint;

            return validMoves[rnd.Next(validMoves.Count)];
        }

        private bool BoundCheck(Point p) => p.y >= 0 || p.x >= 0 || p.y < GameMaster.Level.AreaHeight || p.x < GameMaster.Level.AreaWidth;

        private bool CanCreatureStepOnTile(Tile t, CreatureObject c, bool playerKill = true) =>
            t != null && (t.Creature == null || (playerKill && t.Creature.Creature == Creature.Player)) &&
            ((t.Property.WalkThroughable && c.Property.Walks) || (t.Property.FlyThroughable && c.Property.Flies) || c.Property.Etherial);


    }
}