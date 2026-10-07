using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GA.Collections;
using Cell = GA.Ships.Pathfinding.NavigationGrid.Cell;

namespace GA.Ships.Pathfinding
{
	public class Pathfinder
	{
		private NavigationGrid _grid = null;

		// Cells, which will be inspected.
		private PriorityQueue<Cell> _frontier = new PriorityQueue<Cell>();

		// Cells which has been inspected already.
		private HashSet<Cell> _visited = new HashSet<Cell>();

		public Pathfinder(NavigationGrid grid)
		{
			_grid = grid;
		}

		#region Breadth-First Search
		/// <summary>
		/// Performs a breadth-first search to find a path from the start position to the end position.
		/// Link: https://en.wikipedia.org/wiki/Breadth-first_search
		/// </summary>
		/// <param name="startPosition">Start position</param>
		/// <param name="endPosition">End position</param>
		/// <returns>List of positions representing the path, or null if no path is found</returns>
		public IList<Vector3> BreadthFirstSearch(Vector3 startPosition, Vector3 endPosition)
		{
			Queue<Cell> frontier = new Queue<Cell>();
			Dictionary<Cell, Cell> cameFrom = new Dictionary<Cell, Cell>();

			Cell startCell = _grid.GetCell(startPosition);
			Cell endCell = _grid.GetCell(endPosition);

			frontier.Enqueue(startCell);
			cameFrom[startCell] = null;

			bool isEndReached = false; // Early exit flag

			while (frontier.Count > 0)
			{
				Cell current = frontier.Dequeue();

				isEndReached = current == endCell;
				if (isEndReached)
				{
					// The end node is reached. Path is complete.
					break;
				}

				IList<Cell> neighbours = _grid.GetNeighbours(current, includeDiagonal: false);
				foreach (Cell neighbour in neighbours)
				{
					if (neighbour.IsWalkable && !cameFrom.ContainsKey(neighbour))
					{
						frontier.Enqueue(neighbour);
						cameFrom[neighbour] = current;
					}
				}
			}

			// If isEndReached is false here, there is no path to the end cell.
			if (isEndReached)
			{
				// Construct path
				return ConstructPath(startCell, endCell, cameFrom);
			}

			// There is no path between start and end positions.
			return null;
		}
		
				/// <summary>
		/// Finds every cell that can be reached from <paramref name="start"/> in at most
		/// <paramref name="maxSteps"/> steps within the movement range flood-fill.
		/// Uses breadth-first search, so only axial movement is supported and
		/// every step counts as one, regardless of the cell cost.
		/// Non-walkable cells are never entered, so cells behind them are only included if
		/// they can be reached around the obstacle within the step limit.
		/// </summary>
		/// <param name="start">The cell where the movement starts. Must be walkable.</param>
		/// <param name="maxSteps">
		/// The maximum number of steps to take. The start cell is not counted as a step.
		/// </param>
		/// <returns>
		/// The reachable cells ordered by distance from the start (the nearest first).
		/// The <paramref name="start"/> cell itself is not included, so 0 steps returns an empty list.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="start"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="maxSteps"/> is negative.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="start"/> is outside the navigation grid or is not walkable.
		/// </exception>
		public IList<Cell> GetReachableCells(Cell start, int maxSteps)
		{
			if (start == null)
			{
				throw new ArgumentNullException(nameof(start));
			}

			if (maxSteps < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxSteps), maxSteps, "The step count cannot be negative.");
			}

			if (start.X < 0 || start.Y < 0 || start.X >= _grid.Width || start.Y >= _grid.Height)
			{
				throw new ArgumentException("The start cell is not inside the navigation grid.", nameof(start));
			}

			if (!start.IsWalkable)
			{
				throw new ArgumentException("The start cell is not walkable.", nameof(start));
			}

			Queue<Cell> frontier = new Queue<Cell>();

			// Steps taken from the start to each discovered cell. Also works as the visited set.
			Dictionary<Cell, int> steps = new Dictionary<Cell, int>();
			IList<Cell> reachable = new List<Cell>();

			frontier.Enqueue(start);
			steps[start] = 0;

			while (frontier.Count > 0)
			{
				Cell current = frontier.Dequeue();

				int stepsToCurrent = steps[current];
				if (stepsToCurrent == maxSteps)
				{
					// The step limit is used up. Do not expand any further from here.
					continue;
				}

				IList<Cell> neighbours = _grid.GetNeighbours(current, includeDiagonal: false);
				foreach (Cell neighbour in neighbours)
				{
					// Non-walkable cells are never enqueued, so the search cannot pass through them.
					if (neighbour.IsWalkable && !steps.ContainsKey(neighbour))
					{
						steps[neighbour] = stepsToCurrent + 1;
						frontier.Enqueue(neighbour);
						reachable.Add(neighbour);
					}
				}
			}

			return reachable;
		}

		public IList<Cell> GetReachableCells(Cell start, int maxSteps, bool includeDiagonal)
		{
			Queue<Cell> frontier = new Queue<Cell>();
			Dictionary<Cell, (Cell, int)> cameFrom = new Dictionary<Cell, (Cell, int)>();

			frontier.Enqueue(start);
			cameFrom[start] = (null, 0);

			bool isEndReached = false;

			while (frontier.Count > 0)
			{
				Cell current = frontier.Dequeue();
				int distance = cameFrom[current].Item2;

				isEndReached = distance >= maxSteps;
				if (isEndReached)
				{
					// Max. distance is reached. Path is complete.
					break;
				}

				IList<Cell> neighbours = _grid.GetNeighbours(current, includeDiagonal: includeDiagonal);
				foreach (Cell neighbour in neighbours)
				{
					if (neighbour.IsWalkable && !cameFrom.ContainsKey(neighbour))
					{
						frontier.Enqueue(neighbour);
						cameFrom[neighbour] = (current, distance + 1);
					}
				}
			}

			HashSet<Cell> validCells = new HashSet<Cell>();
			foreach (var kvp in cameFrom)
			{
				if (kvp.Key != start)
				{
					validCells.Add(kvp.Key);
				}
			}

			return validCells.ToList();
		}

		private IList<Vector3> ConstructPath(Cell startCell, Cell endCell, Dictionary<Cell, Cell> cameFrom)
		{
			IList<Vector3> path = new List<Vector3>();
			Cell current = endCell;

			while (current != startCell)
			{
				path.Add(current.WorldPosition);
				current = cameFrom[current];
			}

			path.Reverse();

			return path;
		}

		#endregion

		#region Dijkstra's Algorithm
		/// <summary>
		/// Performs Dijkstra's algorithm to find the shortest path from the start position to the end position.
		/// Link: https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
		/// </summary>
		/// <param name="startPosition">Start position</param>
		/// <param name="endPosition">End position</param>
		/// <returns>List of nodes representing the path, or null if no path is found</returns>
		public IList<Vector3> Dijkstra(Vector3 startPosition, Vector3 endPosition)
		{
			Cell startCell = _grid.GetCell(startPosition);
			Cell endCell = _grid.GetCell(endPosition);

			if (startCell == null || endCell == null || startCell == endCell ||
				!startCell.IsWalkable || !endCell.IsWalkable)
			{
				// Early exit in case there is no valid path possible.
				return null;
			}

			_frontier.Clear();
			_visited.Clear();

			startCell.Parent = null;
			startCell.GCost = 0; // At the beginning the cost is 0. We haven't travelled anywhere yet.
			startCell.HCost = 0; // Has to be zeroed if A* was used between two Dijkstra calls.

			_frontier.Enqueue(startCell);

			while (_frontier.Count > 0)
			{
				Cell current = _frontier.Dequeue();
				_visited.Add(current);

				if (current == endCell)
				{
					// Early exit.
					// We have reached the end node. No need to continue.
					break;
				}

				IList<Cell> neighbours = _grid.GetNeighbours(current, PathfindingConfig.AllowDiagonalPathfinding);
				foreach (Cell neighbour in neighbours)
				{
					if (!neighbour.IsWalkable || _visited.Contains(neighbour))
					{
						// Skip this neighbour if it's not walkable or if it has been already visited.
						continue;
					}

					int costToNeighbour = _grid.GetCostToNeighbour(current, neighbour);
					if (costToNeighbour <= 0)
					{
						// The neighbour is not a neighbour of the current node.
						GD.PrintErr("Invalid cost to the neighbour! Did GetNeighbours return a Node " +
											"which is not a neighbour?");
						continue;
					}

					// The total cost of the path so far.
					int costSoFar = current.GCost + costToNeighbour;
					if (!_frontier.Contains(neighbour) // The neighbour hasn't been inspected yet
						|| costSoFar < neighbour.GCost) // or there is a better path to the neighbour.
					{
						// Update the cost to the neighbour from current node
						neighbour.GCost = costSoFar;
						neighbour.HCost = 0;

						// Add to the frontier in order to process its neighbours.
						_frontier.Enqueue(neighbour);

						// It's cheapest to navigate to this neighbour from the current node.
						neighbour.Parent = current;
					}
				}
			}

			return RetracePath(startCell, endCell);
		}


		#endregion

		#region Common
		private IList<Vector3> RetracePath(Cell startCell, Cell endCell)
		{
			IList<Vector3> path = new List<Vector3>();

			Cell current = endCell;
			bool isValid = true;

			while (current != startCell && (isValid = current != null))
			{
				path.Add(current.WorldPosition);
				current = current.Parent;
			}

			if (!isValid)
			{
				// The path is invalid. The end node was not reached.
				return null;
			}

			path.Reverse();

			return path;
		}
		#endregion
	}
}