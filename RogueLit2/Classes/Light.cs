using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueLit2.Classes {
    internal class Light {
        internal Light(int intensity, Hue hue) {
            Intensity = intensity;
            Hue = hue;
        }
        internal int Intensity;
        internal Hue Hue;
    }
}
