using Godot;

namespace GA.Ships
{
	[GlobalClass]
	public partial class ShipData : Resource
	{
		/// <summary>
		/// The speed at which the ship moves, in units per second.
		/// </summary>
		[Export] public float MovementSpeed { get; set; } = 5.0f;

		/// <summary>
		/// The speed at which the ship rotates, in degrees per second.
		/// </summary>
		[Export] public float RotationSpeed { get; set; } = 30.0f;

		/// <summary>
		/// The acceleration of the ship, in units per second squared.
		/// </summary>
		[Export] public float Acceleration { get; set; } = 1.5f;

		/// <summary>
		/// The deceleration of the ship, in units per second squared.
		/// </summary>
		[Export] public float Deceleration { get; set; } = 1f;
	}
}