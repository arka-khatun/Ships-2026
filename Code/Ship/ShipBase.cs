using Godot;

namespace GA.Ships
{
	public abstract partial class ShipBase : CharacterBody3D
	{
		[Export] private ShipData _shipData = null;
		public ShipData ShipData => _shipData;
	}
}