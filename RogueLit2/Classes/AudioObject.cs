using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace RogueLit2.Classes {
    internal class AudioObject {
        public AudioObject(string name, DateTime startTime, int length) {
            Name = name;
            StartTime = startTime;
            Length = length;
        }

        internal bool IsPlaying => (DateTime.Now - StartTime).TotalSeconds <= Length;
        internal string Name { get; set; }
        internal DateTime StartTime { get; set; }
        internal int Length { get; set; }

    }
}
