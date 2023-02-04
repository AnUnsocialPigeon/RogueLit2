namespace RogueLit2.Classes {
    internal class Level {
        internal int AreaWidth, AreaHeight;
        internal Tile[] Tiles;
        internal int LitTorches = 0;
        internal int MaxTorches = 12;
        private Point[]? UnlitTorches;

        // Boundary around the area of which will be surrounding wall during generation.
        private readonly int boundary = 2;
        private readonly Random rnd = new();
        internal Player Player;

        internal Level(int width, int height, Player player) {
            AreaWidth = width;
            AreaHeight = height;
            Player = player;

            // Generation
            Tiles = new Tile[width * height];
            Tiles = Tiles.Select(x => x = new(0, Texture.Rock)).ToArray();

            LitTorches = 0;
            GenerateLevel();
        }


        #region LevelGeneration
        private void GenerateLevel() {
            GenerateChasm(rnd.Next(65, 70));
            PopulateTorches(MaxTorches);
            UnlitTorches = GetAllUnlitTorches(true);
        }


        private void GenerateChasm(int PercentFloor) {
            Tiles = Tiles.Select(x => { x.SetTexture(rnd.Next(100) <= PercentFloor ? Texture.Floor : Texture.Rock); return x; }).ToArray();
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
                if ((Tiles[i].Property.Texture == Texture.Rock && option == SmootheOption.Floor) ||
                    (Tiles[i].Property.Texture == Texture.Floor && option == SmootheOption.Wall)) continue;
                Point p = LongToPoint(i);
                Tile[] surrounding = GetTilesAround(p);

                if ((option == SmootheOption.Floor || option == SmootheOption.Both) && surrounding.Where(x => x.Property.Texture == Texture.Floor).Count() < 4) {
                    ChangeToWall.Add(i);
                    continue;
                }
                if ((option == SmootheOption.Wall || option == SmootheOption.Both) && surrounding.Where(x => x.Property.Texture == Texture.Rock).Count() < 3) {
                    ChangeToFloor.Add(i);
                }
            }
            foreach (int posLong in ChangeToWall)
                Tiles[posLong].SetTexture(Texture.Rock);
            foreach (int posLong in ChangeToFloor)
                Tiles[posLong].SetTexture(Texture.Floor);
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

        internal Tile?[] GetTilesAdjascent(Point p) {
            List<Tile?> tiles = new();
            tiles.Add(p.y - 1 < 0 ? null : Tiles[PointToLong(new(p.x, p.y - 1))]);
            tiles.Add(p.x - 1 < 0 ? null : Tiles[PointToLong(new(p.x - 1, p.y))]);
            tiles.Add(p.x + 1 >= AreaWidth ? null : Tiles[PointToLong(new(p.x + 1, p.y))]);
            tiles.Add(p.y + 1 >= AreaHeight ? null : Tiles[PointToLong(new(p.x, p.y + 1))]);

            return (tiles.ToArray());
        }

        private bool AttemptToFitRoom(int width, int height, Point topLeft) {
            // Wall checks
            if (topLeft.x > AreaWidth - 10 - boundary || topLeft.y > AreaHeight - 10 - boundary) return false;
            width = Math.Min(width, AreaWidth - topLeft.x - boundary);
            height = Math.Min(height, AreaHeight - topLeft.y - boundary);

            // Can fit?
            for (int x = topLeft.x; x < topLeft.x + width; x++) {
                for (int y = topLeft.y; y < topLeft.y + height; y++) {
                    if (GetTile(new(x, y)).Property.Texture != Texture.Rock) return false;
                }
            }

            // Place
            for (int x = topLeft.x; x < topLeft.x + width; x++) {
                for (int y = topLeft.y; y < topLeft.y + height; y++) {
                    GetTile(new(x, y)).Property.Texture = Texture.Floor;
                }
            }
            return true;
        }


        private void PopulateTorches(int amount) {
            for (int i = 0; i < amount; i++) {
                try {
                    Point pos = GetRandomFreeSpace();
                    Tiles[PointToLong(pos)].SetTexture(Texture.UnlitTorch);
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

        #region CalculteBrigthness
        internal void UpdateAllLightValue() {
            LightSource[] lightSources = GetAllLightSources(true);

            for (int i = 0; i < Tiles.Length; i++) {
                Point t = LongToPoint(i);
                Light l = GetMaxBrightness(lightSources, t);

                Tiles[i].RedLightLevel = l.Hue == Hue.Red ? l.Intensity : 0;
                Tiles[i].LightLevel = l.Hue == Hue.Default ? l.Intensity : 0;
            }
        }


        private Light GetMaxBrightness(LightSource[] lightSources, Point tile) {
            Light maxBrightness = new(0, Hue.Red);
            foreach (LightSource p in lightSources) {
                Tile lightSource = GetTile(p.Position);
                CreatureProperties? creature = lightSource.Creature;

                // Prioritising White.
                if ((lightSource.Property.Hue == Hue.Red || (creature != null && creature.Hue == Hue.Red)) && maxBrightness.Hue == Hue.Default) 
                    continue;


                int lightSourceLevel = Math.Max(lightSource.Property.LightEmittanceLevel,
                        creature == null ? 0 :
                        creature.LightEmmitance);

                // For dist = 0
                if (p.Position.x == tile.x && p.Position.y == tile.y) {
                    maxBrightness.Intensity = lightSourceLevel;

                    // Change hue to default if it is red
                    maxBrightness.Hue = maxBrightness.Hue == Hue.Default ? Hue.Default : p.Hue;
                }

                int distance = Math.Min((int)(lightSourceLevel / DistanceBetweenPoints(p.Position, tile)), 7);
                if ((p.Hue == Hue.Default && distance > 0 && maxBrightness.Hue == Hue.Red) || distance > maxBrightness.Intensity) {
                    maxBrightness.Intensity = distance;

                    // Change hue to default if it is red
                    maxBrightness.Hue = maxBrightness.Hue == Hue.Default ? Hue.Default : p.Hue;
                }

            }
            return maxBrightness;
        }

        internal double DistanceBetweenPoints(Point p, Point t) {
            double x = p.x - t.x;
            double y = p.y - t.y;
            return (double)Math.Sqrt((x * x) + (y * y));
        }

        // TODO: MAKE EFFICIENT
        internal LightSource[] GetAllLightSources(bool IncludeCreatures) {
            List<LightSource> lightSources = new();
            for (int i = 0; i < Tiles.Length; i++) {
                if (Tiles[i].Property.LightEmittanceLevel > 0) {
                    lightSources.Add(new(LongToPoint(i), Tiles[i].Property.Hue));
                    continue;
                }
                if (IncludeCreatures && Tiles[i].Creature != null && Tiles[i].Creature.LightEmmitance > 0)
                    lightSources.Add(new(LongToPoint(i), Tiles[i].Creature.Hue));

            }
            return lightSources.ToArray();
        }
        #endregion



        #region Utilities
        internal Point[] GetAllUnlitTorches(bool reset = false) {
            if (!reset && UnlitTorches != null)
                return UnlitTorches;

            List<Point> unlitTorches = new();
            for (int i = 0; i < Tiles.Length; i++) {
                if (Tiles[i].Property.Texture == Texture.UnlitTorch)
                    unlitTorches.Add(LongToPoint(i));
            }
            UnlitTorches = unlitTorches.ToArray();
            return UnlitTorches;
        }

        internal void LightTorch(Point p, bool debug = true) {
            if (Tiles[PointToLong(p)].Property.Texture != Texture.UnlitTorch) {
                if (debug)
                    throw new($"No unlit torch at ({p.x},{p.y})");
                return;
            }
            Tiles[PointToLong(p)].SetTexture(Texture.LitTorch);
            LitTorches++;
        }
        internal void UnlightTorch(Point p, bool debug = true) {
            if (Tiles[PointToLong(p)].Property.Texture != Texture.LitTorch) {
                if (debug)
                    throw new($"No lit torch at ({p.x},{p.y})");
                return;
            }
            Tiles[PointToLong(p)].SetTexture(Texture.UnlitTorch);
            LitTorches--;
        }

        internal Point GetRandomFreeSpace() {
            if (!Tiles.Any(x => x.Property.Texture == Texture.Floor)) throw new("No free spaces to place Player");

            Point p;
            do p = new(rnd.Next(AreaWidth), rnd.Next(AreaHeight));
            while (GetTile(p).Property.Texture != Texture.Floor);

            return p;
        }

        internal int DistanceToPlayer(Point p) => (int)DistanceBetweenPoints(p, Player.Position);

        /// <summary>
        /// Attempts to spawn a creature at a specific tile
        /// </summary>
        /// <param name="creature">The creature</param>
        /// <param name="position">The position</param>
        /// <param name="force">If you want to force a spawn. !!!DANGEROUS!!!</param>
        /// <returns>Null if successful, or the type of creature that exists there if the spawn was unsuccessful</returns>
        internal CreatureProperties? SpawnCreature(Creature creature, Point position, bool force = false) {
            if (!BoundCheck(position)) throw new($"Attempt to spawn {creature} outside of bounds");
            if (!force && Tiles[PointToLong(position)].Property.Texture != Texture.Floor) throw new($"There is no floor at ({position.x},{position.y}) to spawn creature {creature}");
            if (!force && Tiles[PointToLong(position)].Creature != null) return Tiles[PointToLong(position)].Creature;

            Tiles[PointToLong(position)].SetCreature(creature);
            return null;
        }
        internal void DeleteCreature(Point position) {
            Tiles[PointToLong(position)].SetCreature(null);
        }


        /// <summary>
        /// Moves a creature from posFrom to posTo
        /// </summary>
        /// <param name="posFrom">The position to move a creature from</param>
        /// <param name="posTo">The position to move a creature to</param>
        /// <param name="force">To force move a creature, regardless of if there is an obsticle</param>
        /// <returns>A bool indicating the success of whether the creature has been moved</returns>
        internal bool MoveCreature(Point posFrom, Point posTo, bool force = false) {
            if (!BoundCheck(posFrom) || !BoundCheck(posTo)) return false;
            if (Tiles[PointToLong(posFrom)].Creature == null) throw new($"No creature found at ({posFrom.x},{posFrom.y})");
            if (!force && Tiles[PointToLong(posTo)].Creature != null) return false;

            Tiles[PointToLong(posTo)].Creature = Tiles[PointToLong(posFrom)].Creature;
            Tiles[PointToLong(posFrom)].Creature = null;
            return true;
        }

        #endregion 

        /// <summary>
        /// BoundCheck for a point. Returns true if it succeeds the bound check
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool BoundCheck(Point p) => p.x < AreaWidth && p.y < AreaHeight && p.x >= 0 && p.y >= 0;
        public Tile? GetTile(Point point) => BoundCheck(point) ? Tiles[PointToLong(point)] : null;
        private int PointToLong(Point p) => (p.y * AreaWidth) + p.x;
        private Point LongToPoint(int p) => new(p % AreaWidth, p / AreaWidth);

    }
}
