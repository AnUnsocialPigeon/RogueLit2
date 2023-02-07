using Figgle;
using NAudio.Wave;
using RogueLit2.Classes;
using RogueLit2.Classes.Controllers;
using System.Runtime.InteropServices;

namespace RogueLit2 {
    internal class Program {
        // Handlers
        private static GameMaster GameMaster = new();
        private static readonly CameraHandler CameraHandler = new(GameMaster);
        private static readonly MovementController MovementHandler = new(GameMaster, CameraHandler);
        private const int MinTimeBetweenMoves = 250;
        private const int MinTimeBetweenMines = 800;
        private const int MinTimeBetweenFootsteps = 500;

        private static Random rnd = new();

        private static Task? BackgroundLoop;


        // Key input obtaining
        [DllImport("user32.dll")]
        private static extern int GetAsyncKeyState(Int32 KeyboardKeyDown);

        // https://warwick.ac.uk/fac/sci/dcs/intranet/staff/engagement/gamejam
        public static void Main() {
            // Thread dedicated to stopping the resize
            DisableConsoleMovement.DisableResize();
            Console.CursorVisible = false;
            Console.Title = "RogueLit 2";

            // Intro
            Console.WriteLine("Warwick GameJam Submission 2023 - Theme: Rogue");
            Console.WriteLine("\nA quick introduction to RogueLit 2 : Made by Jake (Solo)");
            Console.WriteLine("\nThis is a game that has been fully built in C#");
            Console.WriteLine("No game engine, no external libraries (except Spectre.Console, for the UI colours)");
            Console.WriteLine("No nothing. Pure C#.\n\nSource Code can be found here: https://github.com/AnUnsocialPigeon/RogueLit2");
            Console.WriteLine("\nPress 'Enter' to continue [Introduction 1/4]");
            Console.ReadLine();
            GameMaster.PlayAudio(SFX.Menu1);
            Console.Clear();

            Console.WriteLine("Warwick GameJam Submission 2023 - Theme: Rogue");
            Console.WriteLine("\nControls:");
            Console.WriteLine("WASD or Arrow keys   = Move");
            Console.WriteLine("Space                = Mine");
            Console.WriteLine("\nPress 'Enter' to continue [Introduction 2/4]");
            Console.ReadLine();
            GameMaster.PlayAudio(SFX.Menu1);
            Console.Clear();

            Console.WriteLine("Warwick GameJam Submission 2023 - Theme: Rogue");
            Console.WriteLine("\nIntroduction:");
            Console.WriteLine("You don't know where you are, you don't know where you came from, you dont even know who you even are. " +
                "All you know is that you do not belong here. Armed with a torch, a pickaxe, and not much else, you set your sights on survival " +
                "by any means necessary.\nLittered around are beacon-like constructs. They seem to glow red, the embers of whoever came before " +
                "still emitting some vague warmth. Relighting these might be the key to success. The harrowing screams of the fellow inhabitance of the " +
                "cave do not feel welcoming. Their unrecognizable screams echo through your mind. You raise your torch and swing the pickaxe over one shoulder. " +
                "\nIt's time to move.");
            Console.WriteLine("\nPress 'Enter' to continue [Introduction 3/4]");
            Console.ReadLine();
            GameMaster.PlayAudio(SFX.Menu1);
            Console.Clear();

            new Task(() => {
                while (true) {
                    DisableConsoleMovement.SetFont("consolas", 26);
                    Thread.Sleep(250);
                }
            }).Start();

            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n                                             Use headphones for the best experience.");
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\nPress 'Enter' to continue");
            Console.ReadLine();
            GameMaster.PlayAudio(SFX.Bell3);
            Thread.Sleep(500);


            //GameMaster = new();
            StartScreen();


            GameMaster.CameraHandler = CameraHandler;
            GameMaster.Begin();
            CameraHandler.CreateTorchUI();


            //// Key input task
            //Thread t = new(() => { HandleUserInput(); });
            //t.Start();
            CameraHandler.QueueUpdate();
            BackgroundLoop = GameMaster.PlayAudio(SFX.Background1, true);

            HandleUserInput();
        }

        private static void StartScreen() {
            GameMaster.PlayAudio(SFX.MainTheme1, true);
            GameMaster.PlayAudio(SFX.Fire2, true);
            GameMaster.PlayAudio(SFX.Hell1, true);

            // Main start sim
            MainScreenSimulation sim = new(CameraHandler.CameraWidth, CameraHandler.CameraHeight, 0);
            sim.Start();
            Console.ReadLine();
            sim.Dispose();
            Console.Clear();

            GameMaster.StopAudio(SFX.MainTheme1);
            GameMaster.StopAudio(SFX.Fire2);
            GameMaster.StopAudio(SFX.Hell1);
        }

        private static DateTime lastMovementKeyPressed = DateTime.Now;
        private static DateTime lastMineKeyPressed = DateTime.Now;
        private static DateTime lastKeyPressed = DateTime.Now;

        private static int lastMovementDirection = 0;
        private static Dictionary<string, Action> KeyToAction = new() {
            { "w", () => { Move(0); lastMovementDirection = 0; } },
            { "[Up]", () => { Move(0); lastMovementDirection = 0; } },
            { "a", () => { Move(1); lastMovementDirection = 1; } },
            { "[Left]", () => { Move(1); lastMovementDirection = 1; } },
            { "s", () => { Move(2); lastMovementDirection = 2; } },
            { "[Down]", () => { Move(2); lastMovementDirection = 2; } },
            { "d", () => { Move(3); lastMovementDirection = 3; } },
            { "[Right]", () => { Move(3); lastMovementDirection = 3; } },
            { "[Space]", () => { Mine(); } },
            { "ERROR", () => { } }
        };
        private static void Move(int v) {
            if (!CanMove(2)) return;
            bool success = MovementHandler.MovePlayer(v);
            lastMovementKeyPressed = DateTime.Now;
            if (success && !GameMaster.IsAudioPlaying(SFX.Footstep1)) {
                GameMaster.PlayAudio(SFX.Footstep1);
            }

        }

        private static void Mine() {
            if (!CanMove()) return;
            bool success = MovementHandler.Mine(lastMovementDirection);
            GameMaster.PlayAudio(SFX.Swing1);
            if (success) GameMaster.PlayAudio(SFX.Rubble1);
            lastMineKeyPressed = DateTime.Now;
        }

        private static bool CanMove(double mineMultiplier = 1f) => !GameMaster.CameraHandler.Pause &&
            (DateTime.Now - lastMovementKeyPressed).TotalMilliseconds > MinTimeBetweenMoves &&
            (DateTime.Now - lastMineKeyPressed).TotalMilliseconds > (MinTimeBetweenMines / mineMultiplier);

        private static void HandleUserInput() {
            while (true) {
                string? key = null;
                // Buffer zone
                while (key == null) {
                    string buffer = ReadKeyStates();
                    if ((DateTime.Now - lastMovementKeyPressed).TotalMilliseconds > MinTimeBetweenFootsteps && GameMaster.IsAudioPlaying(SFX.Footstep1)) {
                        GameMaster.StopAudio(SFX.Footstep1);
                    }
                    if (!KeyToAction.Keys.Contains(buffer)) continue;
                    key = buffer;
                }
                // Should never "ERROR"
                lastKeyPressed = DateTime.Now;
                KeyToAction[key ?? "ERROR"]();
            }
        }

        private static string ReadKeyStates() {
            foreach (int i in KeyValue.AllKeyValues) {
                int wasKeypressed = GetAsyncKeyState(i);
                if (wasKeypressed > 0)
                    return KeyValue.VerifyKey(i);

            }
            return "";
        }
    }
}