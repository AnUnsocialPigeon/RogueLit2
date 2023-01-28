using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueLit2.Classes {
    internal class Level {
        internal int AreaWidth, AreaHeight;
        internal Tile[] Tiles;

        // Boundary around the area of which will be wall during generation.
        private int boundary = 2;

        private Random rnd = new();

        internal Level(int width, int height) {
            AreaWidth = width;
            AreaHeight = height;

            // Generation
            Tiles = new Tile[width * height];
            Tiles = Tiles.Select(x => x = new(0, Texture.Wall)).ToArray();

            GenerateLevel();
            PopulateTorches(10);
        }


        #region LevelGeneration
        private void GenerateLevel() {
            GenerateChasm(rnd.Next(65, 70));
        }

        private void GenerateChasm(int PercentFloor) {
            Tiles = Tiles.Select(x => { x.Texture = rnd.Next(100) <= PercentFloor ? Texture.Floor : Texture.Wall; return x; }).ToArray();
            for (int i = 0; i < 5; i++)
                Smoothe(SmootheOption.Floor);

            for (int i = 0; i < 2; i++)
                Smoothe(SmootheOption.Wall);
        }

        private void GenerateRooms(int MaxRooms) {
            // Inspired by the Rogue level generation
            for (int x = 0; x < 10; x++) {
                int width = rnd.Next(15, 30);
                int height = rnd.Next(15, 30);
                if (AttemptToFitRoom(width, height, new(rnd.Next(boundary, AreaWidth), rnd.Next(boundary, AreaHeight)))) break;
            }
        }


        /// <summary>
        /// Basic Smoothing algorithm
        /// </summary>
        /// <param name="option">Floor = Will check the floors' neighbours, Wall = Will check the Wall's neighbours</param>
        private void Smoothe(SmootheOption option) {
            List<int> ChangeToWall = new();
            List<int> ChangeToFloor = new();

            for (int i = 0; i < Tiles.Length; i++) {
                if ((Tiles[i].Texture == Texture.Wall && option == SmootheOption.Floor) ||
                    (Tiles[i].Texture == Texture.Floor && option == SmootheOption.Wall)) continue;
                Point p = LongToPoint(i);
                Tile[] surrounding = GetTilesAround(p);

                if ((option == SmootheOption.Floor || option == SmootheOption.Both) && surrounding.Where(x => x.Texture == Texture.Floor).Count() < 4) {
                    ChangeToWall.Add(i);
                    continue;
                }
                if ((option == SmootheOption.Wall || option == SmootheOption.Both) && surrounding.Where(x => x.Texture == Texture.Wall).Count() < 3) {
                    ChangeToFloor.Add(i);
                }
            }
            foreach (int posLong in ChangeToWall)
                Tiles[posLong].Texture = Texture.Wall;
            foreach (int posLong in ChangeToFloor)
                Tiles[posLong].Texture = Texture.Floor;
        }

        private Tile[] GetTilesAround(Point p) {
            List<Tile> tiles = new();
            for (int y = -1; y < 2; y++) {
                for (int x = -1; x < 2; x++) {
                    if (y == 0 && x == 0 || p.x + x >= AreaWidth || p.y + y >= AreaHeight || p.x + x < 0 || p.y + y < 0) continue;
                    tiles.Add(Tiles[PointToLong(new(p.x + x, p.y + y))]);
                }
            }
            return tiles.ToArray();
        }

        private bool AttemptToFitRoom(int width, int height, Point topLeft) {
            // Wall checks
            if (topLeft.x > AreaWidth - 10 - boundary || topLeft.y > AreaHeight - 10 - boundary) return false;
            width = Math.Min(width, AreaWidth - topLeft.x - boundary);
            height = Math.Min(height, AreaHeight - topLeft.y - boundary);

            // Can fit?
            for (int x = topLeft.x; x < topLeft.x + width; x++) {
                for (int y = topLeft.y; y < topLeft.y + height; y++) {
                    if (GetTile(new(x, y)).Texture != Texture.Wall) return false;
                }
            }

            // Place
            for (int x = topLeft.x; x < topLeft.x + width; x++) {
                for (int y = topLeft.y; y < topLeft.y + height; y++) {
                    GetTile(new(x, y)).Texture = Texture.Floor;
                }
            }
            return true;
        }


        private void PopulateTorches(int amount) {
            for (int i = 0; i < amount; i++) {
                try {
                    Point pos = GetRandomFreeSpace();
                    Tiles[PointToLong(pos)].Texture = Texture.UnlitTorch;
                }
                catch { break; }
            }
        }


        private enum SmootheOption {
            Wall,
            Floor,
            Both
        }
        #endregion

        internal void UpdateAllLightValue() {
            Point[] lightSources = GetAllLightSources();

            for (int i = 0; i < Tiles.Length; i++) {
                Point t = LongToPoint(i);
                Tiles[i].LightLevel = GetMaxBrightness(lightSources, t);
            }
        }

        private int GetMaxBrightness(Point[] lightSources, Point tile) {
            int maxBrightness = 0;
            foreach (Point p in lightSources) {
                if (p.x == tile.x && p.y == tile.y) {
                    maxBrightness = 5;
                    break;
                }

                int distance = (int)(5d / DistanceBetweenPoints(p, tile));
                if (distance > maxBrightness)
                    maxBrightness = distance;
            
            }
            return maxBrightness;
        }

        private double DistanceBetweenPoints(Point p, Point t) => 
            (double)Math.Sqrt(Math.Pow(p.x - t.x, 2) + Math.Pow(p.y - t.y, 2));

        private Point[] GetAllLightSources() {
            List<Point> lightSources = new();
            for (int i = 0; i < Tiles.Length; i++) {
                if (Tiles[i].Texture == Texture.LitTorch || Tiles[i].Creature == Creature.Player)
                    lightSources.Add(LongToPoint(i));
            }
            return lightSources.ToArray();
        }

        internal Point GetRandomFreeSpace() {
            if (!Tiles.Any(x => x.Texture == Texture.Floor)) throw new("No free spaces to place Player");

            Point p;
            do p = new(rnd.Next(AreaWidth), rnd.Next(AreaHeight));
            while (GetTile(p).Texture != Texture.Floor);

            return p;
        }

        /// <summary>
        /// Attempts to spawn a creature at a specific tile
        /// </summary>
        /// <param name="creature">The creature</param>
        /// <param name="position">The position</param>
        /// <param name="force">If you want to force a spawn. !!!DANGEROUS!!!</param>
        /// <returns>Null if successful, or the type of creature that exists there if the spawn was unsuccessful</returns>
        internal Creature? SpawnCreature(Creature creature, Point position, bool force = false) {
            if (Tiles[PointToLong(position)].Texture != Texture.Floor) throw new($"There is no floor at ({position.x},{position.y}) to spawn creature {creature}");
            if (!force && Tiles[PointToLong(position)].Creature != null) return Tiles[PointToLong(position)].Creature;

            Tiles[PointToLong(position)].Creature = creature;
            return null;
        }

        /// <summary>
        /// Moves a creature from posFrom to posTo
        /// </summary>
        /// <param name="posFrom">The position to move a creature from</param>
        /// <param name="posTo">The position to move a creature to</param>
        /// <param name="force">To force move a creature, regardless of if there is an obsticle</param>
        /// <returns>A bool indicating the success of whether the creature has been moved</returns>
        internal bool MoveCreature(Point posFrom, Point posTo, bool force = false) {
            if (Tiles[PointToLong(posFrom)].Creature == null) throw new($"No creature found at ({posFrom.x},{posFrom.y})");
            if (!force && Tiles[PointToLong(posTo)].Creature != null) return false;

            Tiles[PointToLong(posTo)].Creature = Tiles[PointToLong(posFrom)].Creature;
            Tiles[PointToLong(posFrom)].Creature = null;
            return true;
        }


        public Tile GetTile(Point point) => Tiles[PointToLong(point)];
        private int PointToLong(Point p) => (p.y * AreaWidth) + p.x;
        private Point LongToPoint(int p) => new(p % AreaWidth, p / AreaWidth);

    }
}
