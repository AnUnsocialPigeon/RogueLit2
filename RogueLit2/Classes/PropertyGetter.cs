using RogueLit2.Classes.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueLit2.Classes {
    /// <summary>
    /// Yuck
    /// </summary>
    internal class PropertyGetter {
        /// <summary>
        /// To associate the Texture enum to the properties that should be associated with that texture
        /// </summary>
        internal static readonly Dictionary<Texture, TileProperties> TextureToTileProperties = new() {
            { Texture.Floor, new(Texture.Floor, "░░", 0, false, Hue.Default, true, true, false) },
            { Texture.Rock, new(Texture.Rock, "██", 0, false, Hue.Default, false, false, true) },
            { Texture.Water, new(Texture.Water, "▓▓", 0, false, Hue.Default, false, true, false) },
            { Texture.UnlitTorch, new(Texture.UnlitTorch, "┌┐", 3, false, Hue.Red, true, true, false) },
            { Texture.LitTorch, new(Texture.LitTorch, "╔╗", 7, true, Hue.Default,true, true, false) },
        };
        /// <summary>
        /// To relate the Creature enum to the properties that should be associated with that Creature
        /// </summary>
        internal static readonly Dictionary<Creature, CreatureProperties> CreatureToCreatureProperties = new() {
            { Creature.Player, new(Creature.Player, "@@", 5, true, false, false, 6, Hue.Default) },
            { Creature.FalseUnlitTorch, new(Creature.FalseUnlitTorch, "┌┐", 1, true, false, false, 2, Hue.Red) },
            { Creature.FalseTorch, new(Creature.FalseTorch, "╔╗", 1, true, false, false, 6, Hue.Default) },
            { Creature.Chaser, new(Creature.Chaser, "##", 1, true, false, false, 0, Hue.Red) },
            { Creature.Ghost, new(Creature.Ghost, "{}", 1, true, true, true, 5, Hue.Red) },
        };
    }
}
