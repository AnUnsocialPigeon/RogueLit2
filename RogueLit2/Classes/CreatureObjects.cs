namespace RogueLit2.Classes {
    internal class CreatureObject {
        public CreatureObject(Point Position,
                    CreatureProperties Properties,
                    int Slowness,
                    int UnitSize,
                    Aggression Aggression,
                    int PlayerViscinityThreshold) {
            this.Position = Position;
            Property = Properties;
            this.Slowness = Slowness;
            this.UnitSize = UnitSize;
            this.Aggression = Aggression;
            this.PlayerViscinityThreshold = PlayerViscinityThreshold;
        }
        public CreatureObject(Point position,
                    CreatureProperties property,
                    int slowness,
                    int unitSize,
                    Aggression aggression,
                    int playerViscinityThreshold,
                    Task handler) :
                this(position, property, slowness, unitSize, aggression, playerViscinityThreshold) {

            Handler = handler;
        }

        internal Point Position { get; set; }
        internal CreatureProperties Property { get; set; }

        /// <summary>
        /// = GameTicks between Movement Cycles. Large value = slow
        /// </summary>
        internal int Slowness { get; set; }
        internal int UnitSize { get; set; }
        internal Aggression Aggression { get; set; }
        internal int PlayerViscinityThreshold { get; set; }
        internal Task? Handler { get; set; }

    }

    public enum Aggression {
        Tame,
        Calm,
        Cautious,
        Afraid,
        Aggressive,
        Trap,
        Roam,
        FearOfLight,
    }
}
