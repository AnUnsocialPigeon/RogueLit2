namespace RogueLit2.Classes.Controllers {
    internal class GameMaster {
        internal Level Level;
        internal AudioController AudioController;
        internal CreatureHandler CreatureHandler;
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

        public CameraHandler CameraHandler { get; internal set; }

        private Player? _Player;


        private int GameTickTime = 400;
        private int StartChanceForSpawnPerTick = 20;
        private int StartUnitBucketSize = 20;
        private int LevelWidth = 100;
        private int LevelHeight = 100;
        internal int TorchLeniency = 2;
        internal int Depth = 1;
        private int DepthUIBoxID;

        private readonly Random rnd = new();
        private Task CreatureHandlerTask;
        private Task CreepyBreathTask;

        private SFX[] WinSFX = new SFX[] { SFX.Bell1, SFX.Bell2, SFX.Bell4, SFX.Bell3, SFX.JumpScare1 };


        internal GameMaster() {
            AudioController = new();
        }
        internal void Begin() {
            CreatureHandler = new(this, GameTickTime, StartChanceForSpawnPerTick);
            GenerateLevel();

            DepthUIBoxID = CameraHandler.CreateUIBox(GetDepthContents(), 14, 3, new((CameraHandler.CameraWidth * 2) - 14, 0));

            CreatureHandlerTask = CreatureHandler.Start();

            // Reset CreatureHandler properties
            CreatureHandler.Currency = 0;
            CreatureHandler.GameTickTime = GameTickTime;
            CreatureHandler.CurrencyPerTick = 1;
            CreatureHandler.UnitBucketSize = StartUnitBucketSize;
            CreatureHandler.ChanceForSpawnPerTick = StartChanceForSpawnPerTick;

            // Starts a task loop to play creepy breath audio's
            CreepyBreathTask = new(() => {
                SFX[] chooseFrom = new SFX[] { SFX.Crechendo1, SFX.Dino1, SFX.Dino2, SFX.Dino3 };
                while (true) {
                    Thread.Sleep(1000);
                    if (rnd.Next(70) == 0) {
                        AudioController.PlayAudio(chooseFrom[rnd.Next(chooseFrom.Length)]);
                        Thread.Sleep(24000);
                    }
                }
            });
            CreepyBreathTask.Start();


        }

        public void Reset() {
            Depth = 1;
            GenerateLevel();

            CameraHandler.UpdateTorchUI();
        }

        internal void GenerateLevel() {
            Level = new(LevelWidth, LevelHeight, new(new(0, 0)));

            // reset player
            ResetPlayer();
        }

        private void ResetPlayer() {
            _Player = null;
            Player = new(Level.GetRandomFreeSpace());
            Level.Player = Player;
        }

        internal void LightTorch(Point newPos, bool debug = true) {
            Level.LightTorch(newPos, debug);
            AudioController.PlayAudio(SFX.TorchLighting1);

            // Win (A level)
            if (Level.LitTorches >= Level.MaxTorches - TorchLeniency) {
                AudioController.PlayAudio(WinSFX[Math.Min(Depth, WinSFX.Length - 1)]);
                
                GenerateLevel();
                CameraHandler.UpdateTorchUI();
                CameraHandler.UpdateUIBox(GetDepthContents(), DepthUIBoxID);

                CreatureHandler.UnitBucketSize *= 1.3d;
                CreatureHandler.CurrencyPerTick *= 1.4d;
                CreatureHandler.Currency = 7 * Depth;
                CreatureHandler.ChanceForSpawnPerTick *= 0.9f;

                Depth++;
            }
        }

        private string[] GetDepthContents() =>
            new string[] {
                "+------------+",
                $"| Depth: {Depth}{string.Concat(Enumerable.Repeat(" ", 4 - Depth.ToString().Length))}|",
                "+------------+",
            };

        internal Task PlayAudio(SFX audio, bool loop = false) => AudioController.PlayAudio(audio, loop);

        internal void StopAudio(SFX audio) => AudioController.StopAudio(audio);

        internal bool IsAudioPlaying(SFX audio) => AudioController.IsAudioPlaying(audio);
    }
}
