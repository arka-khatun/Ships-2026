using Godot;
using System;

namespace GA.Ships
{
	public partial class ShipVisual : Node3D
	{
		// Movement along Y-axis.
		[Export] private float _floatFrequency = 1.0f;
		[Export] private float _floatAmplitude = 1.0f;

		// Rotation around forward and right axis.
		[Export] private float _rollFrequency = 0.7f;
		[Export] private float _rollAmplitude = 2.0f;
		[Export] private float _pitchAmplitude = 1.0f;

		private float _time = 0.0f;
		private Vector3 _neutralPosition = Vector3.Zero;
		private Vector3 _neutralRotation = Vector3.Zero;

		public override void _EnterTree()
		{
			_neutralPosition = Position;
			_neutralRotation = RotationDegrees;
		}

		public override void _ExitTree()
		{
			RotationDegrees = _neutralRotation;
			Position = _neutralPosition;
		}

		public override void _Process(double delta)
		{
			_time += (float)delta;
			AnimatePosition();
			AnimateRotation();
		}

		private void AnimateRotation()
		{
			float roll = Mathf.Sin(_time * _rollFrequency) * _rollAmplitude;

			// Multiply the rock frequency by a factor to slow down the pitch oscillation relative to the roll oscillation.
			// The Pi/3 is added to the phase to create a more natural rocking motion.
			float pitch = Mathf.Sin(_time * (_rollFrequency * 0.67f) + Mathf.Pi / 3f) * _pitchAmplitude;

			RotationDegrees = new Vector3(
				_neutralRotation.X + pitch,
				_neutralRotation.Y,
				_neutralRotation.Z + roll);
		}

		private void AnimatePosition()
		{
			float yOffset = Mathf.Sin(_time * _floatFrequency) * _floatAmplitude;
			Position = new Vector3(Position.X, _neutralPosition.Y + yOffset, Position.Z);
		}
	}
}