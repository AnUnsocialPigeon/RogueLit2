using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueLit2.Classes {
    internal class Tile {
        internal Tile(int LightLevel, Texture Texture) {
            this.LightLevel = LightLevel;
            this.Texture = Texture;
            Seen = false;
        }

        internal int LightLevel { 
            get => _LightLevel; 
            set {
                if (value != 0) Seen = true;
                _LightLevel = value;
            }
        }
        internal int _LightLevel { get; set; }
        internal Texture Texture;
        internal Creature? Creature;
        internal bool Seen;
    }


    internal enum Texture {
        Floor, 
        Wall,
        Water,
        UnlitTorch,
        LitTorch
    }

    internal enum Creature {
        Player
    }
}
