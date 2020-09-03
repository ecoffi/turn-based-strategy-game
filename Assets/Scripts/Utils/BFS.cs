using System.Collections.Generic;
using UnityEngine.Rendering;
using Color = Utils.Cell.Color;

namespace Utils
{
    //searches for the goal, visiting each accessible cell before visiting the ones those cells can access 
    public class BFS
    {
        // records where we've been and what steps we've taken.
        private Cell[,] _memory;

        // for tracking the "traversable" solution.
        private Coords _solution;
        private bool _foundSolution;
        private int _solutionLength;

        // the terrain we're searching in.
        private Grid _terrain;
        private Coords _start;
        private Coords _destination;

        /// <summary>
        /// create a new bfs search
        /// </summary>
        public BFS()
        {
            _foundSolution = false;
        }

        /// <summary>
        /// Solve the given Grid using the BFS method
        /// </summary>
        /// <param name="grid">grid object to solve</param>
        /// <param name="start">starting coordinates</param>
        /// <param name="destination">destination coordinates</param>
        /// <param name="range">the number of spaces allowed to move</param>
        public void Solve(Grid grid, Coords start, Coords destination, int range)
        {
            _terrain = grid;
            _start = start;
            _destination = destination;
            const int xLength = Grid.GridWidth;
            const int yLength = Grid.GridHeight;
            
            //check if destination NOT reachable
            if (!Grid.InMap(_destination) && !_terrain.IsFree(_destination))
            {
                //if destination isn't free, there's no solution
                _foundSolution = false;
                return;
            }
            
            // track locations we've been to using our terrain "memory"
            Cell defaultCell = new Cell(Color.White);
            _memory = new Cell[xLength, yLength];
            //Fill the memory with default cells
            for (int x = 0; x < xLength; x++)
            {
                for (int y = 0; y < yLength; y++)
                {
                    _memory[x, y] = defaultCell.Copy();
                }
            }

            Queue<Coords> toCheck = new Queue<Coords>(); //holds the spaces to check. Spaces in queue will be grey

            // track the current search location, starting at the start of queue
            Coords current = _start;

            while(!current.Equals(_destination)) //search have not reached goal yet
            {
                // record that we've been here
                _memory[current.X, current.Y].SetColor(Color.Black);

                // find the next direction. Check Up, down, left, right for white space
                foreach (Direction directionToCheck in DirectionMethods.GetClockwise()) //loop over each direction in clockwise order
                {
                    Coords next = current.Get(directionToCheck); //next is the location of the next square to check
                    if (Grid.InMap(next) && _terrain.IsFree(next) && _memory[next.X, next.Y].GetColor() == Color.White) //part of terrain and free space and not visited
                    {
                        //next space is white. record it as grey and add to queue,
                        //then go back to current and continue looking at other directions

                        //set from direction of next, to backtrack from goal
                        _memory[next.X, next.Y].SetFrom(DirectionMethods.Opposite(directionToCheck));

                        //set color to grey(seen)
                        _memory[next.X, next.Y].SetColor(Color.Grey);
                        
                        //set the distance to distance of current + 1
                        _memory[next.X, next.Y].SetDistance(_memory[current.X, current.Y].GetDistance() + 1);

                        //check if next is within range
                        if (_memory[next.X, next.Y].GetDistance() <= range)
                        {
                            //add next location to queue
                            toCheck.Enqueue(next);
                        }
                    }
                }

                //if queue is empty, BFS has searched every path and found no solution
                if (toCheck.Count == 0)
                {
                    _foundSolution = false;
                    return;
                }

                //move to next spot in queue
                current = toCheck.Dequeue();
            }

            // we reached the goal and have a solution.
            _solutionLength = _memory[current.X, current.Y].GetDistance();
            _foundSolution = true;

            // Get the solution in reverse and set the To of each location

            current = _destination; // start at the goal

            Coords previous = current; //last current
            current = current.Get(_memory[current.X, current.Y].GetFrom()); //move to where we came here from

            while (!current.Equals(_start)) { //loop until we reach the goal

                //set from direction of new current
                Direction from = DirectionMethods.Opposite(_memory[previous.X, previous.Y].GetFrom());
                _memory[current.X, current.Y].SetTo(from); //set current To as opposite of previous From

                previous = current;

                // step to where we came to current from
                current = current.Get(_memory[current.X, current.Y].GetFrom());
            }

            //set to one last time for the starting spot
            Direction fromLast = DirectionMethods.Opposite(_memory[previous.X, previous.Y].GetFrom());
            _memory[current.X, current.Y].SetTo(fromLast); //set to as opposite of previous from
        }

        /// <summary>
        /// Check if BFS has found a solution
        /// </summary>
        /// <returns>true if BFS found a solution, false otherwise</returns>
        public bool GetFoundSolution()
        {
            return _foundSolution;
        }

        public int GetSolutionLength()
        {
            return _solutionLength;
        }

        /// <summary>
        /// Reset the BFS solution to beginning
        /// </summary>
        public void Reset()
        {
            // start the traversal of our path at the terrain's start.
            _solution = _start;
        }
        
        /// <summary>
        /// Take one step towards solution
        /// </summary>
        /// <returns>the direction moved in</returns>
        public Direction Next()
        {
            // recall the direction at this location, move to the corresponding location and return it.
            Direction direction = _memory[_solution.X, _solution.Y].GetTo();
            _solution = _solution.Get(direction);
            return direction;
        }

        /// <summary>
        /// Check if solution traversal has completed
        /// </summary>
        /// <returns>true if traversal hasn't reached end, false otherwise</returns>
        public bool HasNext()
        {
            // we're only done when we get to the terrain goal.
            return !_solution.Equals(_destination);
        }
    }
}
