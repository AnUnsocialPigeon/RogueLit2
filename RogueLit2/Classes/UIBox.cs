namespace RogueLit2.Classes {
    internal class UIBox {
        internal UIBox(Point Position, string[] Contents, int Width, int Height, int ID) {
            this.Position = Position;
            this.Contents = Contents;
            this.Width = Width;
            this.Height = Height;
            this.ID = ID;
        }

        /// <summary>
        /// The top left co-ordinate of the Box
        /// </summary>
        internal Point Position;
        internal string[] Contents;
        internal int ID, Width, Height;
        internal bool NeedReDraw = true;

        internal bool PointInArea(Point position) =>
            position.x >= Position.x && position.x < Position.x + Width &&
            position.y >= Position.y && position.y < Position.y + Height;

    }
}
