namespace RogueLit2.Classes {
    internal class Tile {
        internal Tile(int LightLevel, Texture Texture, Creature? Creature = null) {
            this.LightLevel = LightLevel;
            SetTexture(Texture);
            Seen = LightLevel > 0;
            SetCreature(Creature);
        }

        internal int LightLevel {
            get => _LightLevel;
            set {
                if (value != 0) Seen = true;
                _LightLevel = value;
            }
        }
        private int _LightLevel { get; set; }
        internal int RedLightLevel { get; set; }
        internal TileProperties Property { get; private set; }
        internal CreatureProperties? Creature { get; set; }
        internal void SetTexture(Texture t) {
            Property = PropertyGetter.TextureToTileProperties[t];
        }
        internal void SetCreature(Creature? c) {
            Creature = c != null ? PropertyGetter.CreatureToCreatureProperties[c ?? Classes.Creature.Player] : null;
        }
        internal bool Seen { get; set; }
    }

    internal enum Texture {
        Floor,
        Rock,
        Water,
        UnlitTorch,
        LitTorch
    }

    internal enum Creature {
        Player,
        FalseTorch,
        Chaser,
        FalseUnlitTorch,
        Ghost,
    }
    internal enum Hue {
        Default,
        Red,
    }
}
