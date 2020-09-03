using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Grid = Utils.Grid;

//generate tile map
//copied from Sebastian Lague - https://www.youtube.com/watch?v=gIUVRYViG_g
public class MapGenerator : MonoBehaviour {

    public Transform tilePrefab; //the grass prefab
    public Transform obstaclePrefab; //the obstacle prefab

    public Grid grid; //the grid script
    public UnitManager unitManager;
    public Transform[,] tiles = new Transform[Grid.GridWidth, Grid.GridHeight]; //2d array to store tiles
    
    void Awake()
    {
        GenerateMap ();
    }

    public void GenerateMap() {
        const string mapHolderName = "Generated Map";
        //check if map holder already exists & delete if it does
        if (transform.Find(mapHolderName)) {
            DestroyImmediate(transform.Find(mapHolderName).gameObject);
        }
        //create new mapholder
        Transform mapHolder = new GameObject(mapHolderName).transform;
        mapHolder.parent = transform;
        
        const string unitHolderName = "Units";
        //check if unit holder already exists & delete if it does
        if (transform.Find(unitHolderName))
        {
            DestroyImmediate(transform.Find(unitHolderName).gameObject);
        }
        //create new unitholder
        Transform unitHolder = new GameObject(unitHolderName).transform;
        unitHolder.parent = transform;

        grid.PopulateGrid();
        
        //loop over each grid position
        for (int x = 0; x < Grid.GridWidth; x++) {
            for (int y = 0; y < Grid.GridHeight; y++) {
                //instantiate new tile at position
                Vector3 tilePosition = new Vector3(x + Grid.CellOffset, Grid.MapHeight,y + Grid.CellOffset);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as Transform;
                newTile.parent = mapHolder;
                //add tile to tile array
                tiles[x, y] = newTile;
                
                //check if there is a unit to spawn at this location
                if (grid.GetGridArray(new Coords(x, y)) != null)
                {
                    //instantiate new unit
                    Transform newObject = Instantiate(grid.GetGridArray(new Coords(x, y)).transform, tilePosition, Quaternion.identity);
                    newObject.parent = unitHolder;
                    //add new object to grid (replacing the prefab that is currently there)
                    grid.SetGridArray(new Coords(x, y), newObject.gameObject);
                    
                    //check new unit's tag and add it's unitcontroller to correct army list
                    if (newObject.CompareTag("Friendly Unit"))
                        unitManager.playerArmy.Add(newObject.GetComponent<UnitController>());
                    else if (newObject.CompareTag("Enemy Unit"))
                        unitManager.enemyArmy.Add(newObject.GetComponent<UnitController>());
                }
            }
        }
    }

    /// <summary>
    /// get groundcontroller of tile at coordinates
    /// </summary>
    /// <param name="coords">coordinates of tile</param>
    /// <returns>groundcontroller script of tile</returns>
    public GroundController GetTile(Coords coords)
    {
        return tiles[coords.X, coords.Y].GetComponent<GroundController>();
    }
}