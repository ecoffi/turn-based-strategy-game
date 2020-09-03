using UnityEngine;

namespace Utils
{
    //a cell in an array that store data used in breadth-first search
    public class Cell
    {
        public enum Color
        {
            White, //unvisited
            Black, //visited
            Grey //seen, but not visited
        }

        private Color _color;
        private Direction _to;
        private Direction _from;
        private int _distance; //the distance in cells from a location

        /// <summary>
        /// empty constructor creates a white (unvisited) cell
        /// </summary>
        private Cell() : this(Color.White)
        {
        }

        public Cell(Color color)
        {
            this.SetColor(color);
            SetTo(Direction.None);
            SetFrom(Direction.None);
            _distance = 0; //default distance is zero
        }

        /// <summary>
        /// create a copy of this cell
        /// </summary>
        /// <returns>a copy of this cell</returns>
        public Cell Copy()
        {
            Cell clone = new Cell();
            clone.SetColor(GetColor());
            clone.SetTo(GetTo());
            clone.SetDistance(GetDistance());
            return clone;
        }

        public Color GetColor()
        {
            return _color;
        }

        public void SetColor(Color color)
        {
            this._color = color;
        }

        public Direction GetTo()
        {
            return _to;
        }

        public void SetTo(Direction to)
        {
            this._to = to;
        }

        public Direction GetFrom()
        {
            return _from;
        }

        public void SetFrom(Direction from)
        {
            this._from = from;
        }

        public int GetDistance()
        {
            return _distance;
        }

        public void SetDistance(int distance)
        {
            _distance = distance;
        }
    }
}