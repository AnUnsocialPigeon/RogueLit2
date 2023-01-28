using RogueLit2.Classes;
using Spectre.Console;
using System.Runtime.InteropServices;

namespace RogueLit2 {
    internal class Program {
        private static GameMaster GameMaster = new();

        // Render vars
        private static GuiObject[] LastRender = Array.Empty<GuiObject>();
        private static bool FirstRender = true;
        private const int CameraHeight = 22;
        private const int CameraWidth = 90;

        // Key input obtaining
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 KeyboardKeyDown);
        public static string[] ValidKeys = { "w", "a", "s", "d", "[Up]", "[Left]", "[Down]", "[Right]" };

        public static void Main() {
            DisableConsoleMovement.DisableResize();
            Console.CursorVisible = false;

            // Thread dedicated to stopping the resize
            new Task(() => {
                while (true) {
                    DisableConsoleMovement.SetFont("consolas", 36);
                    Thread.Sleep(250);
                }
            }).Start();

            // Key input task
            Thread t = new(() => {
                DateTime lastKeyPressed = DateTime.Now;
                Dictionary<string, Action> KeyToAction = new() {
                    { "w", () => { Move(0); } },
                    { "[Up]", () => { Move(0); } },
                    { "a", () => { Move(1); } },
                    { "[Left]", () => { Move(1); } },
                    { "s", () => { Move(2); } },
                    { "[Down]", () => { Move(2); } },
                    { "d", () => { Move(3); } },
                    { "[Right]", () => { Move(3); } }
                };

                while (true) {
                    string key = ReadForKeyEvent();
                    if (!ValidKeys.Contains(key)) continue;
                    KeyToAction[key]();
                }
            });
            t.Start();

            //Console.ReadLine();
            while (true) {
                Update();
                //Thread.Sleep(250);
            } // The better Console.ReadLine();
        }

        private static string ReadForKeyEvent() {
            while (true) {
                for (int i = 0; i < 255; i++) {
                    int wasKeypressed = GetAsyncKeyState(i);
                    if (wasKeypressed == 1 || wasKeypressed == -32767 || wasKeypressed == 32769) {
                        return KeyValue.VerifyKey(i);
                    }
                }
            }
        }

        /// <summary>
        /// Move in direction
        /// </summary>
        /// <param name="dir">0 = up, 1 = left, 2 = down, 3 = right</param>
        private static void Move(int dir) {
            if (dir > 4 || dir < 0) throw new("Direction out of bounds");
            Point offset = new Point[] { new(0, -1), new(-1, 0), new(0, 1), new(1, 0) }[dir];
            Point newPos = new(GameMaster.Player.Position.x + offset.x, GameMaster.Player.Position.y + offset.y);

            // Bound + wall check
            if (newPos.x >= GameMaster.Level.AreaWidth || newPos.y >= GameMaster.Level.AreaHeight || 
                GameMaster.Level.GetTile(newPos).Texture == Texture.Wall) return;
            
            // Lighting Torches
            if (GameMaster.Level.GetTile(newPos).Texture == Texture.UnlitTorch) {
                GameMaster.Level.GetTile(newPos).Texture = Texture.LitTorch;
            }

            // GameMaster.Player will handle the moving of the player through the setter.
            if (GameMaster.Level.MoveCreature(GameMaster.Player.Position, newPos))
                GameMaster.Player.Position = newPos;
            
        }



        #region CameraRenderer

        private static readonly Dictionary<Creature, char> CreatureRenderPairs = new() {
            { Creature.Player, '@' }
        };
        private static readonly Dictionary<Texture, char> TextureRenderPairs = new() {
            { Texture.Floor, '░' },
            { Texture.Wall, '█' },
            { Texture.Water, '▓' },
            { Texture.UnlitTorch, 'i' },
            { Texture.LitTorch, 'I' }
        };
        private static readonly Dictionary<int, string> ColourHexValues = new() {
            { 0, "000000" },
            { 1, "443322" },
            { 2, "776655" },
            { 3, "aa9988" },
            { 4, "ddccbb" },
            { 5, "ffffff" },
        };


        /// <summary>
        /// Updates the screen with what it needs to display
        /// </summary>
        private static void Update() {
            GameMaster.Level.UpdateAllLightValue();
            Tile[] subLevel = GetRenderedTilesAroundPlayer(CameraWidth, CameraHeight);

            GuiObject[] toRender = new GuiObject[subLevel.Length];
            for (int i = 0; i < subLevel.Length; i++) {
                toRender[i] = GetTileRenderObject(subLevel[i]);
            }

            RenderTileToScreen(toRender);
        }

        private static GuiObject GetTileRenderObject(Tile tile) {
            char icon = tile.Seen ?
                tile.Creature != null ?
                    CreatureRenderPairs[tile.Creature ?? Creature.Player] :
                    TextureRenderPairs[tile.Texture] :
                ' ';

            string colourHex = ColourHexValues[tile.LightLevel];
            return new(icon, colourHex);
        }

        /// <summary>
        /// Renders all guiObjects to the screen
        /// </summary>
        /// <param name="guiObject">An array to be the same size as everything that needs to be on the screen</param>
        /// <param name="forceOverride">Whether you want to refresh entirely</param>
        private static void RenderTileToScreen(GuiObject[] guiObject, bool forceOverride = false) {
            // Setup cursor position
            bool skip = false;
            Console.SetCursorPosition(0, 0);
            if (FirstRender) {
                LastRender = new GuiObject[guiObject.Length].Select(x => x = new('E', "")).ToArray();
                FirstRender = false;
            }

            for (int i = 0; i < guiObject.Length; i++) {
                // Replace
                if (guiObject[i].icon != LastRender[i].icon || guiObject[i].hexColour != LastRender[i].hexColour) {
                    RenderTile(i, guiObject[i], skip);
                    LastRender[i] = new(guiObject[i]);
                    skip = false;
                    continue;
                }
                skip = true;
            }
        }

        /// <summary>
        /// renders a series of tiles to the screen
        /// </summary>
        /// <param name="longPos">the longPos of the tile that needs to be rendered to the screen</param>
        /// <param name="guiObject">The GuiObject to render to the screen</param>
        /// <param name="skip">Whether there has been tiles that have been skipped</param>
        private static void RenderTile(int longPos, GuiObject guiObject, bool skip) {
            Point position = new(longPos % CameraWidth, longPos / CameraWidth);
            if (Console.GetCursorPosition() != (position.x, position.y))
                Console.SetCursorPosition(position.x, position.y);
            AnsiConsole.Markup($"[#{guiObject.hexColour}]{guiObject.icon}[/]");
        }


        private static Tile[] GetRenderedTilesAroundPlayer(int cameraWidth, int cameraHeight) {
            Point cameraPos = GetCameraPosition(cameraWidth, cameraHeight);

            // Get all tiles around the camera
            List<Tile> renderTiles = new();
            for (int y = 0; y < cameraHeight; y++) {
                for (int x = 0; x < cameraWidth; x++) {
                    Point t = new(cameraPos.x + x, cameraPos.y + y);
                    renderTiles.Add(GameMaster.Level.GetTile(t));
                }
            }

            return renderTiles.ToArray();
        }

        /// <summary>
        /// Gets the top left position of the camera
        /// </summary>
        /// <param name="cameraWidth">The width of the camera</param>
        /// <param name="cameraHeight">The height of the camera</param>
        /// <returns></returns>
        private static Point GetCameraPosition(int cameraWidth, int cameraHeight) {
            Point playerPos = GameMaster.Player.Position;
            int areaWidth = GameMaster.Level.AreaWidth;
            int areaHeight = GameMaster.Level.AreaHeight;

            Point cameraPos = new(
                Math.Clamp(playerPos.x - (cameraWidth / 2), 0, areaWidth - (cameraWidth)),
                Math.Clamp(playerPos.y - (cameraHeight / 2), 0, areaHeight - (cameraHeight))
            );

            return cameraPos;
        }

        #endregion
    }
}