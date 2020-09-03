using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Grid = Utils.Grid;

//manage the units of the map
public class UnitManager : MonoBehaviour
{
    //The Game State
    private enum State
    {
        Busy, //the computer is working on something
        SelectAndMove, //player is selecting and moving a unit
        Attacking, //the selected unit has moved and is now selecting an opponent to attack
        Waiting, //waiting for animation or event to occur/finish
    }

    private State _currentState; //the current state of the turn
    private UnitController _selectedUnit; //unitcontroller of selected unit
    private Camera _currentCamera; //the camera in use

    public List<UnitController> playerArmy = new List<UnitController>(); //list containing all player unitcontrollers
    public List<UnitController> enemyArmy = new List<UnitController>(); //list containing all enemy unitcontrollers
    public Grid grid; //the grid script
    public GameObject endTurnButton; //the "end turn" button
    public GameObject noAttackButton; //the "no attack" button
    private GameManager _gameManager;

    //called at start
    private void Start()
    {
        _currentCamera = Camera.main;
        _gameManager = FindObjectOfType<GameManager>();
        StartPlayerTurn();
    }

    //called every frame
    private void Update()
    {
        //branch to state update functions
        switch (_currentState)
        {
            case State.Attacking:
                AttackingUpdate();
                break;
            case State.SelectAndMove:
                SelectAndMoveUpdate();
                break;
        }
    }

    /// <summary>
    /// Update function when state is select and move
    /// </summary>
    private void SelectAndMoveUpdate()
    {
        //SELECT
        //check if left click was pressed
        if (Input.GetButtonDown("Select"))
        {
            //get collider clicked on
            Collider clickedOnCollider = ClickOnCollider();
            
            //check if player clicked on collider
            if (clickedOnCollider != null)
            {
                //check if clicked on friendly unit AND unit has action
                if (clickedOnCollider.gameObject.CompareTag("Friendly Unit"))
                {
                    //get unitcontroller clicked on
                    UnitController unitController = clickedOnCollider.gameObject.GetComponent<UnitController>();
                    
                    //check if unit has action or not. If it has no action, don't select anything
                    if (!unitController.GetHasAction())
                        return;
                    
                    //select unit clicked on
                    SelectUnit(unitController);
                }
                //clicked on ground
                else
                {
                    //unset selected unit
                    DeselectUnit();
                }
            }
        }

        //MOVE
        //check if right click was pressed and there is a selected unit
        else if (Input.GetButtonDown("Alt Select") && _selectedUnit != null)
        {
            //get collider clicked on
            Collider clickedOnCollider = ClickOnCollider();
            
            //check if player clicked on collider
            if (clickedOnCollider != null)
            {
                Coords oldCoords = _selectedUnit.GetUnitCoords(); //the current coords of selected unit
                Coords toCoords = Grid.WorldSpaceToCoords(clickedOnCollider.transform.position); //the coordinates player clicked on (to move to)
                
                //check if grid space clicked is selectedunit's own space. Skip movement in that case
                if (grid.GetGridArray(toCoords) == _selectedUnit.gameObject)
                {
                    _selectedUnit.UnhighlightTiles();
                    SetStateAttacking();
                }
                
                //check and move unit
                else if (_selectedUnit.Move(toCoords))
                {
                    //move was successful, update grid
                    grid.SetGridArray(toCoords, _selectedUnit.gameObject);
                    grid.SetGridArray(oldCoords, null);
                    
                    //state becomes attacking(with selectedunit)
                    SetStateAttacking();
                }
                else
                    //move failed
                    DeselectUnit();
            }
        }
    }

    /// <summary>
    /// Update function when state is attacking
    /// </summary>
    private void AttackingUpdate()
    {
        //check if enemy unit is clicked on
        if (Input.GetButtonDown("Select") || Input.GetButtonDown("Alt Select"))
        {
            //get collider clicked on
            Collider clickedOnCollider = ClickOnCollider();

            //check if it is an enemy unit
            if (clickedOnCollider.CompareTag("Enemy Unit"))
            {
                UnitController enemyUnit = clickedOnCollider.gameObject.GetComponent<UnitController>();
                if (AttackUnit(_selectedUnit, enemyUnit))
                    SetStateSelectAndMove();
            }
        }
    }

    /// <summary>
    /// Perform an Enemy Turn
    /// </summary>
    private void EnemyTurn()
    {
        //loop over each enemy unit
        foreach (var enemyUnitController in enemyArmy)
        {
            //find nearest friendly unit
            UnitController targetFriendlyUnit = FindNearestFriendlyUnit(enemyUnitController);
            
            //check if enemy is NOT within attacking distance
            if (!enemyUnitController.CanAttack(targetFriendlyUnit.GetUnitCoords()))
                //move closer to friendly unit
                MoveTowardsUnit(enemyUnitController, targetFriendlyUnit);
            
            //attempt attack
            AttackUnit(enemyUnitController, targetFriendlyUnit);
        }
        
        //switch back to player turn
        StartPlayerTurn();
    }

    /// <summary>
    /// Find the friendly unit nearest to a given enemy unit
    /// </summary>
    /// <param name="enemyUnitController">unitcontroller of enemy unit</param>
    /// <returns>unitcontroller of nearest friendly unit</returns>
    private UnitController FindNearestFriendlyUnit(UnitController enemyUnitController)
    {
        //loop over each friendly unit and check for the shortest distance
        
        int shortestDistance = Int32.MaxValue; //distance to unit with shortest distance
        UnitController shortestDistanceUnitController = null; //unitcontroller of unit with shortest distance
        foreach (var friendlyUnitController in playerArmy)
        {
            Coords friendlyUnitCoords = friendlyUnitController.GetUnitCoords();
            
            if (enemyUnitController.CanAttack(friendlyUnitCoords))
            {
                //if enemy unit can attack friendly, return the attackable unit
                return friendlyUnitController;
            }
            
            foreach (var direction in DirectionMethods.GetClockwise())
            {
                BFS bfs = new BFS();
                Coords coordsToCheck = friendlyUnitCoords.Get(direction); //coords right next to unit in direction
                
                //check if enemy unit has path to friendly unit
                if (enemyUnitController.HasPath(coordsToCheck, ref bfs))
                {
                    //get length of solution and check if shorter than the previous shortest distance
                    int solutionLength = bfs.GetSolutionLength();
                    if (solutionLength < shortestDistance)
                    {
                        shortestDistance = solutionLength;
                        shortestDistanceUnitController = friendlyUnitController;
                    }
                }
            }
        }

        if (shortestDistanceUnitController == null)
            //there is no path to any friendly unit
            //just select the first enemy 
            shortestDistanceUnitController = playerArmy[0];
        
        return shortestDistanceUnitController;
    }

    /// <summary>
    /// Check if attacking unit can attack another unit and then attack with attacking unit
    /// </summary>
    /// <param name="attackingUnit">unitcontroller of attacking unit</param>
    /// <param name="defendingUnit">unitcontroller of unit being attacked</param>
    /// <returns>true if attack succeeded, false if defending unit was out of range</returns>
    private bool AttackUnit(UnitController attackingUnit, UnitController defendingUnit)
    {
        Coords enemyUnitCoords = defendingUnit.GetUnitCoords();
                
        //check if enemy is within range and has path
        if (attackingUnit.CanAttack(enemyUnitCoords))
        {
            attackingUnit.Attack(defendingUnit);
                    
            //check if enemy unit is dead
            if (!defendingUnit.IsAlive())
            {
                //replace this gameobject in grid with null
                grid.SetGridArray(enemyUnitCoords, null);
                        
                //remove from army list - check which army it is in
                if (defendingUnit.CompareTag("Enemy Unit"))
                    enemyArmy.Remove(defendingUnit);
                else
                    playerArmy.Remove(defendingUnit);

                defendingUnit.Die();

                StartCoroutine(CheckForVictor());
            }
            
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Determine the best move towards attacking range of unit
    /// </summary>
    /// <param name="movingUnitController">the unit that will be moving</param>
    /// <param name="targetUnitController">the unit to move towards</param>
    private void MoveTowardsUnit(UnitController movingUnitController, UnitController targetUnitController)
    {
        Transform movingUnitTransform = movingUnitController.GetComponent<Transform>(); //transform of moving unit
        Coords movingUnitCoords = movingUnitController.GetUnitCoords();
        Coords targetUnitCoords = targetUnitController.GetUnitCoords();
        int attackRange = movingUnitController.GetAttackRange();
        int moveRange = movingUnitController.GetMoveRange();
        
        //Determine best coordinates to move to to be within attack range
        //we will get a couple of coords and then check to see which one is shortest distance from moving unit
        
        //check the spaces next to target for best space eligibility
        List<Coords> coordsToCheck = new List<Coords>(); //a list of coords to check for best space eligibility
        
        //look at spaces left of target
        //move from furthest space attackable from to closer to target unit
        for (int x = targetUnitCoords.X - attackRange; x < targetUnitCoords.X; x+=2) //increment by two because if a space isn't good then the next one won't be either
        {
            Coords testCoords = new Coords(x, targetUnitCoords.Y);
            if (targetUnitController.HasDirectPath(testCoords))
                //coords close to target and has direct path, add them to list of coords to check
                coordsToCheck.Add(testCoords);
        }
        //spaces right of target
        for (int x = targetUnitCoords.X + attackRange; x > targetUnitCoords.X; x-=2)
        {
            Coords testCoords = new Coords(x, targetUnitCoords.Y);
            if (targetUnitController.HasDirectPath(testCoords))
                //coords close to target and has direct path, add them to list of coords to check
                coordsToCheck.Add(testCoords);
        }
        //spaces above target
        for (int y = targetUnitCoords.Y + attackRange; y > targetUnitCoords.Y; y-=2)
        {
            Coords testCoords = new Coords(targetUnitCoords.X, y);
            if (targetUnitController.HasDirectPath(testCoords))
                //coords close to target and has direct path, add them to list of coords to check
                coordsToCheck.Add(testCoords);
        }
        //spaces below target
        for (int y = targetUnitCoords.Y - attackRange; y < targetUnitCoords.Y; y+=2)
        {
            Coords testCoords = new Coords(targetUnitCoords.X, y);
            if (targetUnitController.HasDirectPath(testCoords))
                //coords close to target and has direct path, add them to list of coords to check
                coordsToCheck.Add(testCoords);
        }
        
        //evaluate coords that were added to list
        //look for the one with the shortest path from moving unit
        int shortestPathLength = int.MaxValue; //the length of the path to the coords that have the shortest path from moving to target
        BFS shortestPathBFS = new BFS(); //the bfs of the shortest path to coords
        
        foreach (var coords in coordsToCheck)
        {
            BFS bfs = new BFS();
            if (movingUnitController.HasPath(coords, ref bfs))
            {
                int solutionLength = bfs.GetSolutionLength();
                if (solutionLength < shortestPathLength)
                {
                    shortestPathLength = solutionLength;
                    shortestPathBFS = bfs;
                }
            }
        }
            
        //Move (partially or fully)
        //traverse through bfs
        int stepsTaken = 0; //the number of space moved already
        Coords coordsToMoveTo = movingUnitCoords; //the coords to move to (within moveRange)
        shortestPathBFS.Reset();
        
        while (stepsTaken < moveRange && shortestPathBFS.HasNext())
        {
            //step along path to destination
            coordsToMoveTo = coordsToMoveTo.Get(shortestPathBFS.Next());
            
            stepsTaken++; //unit has moved 1 space
        }
        
        //set new position for moving unit
        movingUnitTransform.position = Grid.CoordsToWorldSpace(coordsToMoveTo);
        
        //update unit location on grid
        grid.SetGridArray(coordsToMoveTo, movingUnitController.gameObject);
        grid.SetGridArray(movingUnitCoords, null);
    }
    
    /// <summary>
    /// Check to see if player or enemy has won the game
    /// If they have, wait for seconds and then go to winner menu
    /// </summary>
    private IEnumerator CheckForVictor()
    {
        //set playerWon
        if (enemyArmy.Count == 0)
            _gameManager.SetPlayerWon(true);
        else if (playerArmy.Count == 0)
            _gameManager.SetPlayerWon(false);
        else
            //each army has at least 1 unit
            yield break;
        
        yield return new WaitForSeconds(1.5f);
        GameManager.LoadScene("WinnerMenu");
    }

    /// <summary>
    /// change state to select and move
    /// </summary>
    public void SetStateSelectAndMove()
    {
        //switch the no attack button to end turn
        endTurnButton.SetActive(true);
        noAttackButton.SetActive(false);

        if (_selectedUnit != null)
        {
            _selectedUnit.SetHasAction(false);
            DeselectUnit();
        }

        _currentState = State.SelectAndMove;
    }

    /// <summary>
    /// change current state to attacking
    /// </summary>
    private void SetStateAttacking()
    {
        //switch the end turn button to no attack
        endTurnButton.SetActive(false);
        noAttackButton.SetActive(true);
        
        _selectedUnit.HighlightAttackableSpaces();
        
        _currentState = State.Attacking;
    }
    
    /// <summary>
    /// End the player's turn
    /// </summary>
    public void EndPlayerTurn()
    {
        //disable end turn button & set the state to enemy turn
        endTurnButton.SetActive(false);
        _currentState = State.Busy;

        foreach (var unit in playerArmy)
        {
            //remove each unit's action
            unit.SetHasAction(false);
        }
        
        EnemyTurn();
    }

    /// <summary>
    /// Start the player's turn
    /// </summary>
    public void StartPlayerTurn()
    {
        foreach (var unit in playerArmy)
        {
            //give each unit their action
            unit.SetHasAction(true);
        }

        SetStateSelectAndMove();
    }

    /// <summary>
    /// Set a unit as the currently selected unit
    /// </summary>
    /// <param name="unitController">The unitcontroller to select</param>
    private void SelectUnit(UnitController unitController)
    {
        //deselect the current unit controller if necessary
        if (_selectedUnit != null)
            _selectedUnit.Deselect();
        
        _selectedUnit = unitController;
        _selectedUnit.Select();
    }

    /// <summary>
    /// Unset the selected unit
    /// </summary>
    private void DeselectUnit()
    {
        _selectedUnit.Deselect();
        _selectedUnit = null;
    }

    /// <summary>
    /// raycast to screen and get the collider under the mouse cursor 
    /// </summary>
    /// <returns>the collider clicked on (null if none)</returns>
    private Collider ClickOnCollider()
    {
        RaycastHit hit; //the raycasthit caused by click
                
        //check if player clicked on collider
        if (ClickOnScreen(out hit))
            return hit.collider;
        else
            //clicked on nothing
            return null;
    }
    
    /// <summary>
    /// Raycast hit to screen
    /// </summary>
    /// <param name="hit">the raycasthit that is sent back</param>
    /// <returns>true if raycast is successful, false otherwise</returns>
    private bool ClickOnScreen(out RaycastHit hit)
    {
        //ray to place clicked on
        Ray ray = _currentCamera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit);
    }
}
