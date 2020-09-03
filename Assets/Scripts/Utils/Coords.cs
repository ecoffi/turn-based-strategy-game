using UnityEngine;

namespace Utils
{
    /// <summary>
    /// a pair of ints representing a place on a grid
    /// </summary>
    public readonly struct Coords
    {
        public Coords(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}";
        }

        /// <summary>
        /// make a copy of this coords
        /// </summary>
        /// <returns>a new coords with same x and y</returns>
        private Coords Copy()
        {
            return new Coords(X, Y);
        }

        /// <summary>
        /// get coords moving in a direction
        /// </summary>
        /// <param name="direction">the direction to move in</param>
        /// <returns>new coords in that direction</returns>
        public Coords Get(Direction direction)
        {
            Coords newCoords = this.Copy();
            switch (direction)
            {
               case Direction.Up:
                   newCoords = new Coords(X, Y + 1);
                   break;
               case Direction.Down:
                   newCoords = new Coords(X, Y - 1);
                   break;
               case Direction.Right:
                   newCoords = new Coords(X + 1, Y);
                   break;
               case Direction.Left:
                   newCoords = new Coords(X - 1, Y);
                   break;
            }
            return newCoords;
        }

        /// <summary>
        /// Check whether coordinates are part of the map or not
        /// </summary>
        /// <returns>true if out of bounds, false otherwise</returns>
        public bool IsOutOfBounds()
        {
            return X < 0 || X > Grid.GridWidth || Y < 0 || Y > Grid.GridHeight;
        }
    }
}

