using Godot;
using System;

public sealed partial class FreeLookCamera : Camera3D
{
	[Export] public bool Active = true;
	public static FreeLookCamera singleton;
	[Export] public float target_speed = 5f;
	[Export] public float target_accel = 10f;
	[Export] public float deaccel = 10f;
	[Export] public float friction = 3f;

	[Export] public Vector2 Rot = Vector2.Zero;

	[Export] public Cmd cmd = new();

	[Export]
	public Vector3
   velocity = Vector3.Zero,
   move_direction = Vector3.Zero;
	public override void _Ready()
	{
	}

	public override void _UnhandledInput(InputEvent input)
	{
		bool is_pressed;

		Type input_type = input.GetType();


		if (input_type == typeof(InputEventKey))
		{
			InputEventKey key = (InputEventKey)input;
			Key key_code = key.PhysicalKeycode;
			is_pressed = input.IsPressed();
			UnhandledInputKey(key, in key_code, in is_pressed, input.IsEcho());
		}
		if (input_type == typeof(InputEventMouseMotion))
		{
			InputEventMouseMotion mouse_motion = (InputEventMouseMotion)input;

			UnhandledInputMouseMotion(mouse_motion, mouse_motion.Relative);
		}
		else if (input_type == typeof(InputEventMouseButton))
		{
			InputEventMouseButton mouse_button = (InputEventMouseButton)input;
			is_pressed = input.IsPressed();
		}
		else if (input_type == typeof(InputEventJoypadMotion))
		{
			InputEventJoypadMotion joypad_motion = (InputEventJoypadMotion)input;
		}
		else if (input_type == typeof(InputEventJoypadButton))
		{
			InputEventJoypadButton joypad_button = (InputEventJoypadButton)input;

			is_pressed = input.IsPressed();
		}
		else if (input_type == typeof(InputEventScreenDrag))
		{
			InputEventScreenDrag screen_drag = (InputEventScreenDrag)input;
		}
		else if (input_type == typeof(InputEventScreenTouch))
		{
			InputEventScreenTouch screen_touch = (InputEventScreenTouch)input;
			is_pressed = input.IsPressed();
		}
	}

	public void UnhandledInputKey(InputEventKey inputEventKey, in Key key_code, in bool is_pressed, in bool is_echo)
	{
		if (!is_echo) // not holding down
		{
			if (is_pressed)
			{

				if (Cmd.is_action(in key_code, Cmd.Mappings.left))
				{

					if (Client.input_flags.HasFlag(Client.InputBitFlag.Right))
					{
						cmd.move.X = 0f;
						cmd.move.Y = Math.Sign(cmd.move.Y);
					}
					else
					{
						cmd.move.X = Math.Abs(cmd.move.Y) > 0f ? -0.7071f : -1f;
						cmd.move.Y = Math.Sign(cmd.move.Y) * 0.7071f;
					}
					Client.input_flags |= Client.InputBitFlag.Left; //add
				}
				else if (Cmd.is_action(in key_code, Cmd.Mappings.right))
				{
					if (Client.input_flags.HasFlag(Client.InputBitFlag.Left))
					{
						cmd.move.X = 0f;
						cmd.move.Y = Math.Sign(cmd.move.Y);
					}
					else
					{
						cmd.move.X = Math.Abs(cmd.move.Y) > 0f ? 0.7071f : 1f;
						cmd.move.Y = Math.Sign(cmd.move.Y) * 0.7071f;
					}

					Client.input_flags |= Client.InputBitFlag.Right;
				}
				else if (Cmd.is_action(in key_code, Cmd.Mappings.forward))
				{
					if (Client.input_flags.HasFlag(Client.InputBitFlag.Backward))
					{
						cmd.move.Y = 0f;
						cmd.move.X = Math.Sign(cmd.move.X);
					}
					else
					{
						cmd.move.Y = Math.Abs(cmd.move.X) > 0f ? 0.7071f : 1f;
						cmd.move.X = Math.Sign(cmd.move.X) * 0.7071f;
					}
					Client.input_flags |= Client.InputBitFlag.Forward;
				}
				else if (Cmd.is_action(in key_code, Cmd.Mappings.backward))
				{
					if (Client.input_flags.HasFlag(Client.InputBitFlag.Forward))
					{
						cmd.move.Y = 0f;
						cmd.move.X = Math.Sign(cmd.move.X);
					}
					else
					{
						cmd.move.Y = Math.Abs(cmd.move.X) > 0f ? -0.7071f : -1f;
						cmd.move.X = Math.Sign(cmd.move.X) * 0.7071f;

						// if (cmd.sprintState > 0)
						// 	cmd.sprintState = SprintState.None;

					}
					Client.input_flags |= Client.InputBitFlag.Backward;
				}
			}
			else // if key release
			{

				if (Cmd.is_action(in key_code, Cmd.Mappings.left))
				{
					if (Client.input_flags.HasFlag(Client.InputBitFlag.Right))
					{
						cmd.move.X = Math.Abs(cmd.move.Y) > 0f ? 0.7071f : 1f;
						cmd.move.Y = Math.Sign(cmd.move.Y) * 0.7071f;
					}
					else
					{
						cmd.move.X = 0f;
						cmd.move.Y = Math.Sign(cmd.move.Y);
					}
					Client.input_flags &= ~Client.InputBitFlag.Left;
				}
				else if (Cmd.is_action(in key_code, Cmd.Mappings.right))
				{
					if (Client.input_flags.HasFlag(Client.InputBitFlag.Left))
					{
						cmd.move.X = Math.Abs(cmd.move.Y) > 0f ? -0.7071f : -1f;
						cmd.move.Y = Math.Sign(cmd.move.Y) * 0.7071f;
					}
					else
					{
						cmd.move.X = 0f;
						cmd.move.Y = Math.Sign(cmd.move.Y);
					}
					Client.input_flags &= ~Client.InputBitFlag.Right; // remove
				}
				else if (Cmd.is_action(in key_code, Cmd.Mappings.forward))
				{
					if (Client.input_flags.HasFlag(Client.InputBitFlag.Backward))
					{
						cmd.move.Y = Math.Abs(cmd.move.X) > 0f ? -0.7071f : -1f;
						cmd.move.X = Math.Sign(cmd.move.X) * 0.7071f;
					}
					else
					{
						cmd.move.Y = 0f;
						cmd.move.X = Math.Sign(cmd.move.X);

						// if (cmd.sprintState > 0)
						// {
						//     cmd.sprintState = SprintState.None;
						//     if (isGrounded && sprintState > 0)
						//         stop_sprinting();
						// }
					}
					Client.input_flags &= ~Client.InputBitFlag.Forward;
				}
				else if (Cmd.is_action(in key_code, Cmd.Mappings.backward))
				{
					if (Client.input_flags.HasFlag(Client.InputBitFlag.Forward))
					{
						cmd.move.Y = Math.Abs(cmd.move.X) > 0f ? 0.7071f : 1f;
						cmd.move.X = Math.Sign(cmd.move.X) * 0.7071f;
					}
					else
					{
						cmd.move.Y = 0f;
						cmd.move.X = Math.Sign(cmd.move.X);
					}
					Client.input_flags &= ~Client.InputBitFlag.Backward;
				}
			}
		}
	}
	public void UnhandledInputMouseMotion(InputEventMouseMotion mouse_motion, in Vector2 relative)
	{
		Rot -= relative;

		if (Mathf.Abs(Rot.X) >= 360f)
			Rot.X = 0f;

		if (Mathf.Abs(Rot.Y) > 89.5f)
			Rot.Y = Mathf.Sign(Rot.Y) * 89.5f;
	}

	public void doFly()
	{
		float current_speed = MathF.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y + velocity.Z * velocity.Z);
		// Deacceleration and Friction
		float control = current_speed < deaccel ? deaccel : current_speed;
		float drop = control * friction * Game.PhysicsDelta;
		float newspeed = current_speed - drop;

		if (newspeed < 0f)
			newspeed = 0f;
		if (current_speed > 0f)
			newspeed /= current_speed;

		velocity *= newspeed;

		// Acceleration
		float currentspeed = velocity.Dot(move_direction);
		float addspeed = target_speed - currentspeed;
		if (addspeed <= 0f)
			return;
		float accelspeed = target_accel * Game.PhysicsDelta * target_speed;
		if (accelspeed > addspeed)
			accelspeed = addspeed;

		velocity += accelspeed * move_direction;

	}


	public void FreeMovementDirection()
	{
		// Convert rotation degrees to radians
		float rotationX_rad = Rot.X * Utilities.DegToRad; // DegToRad
		float rotationY_rad = Rot.Y * Utilities.DegToRad; // DegToRad

		float cosY = -MathF.Cos(rotationY_rad);

		// Calculate the direction vectors for rotationX and rotationY
		float directionX = MathF.Sin(rotationX_rad) * cosY;
		float directionY = MathF.Sin(rotationY_rad);
		float directionZ = MathF.Cos(rotationX_rad) * cosY;

		// Combine the direction vectors with the movement inputs

		move_direction.X = directionX * cmd.move.Y - directionZ * cmd.move.X;
		move_direction.Y = directionY * cmd.move.Y;
		move_direction.Z = directionZ * cmd.move.Y + directionX * cmd.move.X;

		Utilities.Normalize(ref move_direction);
		// move_direction.Y += Convert.ToSingle(player.cmd.jump) - Convert.ToSingle(player.cmd.stanceState == StanceState.Crouching);

	}


	public override void _Process(double delta)
	{
		FreeMovementDirection();
		doFly();
		GlobalTranslate(velocity * Game.PhysicsDelta);
		GlobalRotationDegrees = new(Rot.Y, Rot.X, 0f);
	}
}
