using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueLit2.Classes {
    internal class LightSource {
        internal LightSource(Point position, Hue hue) {
            Position = position;
            Hue = hue;
        }
        internal Point Position;
        internal Hue Hue;
    }
}
