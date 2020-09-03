using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Utils
{
    public class Grid : Singleton<Grid>
    {
        //Grid properties
        public const int GridWidth = 10; //height of game grid
        public const int GridHeight = 8; //width of game grid
        public const int CellOffset = 0; //offset of cells
        public const int MapHeight = 0; //the y-axis height of the map
        
        //entities to populate grid with
        [SerializeField] private GameObject friendlySwordsman;
        [SerializeField] private GameObject friendlyCrossbowman;
        [SerializeField] private GameObject obstacle;
        [SerializeField] private GameObject enemySwordsman;
        [SerializeField] private GameObject enemyCrossbowman;
        
        //hold each gameobject at a position in the array
        private readonly GameObject[,] _gridArray = new GameObject[GridWidth, GridHeight];

        /// <summary>
        /// Populate grid with a units & obstacles, etc.
        /// </summary>
        public void PopulateGrid()
        {
            _gridArray[3, 2] = obstacle;
            _gridArray[3, 5] = obstacle;
            _gridArray[6, 2] = obstacle;
            _gridArray[6, 5] = obstacle;
            
            _gridArray[0, 3] = friendlyCrossbowman;
            _gridArray[0, 5] = friendlyCrossbowman;
            _gridArray[1, 3] = friendlySwordsman;
            _gridArray[1, 4] = friendlySwordsman;
            
            _gridArray[9, 2] = enemyCrossbowman;
            _gridArray[7, 1] = enemySwordsman;
            _gridArray[7, 6] = enemySwordsman;
        }

        /// <summary>
        /// Get gameobject at coordinate location
        /// </summary>
        /// <param name="coords">coordinate location</param>
        /// <returns>the gameobject at location</returns>
        public GameObject GetGridArray(Coords coords)
        {
            return _gridArray[coords.X, coords.Y];
        }

        /// <summary>
        /// Set the gameobject at the given location
        /// </summary>
        /// <param name="coords">coordinates</param>
        /// <param name="gameObject">gameobject to set</param>
        public void SetGridArray(Coords coords, GameObject gameObject)
        {
            _gridArray[coords.X, coords.Y] = gameObject;
        }
        
        /// <summary>
        /// check whether coordinates are inside of map or not
        /// </summary>
        /// <param name="coords">coordinates to evaluate</param>
        /// <returns>true if inside map, false otherwise</returns>
        public static bool InMap(Coords coords)
        {
            return coords.X >= 0 && coords.X < GridWidth && coords.Y >= 0 && coords.Y < GridHeight;
        }

        /// <summary>
        /// Check if a space is free of another unit or obstacle
        /// </summary>
        /// <param name="coords">the coords to check</param>
        /// <returns>true if space is empty, false otherwise</returns>
        public bool IsFree(Coords coords)
        {
            //Debug.Log(coords);
            return InMap(coords) && _gridArray[coords.X, coords.Y] == null;
        }

        /// <summary>
        /// check is coordinates contain enemy
        /// Prerequisites: must call IsFree before and make sure it is false
        /// </summary>
        /// <param name="coords">the coords to check</param>
        /// <returns>true if space contains enemy, false otherwise</returns>
        public bool IsEnemy(Coords coords)
        {
            return _gridArray[coords.X, coords.Y].CompareTag("Enemy Unit");
        }
        
        /// <summary>
        /// Convert coordinates into a world space position
        /// </summary>
        /// <param name="coords">the xy coords to convert</param>
        /// <returns>Vector3 of coordinates with y value of MapHeight</returns>
        public static Vector3 CoordsToWorldSpace(Coords coords)
        {
            return new Vector3(coords.X, MapHeight, coords.Y);
        }
        
        /// <summary>
        /// Turn a world space vector 3 into xy coordinates
        /// </summary>
        /// <param name="point">the point to convert</param>
        /// <returns>Coords(x,y) x and y)</returns>
        public static Coords WorldSpaceToCoords(Vector3 point)
        {
            return new Coords(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.z));
        }

        /// <summary>
        /// Get the distance between two coordinates
        /// </summary>
        /// <param name="coords1">first coords</param>
        /// <param name="coords2">second coords</param>
        /// <returns>Distance in spaces (no diagonals)</returns>
        public static int Distance(Coords coords1, Coords coords2)
        {
            return Math.Abs(coords2.X - coords1.X) + Math.Abs(coords2.Y - coords1.Y);
        }
    }
}
