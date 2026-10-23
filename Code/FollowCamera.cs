using Godot;

namespace GA.Ships
{
	public partial class FollowCamera : Camera3D
	{
		[Export] private Node3D _target = null;
		[Export] private float _maxDistance = 12f;
		[Export(PropertyHint.Range, "-90, 0, -45")] private float _elevationAngle = 25f;
		[Export(PropertyHint.Range, "-90, 90 ,0")] private float _yawAngle = 45f;
		[Export(PropertyHint.Range, "0, 1, 0")] private float _zoom = 0f;
		[Export] private float _smoothSpeed = 6f;
		[Export] private float _zoomStep = 0.1f;

		public override void _Ready()
		{
			if (_target == null)
			{
				GD.PrintErr("FollowCamera: Target is not assigned.");
			}
		}

		public override void _Process(double delta)
		{
			if (_target == null) return;

			Vector3 desiredPosition = GetDesiredPosition();
			GlobalPosition = GlobalPosition.Lerp(desiredPosition, _smoothSpeed * (float)delta);
			LookAt(_target.GlobalPosition, Vector3.Up);
		}

		public override void _Input(InputEvent @event)
		{
			if (@event is not InputEventMouseButton mouseEvent)
			{
				return;
			}

			if (!mouseEvent.Pressed)
			{
				return;
			}

			if (Input.IsActionJustPressed("ZoomIn"))
			{
				AdjustZoom(_zoomStep);
			}
			else if (Input.IsActionJustPressed("ZoomOut"))
			{
				AdjustZoom(-_zoomStep);
			}
		}

		public void AdjustZoom(float amount)
		{
			_zoom = Mathf.Clamp(_zoom + amount, 0f, 1f);
		}

		private Vector3 GetDesiredPosition()
		{
			float currentDistance = Mathf.Lerp(_maxDistance, 0f, _zoom);
			Vector3 localOffset = GetFollowOffset(currentDistance);
			return _target.GlobalPosition + (_target.GlobalTransform.Basis * localOffset);
		}

		private Vector3 GetFollowOffset(float distance)
		{
			float pitch = Mathf.DegToRad(_elevationAngle);
			float yaw = Mathf.DegToRad(_yawAngle);

			Vector3 offset = new Vector3(
				Mathf.Sin(yaw) * Mathf.Cos(pitch),
				Mathf.Sin(pitch),
				Mathf.Cos(yaw) * Mathf.Cos(pitch)
			);

			return -offset * distance;
		}
	}
}