using Godot;
using System;

[GlobalClass]
public sealed partial class Movement : Resource
{
    public Player player;

    [Export] public bool Active = true;
    [Export] public MovementData data;
    public CollisionShape3D world_collider;
    public CylinderShape3D collider_shape;

    public enum SprintState : byte { None, Sprinting, TacSprinting }
    [Export] public SprintState sprintState = SprintState.None;
    public enum StanceState : byte { Standing, Crouching, Prone }
    [Export] public StanceState stance = StanceState.Standing;

    [Export]
    public Vector3
    Position = Vector3.Zero,
    velocity = Vector3.Zero,
    move_direction = Vector3.Zero;

    [Export]
    public float
    horSpeed = 0f,
    target_speed = 0f,
    target_accel = 0f,
    CurrentSpeedMult = 1f,
    CurrentAccelMult = 1f;
    public void Init(Player player)
    {
        this.player = player;
        player.SetNode(out world_collider, world_collider_path);
        collider_shape = (CylinderShape3D)world_collider.Shape;
        stance_check_shape.Radius = collider_shape.Radius;
        stance_check_shape.Height = collider_shape.Height;
        data ??= new();

        OnGrounded += OnGroundedMethod;
        onJump += onJumpMethod;
        target_speed = data.move_speed_base;
    }
    public void UnhandledInputKey(InputEventKey inputEventKey, in uint key_code, in bool is_pressed, in bool is_echo)
    {
        if (!Active) return;

        if (!is_echo) // not holding down
        {
            if (is_pressed)
            {
                #region Move Input

                if (Cmd.is_action(in key_code, Cmd.Mappings.left))
                {
                    player.cmd.move.X += Math.Abs(player.cmd.move.Y) > 0f ? -0.7071f : -1f;
                    player.cmd.move.Y = Math.Abs(player.cmd.move.X) > 0f ? Math.Sign(player.cmd.move.Y) * 0.7071f : Math.Sign(player.cmd.move.Y);
                    move_direction = CalculateDiagonalMoveDirection(in player.look.Rot.X, in player.cmd.move.X, in player.cmd.move.Y);
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.right))
                {
                    player.cmd.move.X += Math.Abs(player.cmd.move.Y) > 0f ? 0.7071f : 1f;
                    player.cmd.move.Y = Math.Abs(player.cmd.move.X) > 0f ? Math.Sign(player.cmd.move.Y) * 0.7071f : Math.Sign(player.cmd.move.Y);
                    move_direction = CalculateDiagonalMoveDirection(in player.look.Rot.X, in player.cmd.move.X, in player.cmd.move.Y);
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.forward))
                {
                    player.cmd.move.Y += Math.Abs(player.cmd.move.X) > 0f ? 0.7071f : 1f;
                    player.cmd.move.X = Math.Abs(player.cmd.move.Y) > 0f ? Math.Sign(player.cmd.move.X) * 0.7071f : Math.Sign(player.cmd.move.X);
                    move_direction = CalculateDiagonalMoveDirection(in player.look.Rot.X, in player.cmd.move.X, in player.cmd.move.Y);
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.backward))
                {
                    player.cmd.move.Y += Math.Abs(player.cmd.move.X) > 0f ? -0.7071f : -1f;
                    player.cmd.move.X = Math.Abs(player.cmd.move.Y) > 0f ? Math.Sign(player.cmd.move.X) * 0.7071f : Math.Sign(player.cmd.move.X);

                    if (player.cmd.sprintState > 0)
                        player.cmd.sprintState = SprintState.None;

                    move_direction = CalculateDiagonalMoveDirection(in player.look.Rot.X, in player.cmd.move.X, in player.cmd.move.Y);
                }
                #endregion

                else if (Cmd.is_action(in key_code, Cmd.Mappings.jump))
                {
                    player.cmd.jump = true;
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.crouch))
                {
                    ChangeStance(stance == StanceState.Crouching ? StanceState.Standing : StanceState.Crouching); // this is smart
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.sprint) && player.cmd.move.Y > 0f)
                {
                    player.cmd.sprintState = (SprintState)((byte)(player.cmd.sprintState + 1) % 3);
                    if (isGrounded)
                    {
                        switch (player.cmd.sprintState)
                        {
                            case SprintState.Sprinting:
                                start_sprinting();
                                break;
                            case SprintState.TacSprinting:
                                start_tac_sprinting();
                                break;
                            default:
                                stop_sprinting();
                                break;
                        }
                    }
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.prone))
                {
                    ChangeStance(stance == StanceState.Prone ? StanceState.Standing : StanceState.Prone);
                }
            }
            else // if key release
            {

                #region Move Input
                if (Cmd.is_action(in key_code, Cmd.Mappings.left))
                {
                    player.cmd.move.X += Math.Abs(player.cmd.move.Y) > 0f ? 0.7071f : 1f;
                    player.cmd.move.Y = Math.Abs(player.cmd.move.X) > 0f ? Math.Sign(player.cmd.move.Y) * 0.7071f : Math.Sign(player.cmd.move.Y);
                    move_direction = CalculateDiagonalMoveDirection(in player.look.Rot.X, in player.cmd.move.X, in player.cmd.move.Y);
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.right))
                {
                    player.cmd.move.X -= Math.Abs(player.cmd.move.Y) > 0f ? 0.7071f : 1f;
                    player.cmd.move.Y = Math.Abs(player.cmd.move.X) > 0f ? Math.Sign(player.cmd.move.Y) * 0.7071f : Math.Sign(player.cmd.move.Y);
                    move_direction = CalculateDiagonalMoveDirection(in player.look.Rot.X, in player.cmd.move.X, in player.cmd.move.Y);
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.forward))
                {
                    player.cmd.move.Y -= Math.Abs(player.cmd.move.X) > 0f ? 0.7071f : 1f;
                    player.cmd.move.X = Math.Abs(player.cmd.move.Y) > 0f ? Math.Sign(player.cmd.move.X) * 0.7071f : Math.Sign(player.cmd.move.X);

                    if (player.cmd.sprintState > 0)
                    {
                        player.cmd.sprintState = SprintState.None;
                        if (isGrounded && sprintState > 0)
                            stop_sprinting();
                    }

                    move_direction = CalculateDiagonalMoveDirection(in player.look.Rot.X, in player.cmd.move.X, in player.cmd.move.Y);
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.backward))
                {
                    player.cmd.move.Y += Math.Abs(player.cmd.move.X) > 0f ? 0.7071f : 1f;
                    player.cmd.move.X = Math.Abs(player.cmd.move.Y) > 0f ? Math.Sign(player.cmd.move.X) * 0.7071f : Math.Sign(player.cmd.move.X);
                    move_direction = CalculateDiagonalMoveDirection(in player.look.Rot.X, in player.cmd.move.X, in player.cmd.move.Y);
                }
                #endregion
                else if (Cmd.is_action(in key_code, Cmd.Mappings.jump))
                {
                    player.cmd.jump = false;
                }
            }
        }
    }

    public void PhysicsUpdate()
    {
        Position = player.Position;
        collision_and_ground_check();
        velocity = player.Velocity; // Taking the current velocity
        doMove(); // processing it
        player.Velocity = velocity; // Applying the newly calculated velocity, simple
        player.MoveAndSlide(); // Finally do the moving


        tac_sprint_stamina_mechanic();

    }
    public void Update()
    {
        change_stance_mechanic();
    }
    public void doMove()
    {

        horSpeed = MathF.Sqrt(velocity.X * velocity.X + velocity.Z * velocity.Z);
        // Deacceleration and Friction
        if (isGrounded)
        {
            float control = horSpeed < data.deaccel ? data.deaccel : horSpeed;
            float drop = control * data.friction * Game.PhysicsDelta;
            float newspeed = horSpeed - drop;

            if (newspeed < 0f)
                newspeed = 0f;
            if (horSpeed > 0f)
                newspeed /= horSpeed;

            velocity.X *= newspeed;
            velocity.Z *= newspeed;
            ground_move();
        }
        else
        {
            velocity.Y -= data.gravity * Game.PhysicsDelta;
            air_move();
        }
        // Acceleration
        float currentspeed = velocity.X * move_direction.X + velocity.Z * move_direction.Z; // Basically Vector2 dot product
        float addspeed = target_speed - currentspeed;
        if (addspeed <= 0f)
            return;
        float accelspeed = target_accel * Game.PhysicsDelta * target_speed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;


        velocity.X += accelspeed * move_direction.X;
        velocity.Z += accelspeed * move_direction.Z;

    }
    void ground_move()
    {
        if (player.cmd.jump)
        {
            switch (stance)
            {
                case StanceState.Standing:
                    if (isGrounded)
                        onJump.Invoke();
                    break;

                case StanceState.Crouching:
                    ChangeStance(StanceState.Standing);
                    player.cmd.jump = false;
                    break;

                default:
                    ChangeStance(StanceState.Crouching);
                    player.cmd.jump = false;
                    break;
            }
        }
    }

    void air_move()
    {
    }

    public bool isJumping = false;
    public Action onJump;

    public void onJumpMethod()
    {
        velocity.Y = data.jump_speed;
        player.cmd.jump = false;
        isJumping = true;
    }
    [Export] public bool isGrounded = false;
    public Action<bool> OnGrounded;
    void collision_and_ground_check()
    {
        bool really_is_grounded = player.IsOnFloor();
        if (really_is_grounded != isGrounded)
            OnGrounded.Invoke(really_is_grounded);
    }
    void OnGroundedMethod(bool isGrounded)
    {
        this.isGrounded = isGrounded;
        GD.Print("isGrounded = " + isGrounded);
        if (isGrounded)
        {
            velocity.Y = 0f;
            switch (player.cmd.sprintState)
            {
                case SprintState.None: // is already not sprinting, just setting the speed and accel values
                    switch (stance)
                    {
                        case StanceState.Standing:
                            CurrentSpeedMult = 1f;
                            CurrentAccelMult = 1f;
                            break;
                        case StanceState.Crouching:
                            CurrentSpeedMult = data.move_speed_crouch_mult;
                            CurrentAccelMult = 1f;
                            break;
                        default: // Prone
                            CurrentSpeedMult = data.move_speed_prone_mult;
                            CurrentAccelMult = 1f;
                            break;
                    }
                    target_speed = data.move_speed_base * CurrentSpeedMult;
                    target_accel = data.accel * CurrentAccelMult;
                    break;
                case SprintState.Sprinting:
                    start_sprinting();
                    break;
                case SprintState.TacSprinting:
                    start_tac_sprinting();
                    break;
            }
            isJumping = false;
        }
        else
        {
            #region stop_sprinting() without modifying the current speed
            sprintState = SprintState.None;
            player.cmd.sprintState = SprintState.None;
            CurrentAccelMult = data.air_accel;
            target_accel = data.accel * CurrentAccelMult;
            #endregion

        }
    }
    public CylinderShape3D stance_check_shape = new();
    public void ChangeStance(in StanceState new_stance)
    {
        if (new_stance == stance) return;
        byte new_stance_index = (byte)new_stance;

        float new_height = collider_heights[new_stance_index];


        if (new_stance_index < 2 && new_stance_index > 0) // rising
        {
            ShapeCaster.Origin = Position + new Vector3(0f, new_height * 0.5f, 0f);
            stance_check_shape.Height = new_height;
            ShapeCaster.Shape = stance_check_shape;
            ShapeCaster.AddExceptionRid(player.GetRid());
            ShapeCaster.CollisionMask = player.CollisionMask;

            ShapeCaster.ForceShapecastUpdate();
            ShapeCaster.ClearExceptions();

            if (ShapeCaster.IsColliding)
            {
                GD.Print("Cannot change stance, blocked");
                return;
            }
        }
        previous_stance = stance;
        stance = new_stance;
        collider_shape.Height = new_height;
        world_collider.Position = new(0f, new_height * 0.5f, 0f);

        if (sprintState > 0)
        {
            stop_sprinting();
        }
        switch (new_stance)
        {
            case StanceState.Standing:
                CurrentSpeedMult = 1f;
                break;
            case StanceState.Crouching:
                CurrentSpeedMult = (data.move_speed_crouch_mult);
                break;
            default:
                CurrentSpeedMult = (data.move_speed_prone_mult);
                break;
        }
        target_speed = data.move_speed_base * CurrentSpeedMult;
    }
    public static readonly float[] stance_look_heights = new float[3] { 0f, -0.5f, -1f };
    public static readonly float[] collider_heights = new float[3] { 2f, 1f, 0.5f };

    [Export] public float current_stance_look_height = 0f;
    [Export] public float stance_change_speed = 3f;
    public StanceState previous_stance = StanceState.Standing;
    void change_stance_mechanic()
    {
        if (stance == previous_stance) return;

        switch (stance)
        {
            case StanceState.Standing:
                if (current_stance_look_height < stance_look_heights[0])
                {
                    current_stance_look_height += Game.Delta * stance_change_speed;
                }
                else
                {
                    current_stance_look_height = stance_look_heights[0];
                    previous_stance = StanceState.Standing;
                }
                break;

            case StanceState.Crouching:
                if (previous_stance == StanceState.Standing)
                {
                    if (current_stance_look_height > stance_look_heights[1])
                    {
                        current_stance_look_height -= Game.Delta * stance_change_speed;
                    }
                    else
                    {
                        current_stance_look_height = stance_look_heights[1];
                        previous_stance = StanceState.Crouching;
                    }
                }
                else if (previous_stance == StanceState.Prone)
                {
                    if (current_stance_look_height < stance_look_heights[1])
                    {
                        current_stance_look_height += Game.Delta * stance_change_speed;
                    }
                    else
                    {
                        current_stance_look_height = stance_look_heights[1];
                        previous_stance = StanceState.Crouching;
                    }
                }
                break;

            case StanceState.Prone:
                if (current_stance_look_height > stance_look_heights[2])
                {
                    current_stance_look_height -= Game.Delta * stance_change_speed;
                }
                else
                {
                    current_stance_look_height = stance_look_heights[2];
                    previous_stance = StanceState.Prone;
                }
                break;
        }
    }
    float tac_sprint_timestamp = 0f;
    void tac_sprint_stamina_mechanic()
    {
        float reverse_percentage;
        if (sprintState == SprintState.TacSprinting)
        {
            if (tac_sprint_timestamp < data.tac_sprint_duration)
                tac_sprint_timestamp += Game.PhysicsDelta;
            else
            {
                player.cmd.sprintState = SprintState.Sprinting;
                sprintState = SprintState.Sprinting;
            }
            reverse_percentage = (data.tac_sprint_duration - tac_sprint_timestamp) * 100f / data.tac_sprint_duration;
            player.ui.TacSprintStaminaBar.SetValueNoSignal(reverse_percentage);
        }
        else if (tac_sprint_timestamp > 0f)
        {
            tac_sprint_timestamp -= Game.PhysicsDelta;
            reverse_percentage = (data.tac_sprint_duration - tac_sprint_timestamp) * 100f / data.tac_sprint_duration;
            player.ui.TacSprintStaminaBar.SetValueNoSignal(reverse_percentage);
        }
    }
    public void start_sprinting()
    {
        if (stance > 0)
        {
            ChangeStance(StanceState.Standing);
        }
        sprintState = SprintState.Sprinting;
        player.cmd.sprintState = SprintState.Sprinting;
        CurrentSpeedMult = data.sprint_speed_mult;
        target_speed = data.move_speed_base * CurrentSpeedMult;
        if (isGrounded)
        {
            CurrentAccelMult = 0.7f;
            target_accel = data.accel * CurrentAccelMult;
        }
    }
    public void start_tac_sprinting()
    {

        if (stance > 0)
        {
            ChangeStance(StanceState.Standing);
        }
        sprintState = SprintState.TacSprinting;
        player.cmd.sprintState = SprintState.TacSprinting;
        CurrentSpeedMult = data.tac_sprint_speed_mult;
        target_speed = data.move_speed_base * CurrentSpeedMult;

        if (isGrounded)
        {
            CurrentAccelMult = 0.5f;
            target_accel = data.accel * CurrentAccelMult;
        }


    }
    public void stop_sprinting()
    {
        sprintState = SprintState.None;
        player.cmd.sprintState = SprintState.None;
        CurrentSpeedMult = 1f;
        target_speed = data.move_speed_base * CurrentSpeedMult;
        if (isGrounded)
        {
            CurrentAccelMult = 1f;
            target_accel = data.accel * CurrentAccelMult;
        }
    }
    public static Vector3 CalculateDiagonalMoveDirection(in float horizontal_rotation_degrees, in float horizontal, in float vertical) // more efficient than using Godot's Basis infested RotateAxis mess
    {
        // Convert Euler angles to radians
        float y = horizontal_rotation_degrees * Utilities.DegToRad; // DegToRad

        // Calculate the diagonal move direction using the Euler angles
        float cosY = MathF.Cos(y);
        float sinY = MathF.Sin(y);

        return new Vector3(
            cosY * horizontal - sinY * vertical,
            0f,
           (-sinY * horizontal - cosY * vertical)
            ).Normalized();
    }

    [ExportGroup("NodePaths")]
    [Export]
    public NodePath
        world_collider_path;
}
