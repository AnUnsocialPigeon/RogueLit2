namespace RogueLit2.Classes.Controllers {
    internal class MovementController {
        private GameMaster GameMaster;
        private CameraHandler Camera;
        private const int LitTorchLightEmittanceLevel = 5;

        public MovementController(GameMaster gameMaster, CameraHandler camera) {
            GameMaster = gameMaster;
            Camera = camera;
        }

        /// <summary>
        /// Move in direction
        /// </summary>
        /// <param name="dir">0 = up, 1 = left, 2 = down, 3 = right</param>
        public bool MovePlayer(int dir) {
            Point newPos = GetPosInDirectionOfPlayer(dir);

            // Bound + wall check
            if (newPos.x < 0 || newPos.y < 0 || newPos.x >= GameMaster.Level.AreaWidth || newPos.y >= GameMaster.Level.AreaHeight ||
                !GameMaster.Level.GetTile(newPos).Property.WalkThroughable) return false;

            // GameMaster.Player will handle the moving of the player through the setter.
            if (GameMaster.Level.MoveCreature(GameMaster.Player.Position, newPos))
                GameMaster.Player.Position = newPos;
            
            // Lighting Torches
            if (GameMaster.Level.GetTile(newPos).Property.Texture == Texture.UnlitTorch) {
                GameMaster.LightTorch(newPos);
                Camera.UpdateTorchUI();
                return true;
            }

            Camera.QueueUpdate();
            return true;
        }

        private Point GetPosInDirectionOfPlayer(int dir) {
            if (dir > 4 || dir < 0) throw new("Direction out of bounds");
            Point offset = new Point[] { new(0, -1), new(-1, 0), new(0, 1), new(1, 0) }[dir];
            Point newPos = new(GameMaster.Player.Position.x + offset.x, GameMaster.Player.Position.y + offset.y);
            return newPos;
        }

        internal bool Mine(int dir) {
            Point block = GetPosInDirectionOfPlayer(dir);
            if (GameMaster.Level.GetTile(block).Property.Texture != Texture.Rock) return false;
            GameMaster.Level.GetTile(block).SetTexture(Texture.Floor);
            Camera.QueueUpdate();
            return true;
        }
    }
}
