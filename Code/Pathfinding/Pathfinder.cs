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
		private class FrontierEntry : IComparable<FrontierEntry>
		{
			public Cell Cell { get; }
			public int TotalCost { get; }

			public FrontierEntry(Cell cell, int totalCost)
			{
				Cell = cell;
				TotalCost = totalCost;
			}

			public int CompareTo(FrontierEntry other)
			{
				return other == null ? -1 : TotalCost.CompareTo(other.TotalCost);
			}
		}

		private NavigationGrid _grid = null;

		// Cells, which will be inspected.
		private PriorityQueue<FrontierEntry> _frontier = new PriorityQueue<FrontierEntry>();

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

		// public IList<Cell> GetReachableCells(Cell start, int maxSteps, bool includeDiagonal)
		// {
		// 	Queue<Cell> frontier = new Queue<Cell>();
		// 	Dictionary<Cell, (Cell, int)> cameFrom = new Dictionary<Cell, (Cell, int)>();
		//
		// 	frontier.Enqueue(start);
		// 	cameFrom[start] = (null, 0);
		//
		// 	bool isEndReached = false;
		//
		// 	while (frontier.Count > 0)
		// 	{
		// 		Cell current = frontier.Dequeue();
		// 		int distance = cameFrom[current].Item2;
		//
		// 		isEndReached = distance >= maxSteps;
		// 		if (isEndReached)
		// 		{
		// 			// Max. distance is reached. Path is complete.
		// 			break;
		// 		}
		//
		// 		IList<Cell> neighbours = _grid.GetNeighbours(current, includeDiagonal: includeDiagonal);
		// 		foreach (Cell neighbour in neighbours)
		// 		{
		// 			if (neighbour.IsWalkable && !cameFrom.ContainsKey(neighbour))
		// 			{
		// 				frontier.Enqueue(neighbour);
		// 				cameFrom[neighbour] = (current, distance + 1);
		// 			}
		// 		}
		// 	}
		//
		// 	HashSet<Cell> validCells = new HashSet<Cell>();
		// 	foreach (var kvp in cameFrom)
		// 	{
		// 		if (kvp.Key != start)
		// 		{
		// 			validCells.Add(kvp.Key);
		// 		}
		// 	}
		//
		// 	return validCells.ToList();
		// }

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
		/// Finds every cell that can be reached from <paramref name="start"/> by spending at most
		/// <paramref name="maxMovementPoints"/> movement points (a terrain-aware movement range.)
		/// Moving into a cell costs that cell's Cost. The start cell is not included in the result
		/// and its cost is not charged. The algorithm doesn't contain an early exit, so every cell whose
		/// cheapest route fits the budget is returned, even if that route takes more steps than a more
		/// expensive one. Non-walkable cells are never entered.
		/// A diagonal move costs the same as an axial one (the cost of the cell entered). Diagonals are used
		/// only if <paramref name="useDiagonals"/> is true and PathfindingConfig.AllowDiagonalPathfinding is enabled.
		/// Link: https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
		/// </summary>
		/// <param name="startPosition">Start position</param>
		/// <param name="endPosition">End position</param>
		/// <param name="start">The cell where the movement starts. Must be walkable.</param>
		/// <param name="maxMovementPoints">The movement point budget. 0 returns an empty list.</param>
		/// <param name="useDiagonals">True to allow diagonal moves.</param>
		/// <returns>List of nodes representing the path, or null if no path is found</returns>
		/// <exception cref="ArgumentNullException"><paramref name="start"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="maxMovementPoints"/> is negative.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="start"/> is outside the navigation grid or is not walkable.
		/// </exception>

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
			_grid.ResetPathData();

			startCell.Parent = null;
			startCell.GCost = 0; // At the beginning the cost is 0. We haven't travelled anywhere yet.
			startCell.HCost = 0; // Has to be zeroed if A* was used between two Dijkstra calls.

			_frontier.Enqueue(new FrontierEntry(startCell, 0));

			while (_frontier.Count > 0)
			{
				FrontierEntry entry = _frontier.Dequeue();
				Cell current = entry.Cell;

				if (!_visited.Add(current))
				{
					// Outdated entry. The cell was already processed through a cheaper route.
					continue;
				}

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
					if (costSoFar < neighbour.GCost) // or there is a better path to the neighbour.
					{
						// Update the cost to the neighbour from current node
						neighbour.GCost = costSoFar;
						neighbour.HCost = 0;

						// Add to the frontier in order to process its neighbours.
						_frontier.Enqueue(new FrontierEntry(neighbour, costSoFar));

						// It's cheapest to navigate to this neighbour from the current node.
						neighbour.Parent = current;
					}
				}
			}

			return RetracePath(startCell, endCell);
		}
		
		public IList<Cell> GetReachableCells(Cell start, int maxMovementPoints, bool useDiagonals)
		{
			if (start == null)
			{
				throw new ArgumentNullException(nameof(start));
			}

			if (maxMovementPoints < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxMovementPoints), maxMovementPoints,
					"The movement point budget cannot be negative.");
			}

			if (start.X < 0 || start.Y < 0 || start.X >= _grid.Width || start.Y >= _grid.Height)
			{
				throw new ArgumentException("The start cell is not inside the navigation grid.", nameof(start));
			}

			if (!start.IsWalkable)
			{
				throw new ArgumentException("The start cell is not walkable.", nameof(start));
			}

			// This is a read-only query, so it uses local state. The shared _frontier and _visited fields
			// and the per-cell GCost/Parent data belong to Dijkstra and A*, and are left untouched.
			PriorityQueue<FrontierEntry> frontier = new PriorityQueue<FrontierEntry>();

			// The cheapest known total cost to each discovered cell.
			Dictionary<Cell, int> bestCost = new Dictionary<Cell, int>();
			IList<Cell> reachable = new List<Cell>();

			frontier.Enqueue(new FrontierEntry(start, 0));
			bestCost[start] = 0;

			while (frontier.Count > 0)
			{
				FrontierEntry entry = frontier.Dequeue();
				Cell current = entry.Cell;

				if (entry.TotalCost > bestCost[current])
				{
					// Outdated entry. A cheaper route to this cell was already processed.
					continue;
				}

				if (current != start)
				{
					// The cheapest route to this cell is now final, and it fits the budget.
					reachable.Add(current);
				}

				int remainingPoints = maxMovementPoints - entry.TotalCost;

				IList<Cell> neighbours = _grid.GetNeighbours(current, includeDiagonal: useDiagonals);
				foreach (Cell neighbour in neighbours)
				{
					// Compare against the remaining points instead of adding to the total,
					// so a huge budget cannot overflow. The walkable check also keeps
					// negative costs out of the cost calculation.
					if (!neighbour.IsWalkable || neighbour.Cost > remainingPoints)
					{
						continue;
					}

					int newCost = entry.TotalCost + neighbour.Cost;
					if (!bestCost.TryGetValue(neighbour, out int knownCost) || newCost < knownCost)
					{
						bestCost[neighbour] = newCost;
						frontier.Enqueue(new FrontierEntry(neighbour, newCost));
					}
				}
			}

			return reachable;
		}

		#region A*
		public IList<Vector3> AStar(Vector3 startPosition, Vector3 endPosition)
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
			_grid.ResetPathData();

			startCell.Parent = null;
			startCell.GCost = 0;
			startCell.HCost = GetEstimatedCost(startCell, endCell);

			_frontier.Enqueue(new FrontierEntry(startCell, startCell.FCost));

			while (_frontier.Count > 0)
			{
				FrontierEntry entry = _frontier.Dequeue();
				Cell current = entry.Cell;

				if (!_visited.Add(current))
				{
					// Outdated entry. The cell was already processed through a cheaper route.
					continue;
				}

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
						// GetCostToNeighbour returns a negative value when the cells are not neighbours.
						GD.PrintErr("Invalid cost to the neighbour! Did GetNeighbours return a Node " +
											"which is not a neighbour?");
						continue;
					}

					int costSoFar = current.GCost + costToNeighbour;
					if (costSoFar < neighbour.GCost) // Not reached yet (int.MaxValue) or a cheaper path was found.
					{
						neighbour.GCost = costSoFar;
						neighbour.HCost = GetEstimatedCost(neighbour, endCell);
						neighbour.Parent = current;

						_frontier.Enqueue(new FrontierEntry(neighbour, neighbour.FCost));
					}
				}
			}

			return RetracePath(startCell, endCell);
		}

#pragma warning disable CS0162
		private int GetEstimatedCost(Cell startCell, Cell endCell)
		{
			if (PathfindingConfig.AllowDiagonalPathfinding)
			{
				return GetEstimatedDiagonalDistance(startCell, endCell);
			}
			return GetEstimatedAxialDistance(startCell, endCell);
		}
#pragma warning restore CS0162

		private int GetEstimatedDiagonalDistance(Cell startCell, Cell endCell)
		{
			int xDist = Mathf.Abs(startCell.X - endCell.X);
			int yDist = Mathf.Abs(startCell.Y - endCell.Y);

			int diagonalSteps = Mathf.Min(xDist, yDist);
			int axialSteps = Mathf.Max(xDist, yDist) - diagonalSteps;

			return diagonalSteps * 14 + axialSteps * 10;
		}

		private int GetEstimatedAxialDistance(Cell startCell, Cell endCell)
		{
			// Manhattan distance
			return (Mathf.Abs(startCell.X - endCell.X) + Mathf.Abs(startCell.Y - endCell.Y)) * 10;
		}
		#endregion

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