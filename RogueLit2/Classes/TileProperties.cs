namespace RogueLit2.Classes {
    internal class TileProperties {
        public TileProperties(Texture texture,
                        string icon,
                        int lightEmittanceLevel,
                        bool illuminateCreatures,
                        Hue hue,
                        bool walkThroughable,
                        bool flyThroughable,
                        bool mineable) {
            Texture = texture;
            Icon = icon;
            LightEmittanceLevel = lightEmittanceLevel;
            WalkThroughable = walkThroughable;
            Mineable = mineable;
            FlyThroughable = flyThroughable;
            IlluminateCreatures = illuminateCreatures;
            Hue = hue;
        }

        internal Texture Texture { get; set; }
        internal string Icon { get; set; }
        internal int LightEmittanceLevel { get; set; }
        internal bool WalkThroughable { get; set; }
        internal bool Mineable { get; set; }
        internal bool FlyThroughable { get; set; }
        internal bool IlluminateCreatures { get; set; }
        internal Hue Hue { get; set; }
    }
}
