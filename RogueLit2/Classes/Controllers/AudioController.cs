//using NAudio.Wave;

using System.Runtime.InteropServices;
using System.Text;

namespace RogueLit2.Classes.Controllers {
    internal class AudioController {
        private Dictionary<SFX, string> SFXLocation = new() {
            { SFX.Footstep1, Directory.GetCurrentDirectory() + @"\Assets\PlayerandPickAxe\Footstep1.wav" },
            { SFX.Background1, Directory.GetCurrentDirectory() + @"\Assets\Cave\CaveAmbienceCreepy_HV.137.wav" },
            { SFX.Background2, Directory.GetCurrentDirectory() + @"\Assets\Cave\CaveAmbienceWhispers_HV.139.wav" },
            { SFX.Background3, Directory.GetCurrentDirectory() + @"\Assets\Cave\CaveAmbienceDrips_HV.138.wav" },
            { SFX.Bell1, Directory.GetCurrentDirectory() + @"\Assets\Other\old-church-bell-6298.wav" },
            { SFX.Bell2, Directory.GetCurrentDirectory() + @"\Assets\Other\creepyChurch1.wav" },
            { SFX.Bell3, Directory.GetCurrentDirectory() + @"\Assets\Other\tubular-bell-of-death-89485.wav" },
            { SFX.Bell4, Directory.GetCurrentDirectory() + @"\Assets\Other\churchbellkonstanz-6372.wav" },
            { SFX.Swing1, Directory.GetCurrentDirectory() + @"\Assets\PlayerandPickAxe\SkullHitPickAxe_ZA02.505.wav" },
            { SFX.Rubble1, Directory.GetCurrentDirectory() + @"\Assets\PlayerandPickAxe\RocksFallGround_ZA02.448.wav" },
            { SFX.SmallRubble1, Directory.GetCurrentDirectory() + @"\Assets\PlayerandPickAxe\Small_Debris_Scattering.wav" },
            { SFX.Grunt1, Directory.GetCurrentDirectory() + @"\Assets\PlayerandPickAxe\HumanGrunt_S08HU.271.wav" },
            { SFX.BodyFall1, Directory.GetCurrentDirectory() + @"\Assets\PlayerandPickAxe\BodyfallDirt_ZA01.75.wav" },
            { SFX.Aggression1, Directory.GetCurrentDirectory() + @"\Assets\Creature\MonsterSnarlType1_HV.507.wav" },
            { SFX.LoseAggression1, Directory.GetCurrentDirectory() + @"\Assets\Creature\MonsterSnarlSlow_S08AN.273.wav" },
            { SFX.TorchLighting1, Directory.GetCurrentDirectory() + @"\Assets\Fire\Flamethrower_Burst_03.wav" },
            { SFX.JumpScare1, Directory.GetCurrentDirectory() + @"\Assets\Other\jump-scare_1-66858.wav" },
            { SFX.Whoosh1, Directory.GetCurrentDirectory() + @"\Assets\Other\creepy-hifreq-woosh-6873.wav" },
            { SFX.Whimper1, Directory.GetCurrentDirectory() + @"\Assets\Creature\Whimper.wav" },
            //{ SFX.Fire1, Directory.GetCurrentDirectory() + @"\Assets\Fire\Flamethrower_Burst_03.wav" },
            { SFX.Fire2, Directory.GetCurrentDirectory() + @"\Assets\Fire\FireCampfire_ZA01.255.wav" },
            { SFX.CreepyBreath1, Directory.GetCurrentDirectory() + @"\Assets\Creature\BreatheGhostEerie_S08HO.16.wav" },
            { SFX.Dino1, Directory.GetCurrentDirectory() + @"\Assets\Creature\Dino1.wav" },
            { SFX.Dino2, Directory.GetCurrentDirectory() + @"\Assets\Creature\Dino2.wav" },
            { SFX.Dino3, Directory.GetCurrentDirectory() + @"\Assets\Creature\Dino3.wav" },
            { SFX.Crechendo1, Directory.GetCurrentDirectory() + @"\Assets\Other\CrescendoSwell_S08HO.27.wav" },
            { SFX.Confetti, Directory.GetCurrentDirectory() + @"\Assets\Other\Confetti.wav" },
            { SFX.Yay, Directory.GetCurrentDirectory() + @"\Assets\Other\Yay.wav" },
            { SFX.MainTheme1, Directory.GetCurrentDirectory() + @"\Assets\executed-by-guillotine-74406.wav" },
            { SFX.Hell1, Directory.GetCurrentDirectory() + @"\Assets\Wails_In_Hell.wav" },
        };
        private Dictionary<SFX, int> SFXLengths = new() {
            { SFX.Footstep1, 59 },
            { SFX.Background1, 120 },
            { SFX.Background2, 121 },
            { SFX.Background3, 120 },
            { SFX.Bell1, 4 },
            { SFX.Bell2, 5 },
            { SFX.Bell3, 4 },
            { SFX.Bell4, 15 },
            { SFX.Swing1, 0 },
            { SFX.Rubble1, 2 },
            { SFX.SmallRubble1, 2 },
            { SFX.Grunt1, 1 },
            { SFX.BodyFall1, 1 },
            { SFX.Aggression1, 3 },
            { SFX.LoseAggression1, 3 },
            { SFX.TorchLighting1, 1 },
            { SFX.JumpScare1, 18 },
            { SFX.Whoosh1, 7 },
            { SFX.Whimper1, 2 },
            //{ SFX.Fire1, 1 },
            { SFX.Fire2, 119 },
            { SFX.CreepyBreath1, 3 },
            { SFX.Dino1, 10 },
            { SFX.Dino2, 8 },
            { SFX.Dino3, 11 },
            { SFX.Crechendo1, 29 },
            { SFX.Confetti, 1 },
            { SFX.Yay, 1 },
            { SFX.MainTheme1, 54 },
            { SFX.Hell1, 60 },
        };

        private Dictionary<SFX, AudioObject> AudioObjects = new();

        [DllImport("winmm.dll")]
        static extern Int32 mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

        internal AudioController() {
            // Init
            foreach (KeyValuePair<SFX, string> sfx in SFXLocation) {
                ////_ = mciSendString($"open {sfx.Value} type waveaudio alias {sfx.Key}", null, 0, IntPtr.Zero);
                int songLength = SFXLengths[sfx.Key];

                // Add to the list of audio objects
                AudioObjects.Add(sfx.Key, new(sfx.Key.ToString(), DateTime.MinValue, songLength));
            }
        }

        internal async Task<Task> PlayAudio(SFX sfx, bool loop = false) {
            AudioObjects[sfx].StartTime = DateTime.Now;
            mciSendString($"close {sfx}", null, 0, IntPtr.Zero);
            mciSendString($"open {SFXLocation[sfx]} type waveaudio alias {sfx}", null, 0, IntPtr.Zero);
            mciSendString($"play {sfx}", null, 128, IntPtr.Zero);

            // Start the task only if loop is true
            if (loop) {
                Task t = Task.Factory.StartNew(() => {
                    while (loop && AudioObjects[sfx].StartTime != DateTime.MinValue) {
                        if (!AudioObjects[sfx].IsPlaying) {
                            PlayAudio(sfx, false);
                        }
                        Thread.Sleep(AudioObjects[sfx].Length * 1000);
                    }
                }, TaskCreationOptions.LongRunning);
                return t;
            }
            return Task.CompletedTask;
        }

        internal void StopAudio(SFX audio) {
            _ = mciSendString($"stop {audio}", null, 0, IntPtr.Zero);
            _ = mciSendString($"close {audio}", null, 0, IntPtr.Zero);
            AudioObjects[audio].StartTime = DateTime.MinValue;
        }
        internal bool IsAudioPlaying(SFX audio) => AudioObjects[audio].IsPlaying;
        private enum AudioStatus {
            Stopped,
            Playing,
            Paused
        }
    }

    internal enum SFX {
        Background1,
        Background2,
        Background3,
        Footstep1,
        Swing1,
        Rubble1,
        Aggression1,
        TorchLighting1,
        CreepyBreath1,
        Dino1,
        Dino2,
        Dino3,
        Crechendo1,
        Grunt1,
        BodyFall1,
        SmallRubble1,
        Yay,
        Confetti,
        MainTheme1,
        LoseAggression1,
        Fire1,
        Fire2,
        Bell1,
        Bell2,
        Bell3,
        Bell4,
        JumpScare1,
        Whoosh1,
        Whimper1,
        Hell1,
    }
}
