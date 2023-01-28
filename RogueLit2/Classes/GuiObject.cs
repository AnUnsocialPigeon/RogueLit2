using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueLit2.Classes {
    internal class GuiObject {
        /// <summary>
        /// Stores information that can be used for writing to the screen
        /// </summary>
        /// <param name="icon">the character that will be written</param>
        /// <param name="hexColour">What colour the character needs to be written</param>
        public GuiObject(char icon, string hexColour) {
            this.icon = icon;
            this.hexColour = hexColour;
        }

        public GuiObject(GuiObject guiObject) {
            icon = guiObject.icon;
            hexColour = guiObject.hexColour;
        }

        internal char icon;
        internal string hexColour;

    }
}
