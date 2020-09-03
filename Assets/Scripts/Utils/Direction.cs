using System;
using UnityEngine;

namespace Utils
{
    //a direction to move in
    public enum Direction
    {
        None, Up, Down, Left, Right
    }
    
    //methods that interact with the direction enum
    public static class DirectionMethods
    {
        //the directions in clockwise order
        private static readonly Direction[] Clockwise = new Direction[]
            {Direction.Up, Direction.Right, Direction.Down, Direction.Left};

        /// <summary>
        /// get direction clockwise starting from up
        /// </summary>
        /// <returns>directions in clockwise order</returns>
        public static Direction[] GetClockwise()
        {
            return Clockwise;
        }

        /// <summary>
        /// return the opposite of a given direction
        /// </summary>
        /// <param name="direction">the direction to get the opposite of</param>
        /// <returns>the opposite direction of the provided direction</returns>
        public static Direction Opposite(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
                default:
                    return Direction.None;
            }
        }

        /// <summary>
        /// given two coordinates that share an axis, determine which direction the first must face to look at the other
        /// </summary>
        /// <param name="coords1">the "origin" coordinates</param>
        /// <param name="coords2">the "other" coordinates</param>
        /// <returns>direction coords1 faces coords2, or direction.none if not on 1 of same axis</returns>
        public static Direction GetOrientation(Coords coords1, Coords coords2)
        {
            //check if coords2 is NOT on one of the same axis as coords1
            if (coords2.X != coords1.X && coords2.Y != coords1.Y)
                return Direction.None; //neither orientation axis is shared
            
            //one of the coordinates is different, so find out which one and in what direction
            if (coords2.X > coords1.X)
                return Direction.Right;
            else if (coords2.X < coords1.X)
                return Direction.Left;
            else if (coords2.Y > coords1.Y)
                return Direction.Up;
            else
                return Direction.Down;
        }

        /// <summary>
        /// Get Angle of a direction
        /// </summary>
        /// <param name="direction">direction to convert</param>
        /// <returns>angle of direction as integer</returns>
        public static int GetAngle(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return 0;
                case Direction.Left:
                    return 270;
                case Direction.Right:
                    return 90;
                default:
                    return 180;
            }
        }
    }
}
