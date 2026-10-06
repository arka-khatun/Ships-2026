using System;
using Godot;

namespace GA.Ships.Pathfinding.Testing
{
	public partial class PathfindingDistance : Node3D
	{
		[Export] private int _distance = 5;

		public override void _Ready()
		{
			CallDeferred(nameof(PerformDistanceTest));
		}

		private void PerformDistanceTest()
		{
			NavigationGrid.Cell startCell = Level.Current.Grid.GetCell(GlobalPosition);
			var cells = Level.Current.Pathfinder.GetReachableCells(startCell, _distance);
			Level.Current.Grid.SetHighlightedCells(cells);
		}
	}
}