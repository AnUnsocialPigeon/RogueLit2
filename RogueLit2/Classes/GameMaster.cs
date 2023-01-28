using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueLit2.Classes {
    internal class GameMaster {
        internal Level Level;
        internal Player Player {
            get => _Player ?? throw new("Player is null");
            set {
                if (_Player == null) {
                    Level.SpawnCreature(Creature.Player, value.Position, true);
                    _Player = value;
                    return;
                }

                Level.MoveCreature(_Player.Position, value.Position, true);
                _Player = value;
            }
        }
        private Player? _Player;


        internal GameMaster() {
            Level = new(150, 100);
            Point PlayerSpawn = Level.GetRandomFreeSpace();

            Player = new(PlayerSpawn);
        }

    }
}
