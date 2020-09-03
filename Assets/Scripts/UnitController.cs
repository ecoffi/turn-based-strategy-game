using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Grid = Utils.Grid;

//manage an individual unit
public class UnitController : MonoBehaviour
{
    private bool _hasAction = true; //whether or not the unit can move and attack this turn
    [SerializeField] private int maxHealth; //the unit's maximum health
    private int _currentHealth; //current health of unit
    [SerializeField] private int moveRange; //number of spaces unit can move in a turn
    [SerializeField] private int attackRange; //how far away this unit can attack
    [SerializeField] private int attackDamage;
    private Grid _grid; //the grid
    private MapGenerator _mapGenerator;
    private Animator _animator; //animator component
    public Sprite blueCircle;
    public Sprite whiteCircle;
    private SpriteRenderer _footCircle; //sprite renderer of selection circle
    private Image _healthBar;
    private readonly List<GroundController> _highlightedTiles = new List<GroundController>(); //the list of highlighted tiles by this unit
    public Transform bodyTransform; //the transform of container of the unit's body & head
    
    private void Start()
    {
        //get scripts and gameobjects
        _grid = FindObjectOfType<Grid>();
        _mapGenerator = FindObjectOfType<MapGenerator>();
        _footCircle = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        //get health bar sprite
        Image[] images = GetComponentsInChildren<Image>();
        foreach (var image in images)
            if (image.name == "Health Bar")
                _healthBar = image;

        //unit starts at max health
        _currentHealth = maxHealth;
        
        //rotate towards down on y axis
        RotateTowards(Direction.Down);
    }

    public bool GetHasAction()
    {
        return _hasAction;
    }
    
    public void SetHasAction(bool hasAction)
    {
        _footCircle.enabled = _hasAction = hasAction;
    }

    public int GetAttackRange()
    {
        return attackRange;
    }
    
    public int GetMoveRange()
    {
        return moveRange;
    }

    /// <summary>
    /// check if this unit is alive or not
    /// </summary>
    /// <returns>true if unit is alive, false if dead</returns>
    public bool IsAlive()
    {
        return _currentHealth > 0;
    }

    /// <summary>
    /// Mark this unit as "selected" by player
    /// </summary>
    public void Select()
    {
        _footCircle.sprite = whiteCircle;
        HighlightMoveableSpaces();
    }

    /// <summary>
    /// Player "deselected" this unit
    /// </summary>
    public void Deselect()
    {
        _footCircle.sprite = blueCircle;
        UnhighlightTiles();
    }

    /// <summary>
    /// Highlight all spaces moveable & attackable by this unit
    /// </summary>
    private void HighlightMoveableSpaces()
    {
        Coords unitCoords = GetUnitCoords();

        //loop over all squares unit could move to or attack and highlight them
        for (int x = 0; x < moveRange + 1; x++)
        {
            for (int y = 0; y < moveRange + 1 - x; y++)
            {
                //add top right spaces
                CheckAndHighlightMoveableTile(new Coords(unitCoords.X + x, unitCoords.Y + y));
                
                //top left - make sure we aren't double-adding any
                if (x != 0)
                    CheckAndHighlightMoveableTile(new Coords(unitCoords.X - x, unitCoords.Y + y));

                //bottom spaces
                if (y != 0)
                {
                    CheckAndHighlightMoveableTile(new Coords(unitCoords.X + x, unitCoords.Y - y));
                    if (x != 0)
                        CheckAndHighlightMoveableTile(new Coords(unitCoords.X - x, unitCoords.Y - y));
                }
            }
        }
    }

    /// <summary>
    /// Check if a tile should be highlighted and highlight it if so
    /// Only use on moveable tiles, uses a bfs search to get around corners and obstacles.
    /// </summary>
    /// <param name="tileCoords">coordinates of tile</param>
    private void CheckAndHighlightMoveableTile(Coords tileCoords)
    {
        BFS bfs = new BFS();

        if (HasPath(tileCoords, ref bfs, moveRange))
        {
            GroundController tile = _mapGenerator.GetTile(tileCoords);
            tile.SetHighlight(GroundController.HighlightType.Moveable);

            //add to highlighted tiles list
            _highlightedTiles.Add(tile);
        }
            
    }

    /// <summary>
    /// Highlight tiles attackable by this unit.
    /// Used after move, so only highlight tiles that can immediately be attacked
    /// </summary>
    public void HighlightAttackableSpaces()
    {
        Coords unitCoords = GetUnitCoords();
        
        //store whether unit is blocked in cardinal direction
        bool upFree = true;
        bool rightFree = true;
        bool downFree = true;
        bool leftFree = true;
        
        for (int i = 0; i < attackRange; i++)
        {
            //if up isn't already blocked, check and higlight up space
            if (upFree)
                upFree = CheckAndHighlightAttackableTile(new Coords(unitCoords.X, unitCoords.Y + i + 1));
            //right
            if (rightFree)
                rightFree = CheckAndHighlightAttackableTile(new Coords(unitCoords.X + i + 1, unitCoords.Y));
            //down
            if (downFree)
                downFree = CheckAndHighlightAttackableTile(new Coords(unitCoords.X, unitCoords.Y - i - 1));
            //left
            if (leftFree)
                leftFree = CheckAndHighlightAttackableTile(new Coords(unitCoords.X - i - 1, unitCoords.Y));
        }
    }

    /// <summary>
    /// check whether a tile should be highlighted as attackable
    /// </summary>
    /// <param name="tileCoords"></param>
    /// <returns>true if it tile was highlighted, false if it was blocked</returns>
    private bool CheckAndHighlightAttackableTile(Coords tileCoords)
    {
        if (!Grid.InMap(tileCoords))
            return false;
        
        bool isFree = _grid.IsFree(tileCoords);
        
        //highlight if tile is free or if isn't free but has enemy
        if (isFree || _grid.IsEnemy(tileCoords))
        {
            GroundController tile = _mapGenerator.GetTile(tileCoords);
            tile.SetHighlight(GroundController.HighlightType.Attackable);

            //add to highlighted tiles list
            _highlightedTiles.Add(tile);
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Check unit's path to destination by solving the BFS
    /// </summary>
    /// <param name="destination">the coordinates of the destination</param>
    /// <param name="bfs">the bfs search object</param>
    /// <param name="range">the range in spaces to check</param>
    /// <returns>true if available path, false otherwise</returns>
    public bool HasPath(Coords destination, ref BFS bfs, int range = Int32.MaxValue)
    {
        //if destination is out of bounds, no path
        if (destination.IsOutOfBounds())
            return false;
        
        bfs.Solve(_grid, GetUnitCoords(), destination, range);

        return bfs.GetFoundSolution();
    }

    /// <summary>
    /// Move the unit to destination grid position
    /// </summary>
    /// <param name="destination">coords to move to</param>
    /// <returns>true if move successful, false otherwise</returns>
    public bool Move(Coords destination)
    {
        //unhighlight the move tiles when moving
        UnhighlightTiles();

        //create a new Breadth-first search object
        BFS bfs = new BFS();
        
        //check is unit has no path to destination
        if (!HasPath(destination, ref bfs, moveRange))
            return false;
        
        //set new position
        transform.position = Grid.CoordsToWorldSpace(destination);
        
        return true;
    }

    /// <summary>
    /// Check whether coordinates are within attack range of this unit, no direct path necessary
    /// </summary>
    /// <param name="enemyCoords">the enemy coords</param>
    /// <returns>true if within range, false otherwise</returns>
    private bool IsWithinAttackRange(Coords enemyCoords)
    {
        return Grid.Distance(GetUnitCoords(), enemyCoords) <= attackRange;
    }

    /// <summary>
    /// Check if unit has a direct, unobstructed path in a straight line to target 
    /// </summary>
    /// <param name="target">coordinates to check path to</param>
    /// <returns>true if path exists, false otherwise</returns>
    public bool HasDirectPath(Coords target)
    {
        Coords unitCoords = GetUnitCoords();
        
        //get the direction unit would have to look to see target
        Direction facing = DirectionMethods.GetOrientation(unitCoords, target);
        
        if (facing == Direction.None)
            return false; //there is no direct path if target doesn't share an axis
        
        //move along spaces in direction and make sure they're free

        switch (facing)
        {
            case Direction.Right:
            {
                //target is right of unit
                for (int x = unitCoords.X + 1; x < target.X; x++)
                {
                    //check if grid isn't free
                    if (!_grid.IsFree(new Coords(x, unitCoords.Y)))
                        return false;
                }

                break;
            }
            case Direction.Left:
            {
                //target is left of unit
                for (int x = unitCoords.X - 1; x > target.X; x--)
                {
                    //check if grid isn't free
                    if (!_grid.IsFree(new Coords(x, unitCoords.Y)))
                        return false;
                }

                break;
            }
            case Direction.Up:
            {
                //target is above unit
                for (int y = unitCoords.Y + 1; y < target.Y; y++)
                {
                    //check if grid isn't free
                    if (!_grid.IsFree(new Coords(unitCoords.X, y)))
                        return false;
                }

                break;
            }
            case Direction.Down:
            {
                //target is below unit
                for (int y = unitCoords.Y - 1; y > target.Y; y--)
                {
                    //check if grid isn't free
                    if (!_grid.IsFree(new Coords(unitCoords.X, y)))
                        return false;
                }

                break;
            }
            default:
                //facing is Direction.None
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// Get the coordinates of this unit on a grid
    /// </summary>
    /// <returns>the coordinates of this unit</returns>
    public Coords GetUnitCoords()
    {
        return Grid.WorldSpaceToCoords(transform.position);
    }

    /// <summary>
    /// take damage and check if dead
    /// </summary>
    /// <param name="damage">the amount of damage taken</param>
    private void TakeDamage(int damage)
    {
        //subtract damage from health
        _currentHealth -= damage;

        //update healthbar
        _healthBar.fillAmount = (float) _currentHealth / maxHealth;
        
        if (IsAlive())
            _animator.SetTrigger("Take Damage");
    }
    
    /// <summary>
    /// trigger this unit's death
    /// </summary>
    public void Die()
    {
        StartCoroutine(Death());
    }

    /// <summary>
    /// this unit animates a death and then destroys itself
    /// </summary>
    private IEnumerator Death()
    {
        _animator.SetTrigger("Die");
        yield return new WaitForSeconds(1.5f);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Check if the unit can attack an enemy unit
    /// </summary>
    /// <param name="enemyUnitCoords">coordinates of enemy unit</param>
    /// <returns>true if unit can attack, false otherwise</returns>
    public bool CanAttack(Coords enemyUnitCoords)
    {
        //check if enemy is within range and has path
        return IsWithinAttackRange(enemyUnitCoords) && HasDirectPath(enemyUnitCoords);
    }

    /// <summary>
    /// Attack an enemy unit
    /// </summary>
    /// <param name="enemyUnit">unitcontroller of enemy unit</param>
    public void Attack(UnitController enemyUnit)
    {
        //rotate towards enemy
        Direction directionToEnemy = DirectionMethods.GetOrientation(GetUnitCoords(), enemyUnit.GetUnitCoords());
        RotateTowards(directionToEnemy);
        
        //trigger attack animation
        _animator.SetTrigger("Attack");
        
        //deal damage to enemy
        enemyUnit.TakeDamage(attackDamage);
        
        //rotate back
        StartCoroutine(ResetRotation());
    }

    /// <summary>
    /// reset rotation to down after wait for seconds
    /// </summary>
    private IEnumerator ResetRotation()
    {
        yield return new WaitForSeconds(1.3f);
        RotateTowards(Direction.Down);
    }

    /// <summary>
    /// Rotate the unit's body towards a direction
    /// </summary>
    /// <param name="direction">the direction to rotate towards</param>
    private void RotateTowards(Direction direction)
    {
        int rotation = DirectionMethods.GetAngle(direction);
        
        bodyTransform.rotation = Quaternion.AngleAxis(rotation, transform.up);
    }

    /// <summary>
    /// Unhighlight the tiles highlighted by this unit
    /// </summary>
    public void UnhighlightTiles()
    {
        //reset each tile's highlight
        foreach (var tile in _highlightedTiles)
        {
            tile.SetHighlight(GroundController.HighlightType.None);
        }
        //clear the list
        _highlightedTiles.Clear();
    }
}
