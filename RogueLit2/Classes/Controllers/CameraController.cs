using Spectre.Console;

namespace RogueLit2.Classes.Controllers {
    internal class CameraHandler {
        private GameMaster GameMaster;

        // Render vars
        private List<UIBox> UIBoxes = new();
        private int UIBoxID = 0;
        private GuiObject[] LastRender = Array.Empty<GuiObject>();
        private bool FirstRender = true;
        internal const int CameraHeight = 30;
        internal const int CameraWidth = 63;
        internal bool Pause = false;

        private int UpdateBuffer = 0;

        private static readonly Dictionary<int, string> DefaultLightHexValues = new() {
            { 0, "000000" },
            { 1, "443322" },
            { 2, "665544" },
            { 3, "776655" },
            { 4, "aa9988" },
            { 5, "ddccbb" },
            { 6, "ffeedd" },
            { 7, "ffffff" },
            { 8, "ffffff" },
            { 9, "ffffff" },
            { 10, "ffffff" },
            { 11, "ffffff" },
            { 12, "ffffff" },
        };
        private static readonly Dictionary<int, string> RedLightHexValues = new() {
            { 0, "000000" },
            { 1, "200000" },
            { 2, "400505" },
            { 3, "550505" },
            { 4, "700505" },
            { 5, "800505" },
            { 6, "900505" },
            { 7, "aa0505" },
        };


        public CameraHandler(GameMaster gameMaster) {
            GameMaster = gameMaster;
        }

        /// <summary>
        /// Queues screen update to be buffered
        /// </summary>
        public void QueueUpdate() {
            UpdateBuffer++;
            if (UpdateBuffer > 1) return;

            Task t = Task.Run(() => {
                while (UpdateBuffer > 0) {
                    while (Pause)
                        continue;
                    Update();
                    UpdateBuffer--;
                }
            });
        }

        /// <summary>
        /// Updates the screen with what it needs to display
        /// </summary>
        private void Update() {
            GameMaster.Level.UpdateAllLightValue();
            Tile[] subLevel = GetRenderedTilesAroundPlayer(CameraWidth, CameraHeight);

            GuiObject[] toRender = new GuiObject[subLevel.Length];
            for (int i = 0; i < subLevel.Length; i++) {
                toRender[i] = GetGuiRenderObject(subLevel[i]);
            }
            RenderTileToScreen(toRender);
        }

        private GuiObject GetGuiRenderObject(Tile tile) {
            string icon = tile.Seen ?
                    tile.Creature != null && tile.LightLevel > 0 ?
                        tile.Creature.Icon :
                    tile.Property.Icon :
                tile.RedLightLevel > 0 ?
                    "??" :
                "  ";

            string colour = DefaultLightHexValues[tile.LightLevel];
            if (colour == DefaultLightHexValues[0])
                colour = RedLightHexValues[tile.RedLightLevel];

            // Remove formatting
            return new(icon, colour);
        }

        /// <summary>
        /// Renders all guiObjects to the screen
        /// </summary>
        /// <param name="guiObject">An array to be the same size as everything that needs to be on the screen</param>
        /// <param name="forceOverride">Whether you want to refresh entirely</param>
        private void RenderTileToScreen(GuiObject[] guiObject, bool forceOverride = false) {
            // Setup cursor position
            bool skip = false;
            Console.SetCursorPosition(0, 0);
            if (FirstRender) {
                LastRender = new GuiObject[guiObject.Length].Select(x => x = new("EE", "")).ToArray();
                FirstRender = false;
            }

            string toRenderMarkup = "";
            for (int i = 0; i < guiObject.Length; i++) {
                // Replace
                toRenderMarkup += RenderTile(i, guiObject[i], skip);
                if ((i + 1) % CameraWidth == 0) toRenderMarkup += "\n";
            }

            Console.SetCursorPosition(0, 0);
            AnsiConsole.Markup(toRenderMarkup);
        }

        /// <summary>
        /// renders a series of tiles to the screen
        /// </summary>
        /// <param name="longPos">the longPos of the tile that needs to be rendered to the screen</param>
        /// <param name="guiObject">The GuiObject to render to the screen</param>
        /// <param name="skip">Whether there has been tiles that have been skipped</param>
        private string RenderTile(int longPos, GuiObject guiObject, bool skip) {
            Point position = new((longPos * guiObject.icon.Length) % (CameraWidth * guiObject.icon.Length), longPos / CameraWidth);

            if (UIBoxes.Any(x => x.PointInArea(position))) {
                UIBox overlap = UIBoxes.Where(x => x.PointInArea(position)).Last();
                return GetPartOfUIBox(overlap, position);
            }
            return $"[#{guiObject.hexColour}]{guiObject.icon}[/]";
        }

        private string GetPartOfUIBox(UIBox overlap, Point position) {
            Point difference = new(position.x - overlap.Position.x, position.y - overlap.Position.y);
            return String.Join("", overlap.Contents[difference.y][difference.x], overlap.Contents[difference.y][difference.x + 1]);
        }

        private Tile[] GetRenderedTilesAroundPlayer(int cameraWidth, int cameraHeight) {
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
        private Point GetCameraPosition(int cameraWidth, int cameraHeight) {
            Point playerPos = GameMaster.Player.Position;
            int areaWidth = GameMaster.Level.AreaWidth;
            int areaHeight = GameMaster.Level.AreaHeight;

            Point cameraPos = new(
                Math.Clamp(playerPos.x - cameraWidth / 2, 0, areaWidth - cameraWidth),
                Math.Clamp(playerPos.y - cameraHeight / 2, 0, areaHeight - cameraHeight)
            );

            return cameraPos;
        }

        #region TorchUI
        private int TorchUIID = 0;
        public void UpdateTorchUI() => UpdateUIBox(BoxContents, TorchUIID);
        public void CreateTorchUI() {
            TorchUIID = CreateUIBox(BoxContents, BoxContents[0].Length, 3, new(0, 0));
            UpdateTorchUI();
        }
        private string[] BoxContents => new string[] {
            "+----------------+",
            "| Torches: " + GameMaster.Level.LitTorches + "/" + (GameMaster.Level.MaxTorches - GameMaster.TorchLeniency)+ "  |",
            "+----------------+"
        };
        #endregion

        public int CreateUIBox(string[] contents, int width, int height, Point position) {
            int id = UIBoxID++;
            UIBoxes.Add(new(position, contents, width, height, id));
            QueueUpdate();
            return id;
        }
        public void UpdateUIBox(string[] contents, int id) {
            foreach (UIBox b in UIBoxes) {
                if (b.ID == id) {
                    b.Contents = contents;
                    b.NeedReDraw = true;
                    QueueUpdate();
                    return;
                }
            }
        }

        internal bool IsPointInView(Point position) {
            Point camera = GetCameraPosition(CameraWidth, CameraHeight);
            return position.x >= camera.x && position.x < camera.x + CameraWidth && position.y >= camera.y && position.y < camera.y + CameraHeight;
        }

        public void DeleteUIBox(int id) {
            UIBoxes.RemoveAll(x => x.ID == id);
            QueueUpdate();
        }

        private void DrawUIBox(int id) {
            UIBox box = UIBoxes.Find(x => x.ID == id) ?? throw new($"UI Box with id={id} doesnt exist");

            for (int y = 0; y < box.Height; y++) {
                Console.SetCursorPosition(box.Position.x, box.Position.y + y);
                string toWrite = box.Contents[y].Substring(0, box.Contents[y].Length < box.Width ? box.Contents[y].Length : box.Width);
                int l = toWrite.Length;
                for (int i = l; i < box.Width; i++) toWrite += " ";
                Console.Write(toWrite);
            }
        }
    }
}
