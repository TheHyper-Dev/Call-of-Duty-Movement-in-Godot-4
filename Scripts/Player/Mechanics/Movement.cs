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
    public enum StanceState : byte { Standing, Crouching, Prone } // this is also used for swimming up and down
    [Export] public StanceState stance = StanceState.Standing;

    public enum MoveState : byte { OnFoot, Water, Ladder }

    [Export] public MoveState moveState = MoveState.OnFoot;

    [Export]
    public Vector3
    GlobalPosition = Vector3.Zero,
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
        OnEnterWater += OnWaterEnterMethod;
        OnLadderEnter += OnLadderEnterMethod;
        target_speed = data.base_move_speed;
    }



    public void UnhandledInputKey(InputEventKey inputEventKey, in Key key_code, in bool is_pressed, in bool is_echo)
    {
        // if (!Active) return;

        if (!is_echo) // not holding down
        {
            if (is_pressed)
            {
                #region Move Input

                if (Cmd.is_action(in key_code, Cmd.Mappings.left))
                {
                    if (Client.input_flags.HasFlag(Client.InputBitFlag.Right))
                    {
                        player.cmd.move.X = 0f;
                        player.cmd.move.Y = Math.Sign(player.cmd.move.Y);
                    }
                    else
                    {
                        player.cmd.move.X = Math.Abs(player.cmd.move.Y) > 0f ? -0.7071f : -1f;
                        player.cmd.move.Y = Math.Sign(player.cmd.move.Y) * 0.7071f;
                    }
                    Client.input_flags |= Client.InputBitFlag.Left; //add
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.right))
                {
                    if (Client.input_flags.HasFlag(Client.InputBitFlag.Left))
                    {
                        player.cmd.move.X = 0f;
                        player.cmd.move.Y = Math.Sign(player.cmd.move.Y);
                    }
                    else
                    {
                        player.cmd.move.X = Math.Abs(player.cmd.move.Y) > 0f ? 0.7071f : 1f;
                        player.cmd.move.Y = Math.Sign(player.cmd.move.Y) * 0.7071f;
                    }

                    Client.input_flags |= Client.InputBitFlag.Right;
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.forward))
                {
                    if (Client.input_flags.HasFlag(Client.InputBitFlag.Backward))
                    {
                        player.cmd.move.Y = 0f;
                        player.cmd.move.X = Math.Sign(player.cmd.move.X);
                    }
                    else
                    {
                        player.cmd.move.Y = Math.Abs(player.cmd.move.X) > 0f ? 0.7071f : 1f;
                        player.cmd.move.X = Math.Sign(player.cmd.move.X) * 0.7071f;
                    }
                    Client.input_flags |= Client.InputBitFlag.Forward;
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.backward))
                {
                    if (Client.input_flags.HasFlag(Client.InputBitFlag.Forward))
                    {
                        player.cmd.move.Y = 0f;
                        player.cmd.move.X = Math.Sign(player.cmd.move.X);
                    }
                    else
                    {
                        player.cmd.move.Y = Math.Abs(player.cmd.move.X) > 0f ? -0.7071f : -1f;
                        player.cmd.move.X = Math.Sign(player.cmd.move.X) * 0.7071f;

                        if (player.cmd.sprintState > 0)
                            player.cmd.sprintState = SprintState.None;

                    }
                    Client.input_flags |= Client.InputBitFlag.Backward;
                }
                #endregion

                else if (Cmd.is_action(in key_code, Cmd.Mappings.jump))
                {
                    player.cmd.jump = true;

                    switch (moveState)
                    {
                        case MoveState.Ladder:
                            OnLadderEnter.Invoke(false);
                            break;
                    }
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.crouch))
                {
                    switch (moveState)
                    {
                        case MoveState.OnFoot:
                            player.cmd.stanceState = stance == StanceState.Crouching ? StanceState.Standing : StanceState.Crouching; // this is smart
                            ChangeStance(player.cmd.stanceState);
                            break;

                        case MoveState.Water:
                            player.cmd.stanceState = StanceState.Crouching;
                            break;
                    }
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.sprint) && player.cmd.move.Y > 0f)
                {

                    player.cmd.sprintState = (SprintState)((byte)(player.cmd.sprintState + 1) % 3); // and so is this

                    switch (moveState)
                    {
                        case MoveState.OnFoot:
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
                            break;
                    }

                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.prone))
                {
                    switch (moveState)
                    {
                        case MoveState.OnFoot:
                            player.cmd.stanceState = stance == StanceState.Crouching ? StanceState.Standing : StanceState.Crouching; // this is smart
                            ChangeStance(player.cmd.stanceState);
                            break;
                        case MoveState.Water:
                            player.cmd.stanceState = StanceState.Crouching;
                            break;
                        case MoveState.Ladder:
                            break;
                    }
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.use))
                {
                    if (is_near_ladder_to_activate && moveState != MoveState.Ladder)
                    {
                        OnLadderEnter.Invoke(true);
                        GD.Print("is_on_ladder");
                    }
                    else if (moveState == MoveState.Ladder)
                    {
                        OnLadderEnter.Invoke(false);
                        GD.Print("is NOT on ladder");
                    }
                }
            }
            else // if key release
            {

                #region Move Input
                if (Cmd.is_action(in key_code, Cmd.Mappings.left))
                {
                    if (Client.input_flags.HasFlag(Client.InputBitFlag.Right))
                    {
                        player.cmd.move.X = Math.Abs(player.cmd.move.Y) > 0f ? 0.7071f : 1f;
                        player.cmd.move.Y = Math.Sign(player.cmd.move.Y) * 0.7071f;
                    }
                    else
                    {
                        player.cmd.move.X = 0f;
                        player.cmd.move.Y = Math.Sign(player.cmd.move.Y);
                    }
                    Client.input_flags &= ~Client.InputBitFlag.Left;
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.right))
                {
                    if (Client.input_flags.HasFlag(Client.InputBitFlag.Left))
                    {
                        player.cmd.move.X = Math.Abs(player.cmd.move.Y) > 0f ? -0.7071f : -1f;
                        player.cmd.move.Y = Math.Sign(player.cmd.move.Y) * 0.7071f;
                    }
                    else
                    {
                        player.cmd.move.X = 0f;
                        player.cmd.move.Y = Math.Sign(player.cmd.move.Y);
                    }
                    Client.input_flags &= ~Client.InputBitFlag.Right; // remove
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.forward))
                {
                    if (Client.input_flags.HasFlag(Client.InputBitFlag.Backward))
                    {
                        player.cmd.move.Y = Math.Abs(player.cmd.move.X) > 0f ? -0.7071f : -1f;
                        player.cmd.move.X = Math.Sign(player.cmd.move.X) * 0.7071f;
                    }
                    else
                    {
                        player.cmd.move.Y = 0f;
                        player.cmd.move.X = Math.Sign(player.cmd.move.X);

                        if (player.cmd.sprintState > 0)
                        {
                            player.cmd.sprintState = SprintState.None;
                            if (isGrounded && sprintState > 0)
                                stop_sprinting();
                        }
                    }
                    Client.input_flags &= ~Client.InputBitFlag.Forward;
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.backward))
                {
                    if (Client.input_flags.HasFlag(Client.InputBitFlag.Forward))
                    {
                        player.cmd.move.Y = Math.Abs(player.cmd.move.X) > 0f ? 0.7071f : 1f;
                        player.cmd.move.X = Math.Sign(player.cmd.move.X) * 0.7071f;
                    }
                    else
                    {
                        player.cmd.move.Y = 0f;
                        player.cmd.move.X = Math.Sign(player.cmd.move.X);
                    }
                    Client.input_flags &= ~Client.InputBitFlag.Backward;
                }
                #endregion
                else if (Cmd.is_action(in key_code, Cmd.Mappings.jump))
                {
                    player.cmd.jump = false;
                }
                else if (Cmd.is_action(in key_code, Cmd.Mappings.crouch))
                {
                    switch (moveState)
                    {
                        case MoveState.OnFoot:
                            if (!player.cmd.toggle_stance)
                            {
                                player.cmd.stanceState = StanceState.Standing;
                                ChangeStance(StanceState.Standing);
                            }
                            break;

                        case MoveState.Water:
                            player.cmd.stanceState = StanceState.Standing;
                            break;
                    }
                }
            }
        }
    }

    public void PhysicsUpdate()
    {
        GlobalPosition = player.GlobalPosition;
        check_swimming();
        check_ladder();
        velocity = player.Velocity; // Taking the current velocity


        horSpeed = MathF.Sqrt(velocity.X * velocity.X + velocity.Z * velocity.Z);

        switch (moveState)
        {
            case MoveState.Water:
                doSwim();
                break;

            case MoveState.Ladder:
                doClimb();
                break;

            default:
                doMove();
                break;
        }

        collision_and_ground_check();

        player.Velocity = velocity; // Applying the newly calculated velocity, simple
        player.MoveAndSlide(); // Finally do the moving


        tac_sprint_stamina_mechanic();

    }
    public void Update()
    {
        if (Active)
        {
            switch (moveState)
            {
                case MoveState.OnFoot:
                    change_stance_mechanic();
                    CalculateDiagonalMoveDirection();
                    break;

                case MoveState.Water:
                    FreeMovementDirection();
                    break;

                case MoveState.Ladder:
                    CalculateClimbMoveDirection();
                    break;
            }
        }
    }

    #region OnFoot Movement

    public void CalculateDiagonalMoveDirection() // more efficient than using Godot's Basis infested RotateAxis mess
    {
        // Convert Euler angles to radians
        float y = player.look.Rot.X * Utilities.DegToRad; // DegToRad

        // Calculate the diagonal move direction using the Euler angles
        float cosY = MathF.Cos(y);
        float sinY = MathF.Sin(y);

        move_direction.X = cosY * player.cmd.move.X - sinY * player.cmd.move.Y;
        move_direction.Z = -sinY * player.cmd.move.X - cosY * player.cmd.move.Y;


        float length = move_direction.X * move_direction.X + move_direction.Z * move_direction.Z;
        if (length > 0f)
        {
            length = MathF.Sqrt(length);
            move_direction.X /= length;
            move_direction.Z /= length;
        }
    }

    public void doMove()
    {
        // Deacceleration and Friction
        if (isGrounded)
        {
            float control = horSpeed < data.base_deaccel ? data.base_deaccel : horSpeed;
            float drop = control * data.base_friction * Game.PhysicsDelta;
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
        weak_jump_mechanic();
        if (player.cmd.jump)
        {
            switch (stance)
            {
                case StanceState.Standing:
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
            Vector3 latest_direction = velocity / horSpeed; // the latest move direction might be outdated, hence causing false movement

            if (horSpeed > data.sprint_speed_mult)
            {
                // velocity.X = latest_direction.X * target_speed;
                // velocity.Z = latest_direction.Z * target_speed;

                velocity.X *= 0.75f;
                velocity.Z *= 0.75f;
            }

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
                    target_speed = data.base_move_speed * CurrentSpeedMult;
                    target_accel = data.base_accel * CurrentAccelMult;
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
            CurrentAccelMult = data.base_air_accel;
            target_accel = data.base_accel * CurrentAccelMult;
            #endregion

        }
    }

    #endregion

    #region Jumping

    public bool isJumping = false;
    public Action onJump;

    [Export] float weak_jump_timestamp = 0f;

    [Export] float weak_jump_duration = 0.5f;

    void weak_jump_mechanic()
    {
        if (weak_jump_timestamp < weak_jump_duration)
            weak_jump_timestamp += Game.PhysicsDelta;
    }

    public void onJumpMethod()
    {
        weak_jump_timestamp = 0f;
        velocity.Y = weak_jump_timestamp < weak_jump_duration ? data.jump_speed : data.weak_jump_speed;
        player.cmd.jump = false;
        isJumping = true;
    }

    #endregion

    #region Stance Change

    public CylinderShape3D stance_check_shape = new();
    public void ChangeStance(in StanceState new_stance)
    {
        if (new_stance == stance) return;
        byte new_stance_index = (byte)new_stance;

        ref float new_height = ref collider_heights[new_stance_index];

        if (new_stance_index < 2 && new_stance_index > 0) // rising
        {
            ShapeCaster.Origin = GlobalPosition + new Vector3(0f, new_height * 0.5f, 0f);
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
        last_stance = stance;
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
        target_speed = data.base_move_speed * CurrentSpeedMult;
    }
    public static readonly float[] stance_look_heights = new float[3] { 0f, -0.5f, -1f };
    public static readonly float[] collider_heights = new float[3] { 2f, 1f, 0.5f };

    [Export] public float current_stance_look_height = 0f;
    [Export] public float stance_change_speed = 3f;
    public StanceState last_stance = StanceState.Standing;
    void change_stance_mechanic()
    {
        if (stance == last_stance) return;

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
                    last_stance = StanceState.Standing;
                }
                break;

            case StanceState.Crouching:
                if (last_stance == StanceState.Standing)
                {
                    if (current_stance_look_height > stance_look_heights[1])
                    {
                        current_stance_look_height -= Game.Delta * stance_change_speed;
                    }
                    else
                    {
                        current_stance_look_height = stance_look_heights[1];
                        last_stance = StanceState.Crouching;
                    }
                }
                else if (last_stance == StanceState.Prone)
                {
                    if (current_stance_look_height < stance_look_heights[1])
                    {
                        current_stance_look_height += Game.Delta * stance_change_speed;
                    }
                    else
                    {
                        current_stance_look_height = stance_look_heights[1];
                        last_stance = StanceState.Crouching;
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
                    last_stance = StanceState.Prone;
                }
                break;
        }
    }

    #endregion

    #region Sprinting

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
        target_speed = data.base_move_speed * CurrentSpeedMult;
        if (isGrounded)
        {
            CurrentAccelMult = 0.7f;
            target_accel = data.base_accel * CurrentAccelMult;
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
        target_speed = data.base_move_speed * CurrentSpeedMult;

        if (isGrounded)
        {
            CurrentAccelMult = 0.5f;
            target_accel = data.base_accel * CurrentAccelMult;
        }


    }
    public void stop_sprinting()
    {
        sprintState = SprintState.None;
        player.cmd.sprintState = SprintState.None;
        CurrentSpeedMult = 1f;
        target_speed = data.base_move_speed * CurrentSpeedMult;
        if (isGrounded)
        {
            CurrentAccelMult = 1f;
            target_accel = data.base_accel * CurrentAccelMult;
        }
    }

    #endregion

    #region Ladder Climbing

    public void CalculateClimbMoveDirection()
    {
        // Convert rotation degrees to radians
        // float rotationY_rad = player.look.Rot.Y * Utilities.DegToRad; // DegToRad

        // Calculate the direction vectors for rotationX and rotationY
        float directionX = climb_direction.X;
        // float directionY = MathF.Sin(rotationY_rad);
        float directionZ = climb_direction.Z;

        // Combine the direction vectors with the movement inputs

        move_direction.X = (directionX - directionZ) * player.cmd.move.X;
        // move_direction.Y = directionY * player.cmd.move.Y;
        move_direction.Y = player.cmd.move.Y;
        move_direction.Z = (directionX - directionZ) * player.cmd.move.X;

        if (player.cmd.jump)
            move_direction.Y = 1f;

    }

    public Action<bool> OnLadderEnter;
    void OnLadderEnterMethod(bool has_entered)
    {
        if (has_entered)
        {
            moveState = MoveState.Ladder;
            player.look.ladder_rotX = MathF.Atan2(climb_direction.Z, climb_direction.X) * Utilities.RadToDeg;

            // Fixing the player Rot for ladder in here

            if (player.look.ladder_rotX > 0f)
            {
                if (player.look.Rot.X < 0f)
                {
                    player.look.Rot.X += 360f;
                }
            }
            else
            {
                if (player.look.Rot.X > 0f)
                {
                    player.look.Rot.X -= 360f;
                }
            }

        }
        else
        {
            moveState = MoveState.OnFoot;
            move_direction.Y = 0f;
        }

        player.ui.ClimbLabel.Hide();
    }
    [Export] public bool is_near_ladder_to_activate = false;
    [Export] public float ladder_check_range = 1f;
    public Vector3 climb_direction;
    public void check_ladder()
    {
        RayCaster.CollideWithAreas = true;
        RayCaster.CollideWithBodies = false;
        RayCaster.HitFromInside = true;
        RayCaster.CollisionMask = GameplayLibrary.singleton.ladder_collision_mask;
        RayCaster.AddExceptionRid(in player.rid);
        RayCaster.HitFromInside = false;
        RayCaster.Origin = GlobalPosition;


        // Stop climbing if the player is not touching the ladder anymore

        if (moveState == MoveState.Ladder)
        {
            RayCaster.TargetPosition = new(climb_direction.X * ladder_check_range, collider_heights[0], climb_direction.Z * ladder_check_range);
            RayCaster.ForceRaycastUpdate();
            if (!RayCaster.IsColliding)
            {
                OnLadderEnter.Invoke(false);
            }
            else
            {
                climb_direction = -RayCaster.CollisionNormal; // setting it every frame in case the ladder is moving or rotating
            }
        }
        else
        {
            RayCaster.TargetPosition = new(player.look.direction.X * ladder_check_range, collider_heights[0], player.look.direction.Z * ladder_check_range);

            RayCaster.ForceRaycastUpdate();
            if (RayCaster.IsColliding)
            {
                if (!is_near_ladder_to_activate)
                {
                    is_near_ladder_to_activate = true;
                    GD.Print("near ladder");
                    climb_direction = -RayCaster.CollisionNormal; // setting it upon activating climbing
                    player.ui.ClimbLabel.Show();
                }
            }
            else if (is_near_ladder_to_activate)
            {
                is_near_ladder_to_activate = false;
                player.cmd.jump = false;
                GD.Print("NOT near ladder");
                player.ui.ClimbLabel.Hide();
            }
        }

        RayCaster.ClearExceptions();
    }
    public void doClimb()
    {
        float current_speed = MathF.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y + velocity.Z * velocity.Z);
        // Deacceleration and Friction
        float control = current_speed < data.base_deaccel ? data.base_deaccel : current_speed;
        float drop = control * data.base_friction * Game.PhysicsDelta;
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

    #endregion

    #region Water Swimming

    public Action<bool> OnEnterWater;

    public void OnWaterEnterMethod(bool has_entered)
    {
        if (has_entered)
        {
            moveState = MoveState.Water;
            stop_sprinting();
        }
        else
        {
            moveState = MoveState.OnFoot;
            player.cmd.jump = false;
            move_direction.Y = 0f;
        }
    }
    public void check_swimming()
    {
        RayCaster.CollideWithAreas = true;
        RayCaster.CollideWithBodies = false;
        RayCaster.HitFromInside = true;
        RayCaster.CollisionMask = GameplayLibrary.singleton.water_collision_mask;
        RayCaster.AddExceptionRid(in player.rid);
        Vector3 collider_height_vec = new(0f, collider_heights[(byte)stance], 0f);
        RayCaster.Origin = GlobalPosition + collider_height_vec;
        RayCaster.TargetPosition = -collider_height_vec;

        RayCaster.ForceRaycastUpdate();
        RayCaster.ClearExceptions();
        RayCaster.HitFromInside = false;
        if (RayCaster.IsColliding)
        {
            if (moveState != MoveState.Water)
            {
                OnWaterEnterMethod(true);
            }
        }
        else if (moveState == MoveState.Water)
        {
            OnWaterEnterMethod(false);

        }
    }
    public void doSwim()
    {
        float current_speed = MathF.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y + velocity.Z * velocity.Z);
        // Deacceleration and Friction
        float control = current_speed < data.base_deaccel ? data.base_deaccel : current_speed;
        float drop = control * data.base_friction * Game.PhysicsDelta;
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

    #endregion

    public static Vector3 CalculateDiagonalMoveDirection(in float horizontal_rotation_degrees, in float horizontal, in float vertical) // more efficient than using Godot's Basis infested RotateAxis mess
    {
        // ConvertEuler angles to radians
        float y = horizontal_rotation_degrees * Utilities.DegToRad; // DegToRad

        // Calculate the diagonal move direction using the Euler angles
        float cosY = MathF.Cos(y);
        float sinY = MathF.Sin(y);

        return new Vector3(
            cosY * horizontal - sinY * vertical,
            0f,
           -sinY * horizontal - cosY * vertical
            ).Normalized();
    }

    public void FreeMovementDirection()
    {
        // Convert rotation degrees to radians
        float rotationX_rad = player.look.Rot.X * Utilities.DegToRad; // DegToRad
        float rotationY_rad = player.look.Rot.Y * Utilities.DegToRad; // DegToRad

        float cosY = -MathF.Cos(rotationY_rad);

        // Calculate the direction vectors for rotationX and rotationY
        float directionX = MathF.Sin(rotationX_rad) * cosY;
        float directionY = MathF.Sin(rotationY_rad);
        float directionZ = MathF.Cos(rotationX_rad) * cosY;

        // Combine the direction vectors with the movement inputs

        move_direction.X = directionX * player.cmd.move.Y - directionZ * player.cmd.move.X;
        move_direction.Y = directionY * player.cmd.move.Y;
        move_direction.Z = directionZ * player.cmd.move.Y + directionX * player.cmd.move.X;

        Utilities.Normalize(ref move_direction);
        move_direction.Y += Convert.ToSingle(player.cmd.jump) - Convert.ToSingle(player.cmd.stanceState == StanceState.Crouching);

    }


    [ExportGroup("NodePaths")]
    [Export]
    public NodePath
        world_collider_path;
}