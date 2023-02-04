namespace RogueLit2.Classes {
    internal class CreatureProperties {
        internal CreatureProperties(Creature creature,
                        string icon,
                        int hp,
                        bool walks,
                        bool flies,
                        bool etherial,
                        int lightEmmitance,
                        Hue hue) {
            Creature = creature;
            Icon = icon;
            HP = hp;
            Walks = walks;
            Flies = flies;
            Etherial = etherial;
            LightEmmitance = lightEmmitance;
            Hue = hue;
        }

        internal Creature Creature { get; set; }
        internal string Icon { get; set; }
        internal int HP { get; set; }
        internal bool Walks { get; set; }
        internal bool Flies { get; set; }
        internal bool Etherial { get; set; }
        internal int LightEmmitance { get; set; }
        internal Hue Hue { get; set; }
    }
}
