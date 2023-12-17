using Godot;
using System;

[GlobalClass]
public sealed partial class Movement : Resource
{
    public PlayerController controller;
    public CollisionShape3D world_collider;
    [Export] public bool move_enabled = true;

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
    public void Init(PlayerController controller)
    {
        this.controller = controller;
        world_collider = (CollisionShape3D)controller.GetNode(controller.world_collider_path);
        OnGrounded += OnGroundedMethod;
        onJump += onJumpMethod;
        Godot.Collections.Array<Rid> playerRid = new(new Rid[] { controller.GetRid() });

        stance_check_ray_query.Exclude = playerRid;
        stance_check_ray_query.CollisionMask = 1u;

        target_speed = controller.move_data.move_speed_base;
    }



    public void UnhandledInput(InputEvent input)
    {
        if (!move_enabled) return;
        controller.PlayerInput.move = Input.GetVector(PlayerInput.Mappings.left, PlayerInput.Mappings.right, PlayerInput.Mappings.forward, PlayerInput.Mappings.backward);
        move_direction = CalculateDiagonalMoveDirection(controller.PlayerInput.move.X, controller.PlayerInput.move.Y);
        if (!input.IsEcho()) // not holding down
        {
            if (input.IsPressed())
            {
                if (input.IsAction(PlayerInput.Mappings.jump))
                {
                    controller.PlayerInput.jump = true;
                }
                else if (input.IsAction(PlayerInput.Mappings.crouch))
                {
                    ChangeStance(stance == StanceState.Crouching ? StanceState.Standing : StanceState.Crouching);
                }
                else if (input.IsAction(PlayerInput.Mappings.prone))
                {
                    ChangeStance(stance == StanceState.Prone ? StanceState.Standing : StanceState.Prone);
                }
            }
            else
            {
                if (input.IsAction(PlayerInput.Mappings.jump))
                {
                    controller.PlayerInput.jump = false;
                }
            }
        }
    }
    public void PhysicsUpdate(in float dt)
    {
        collision_and_ground_check();

        velocity = controller.Velocity; // Taking the current velocity
        doMove(in dt); // processing it
        controller.Velocity = velocity; // Applying the newly calculated velocity, simple
        controller.MoveAndSlide();
    }
    public void Update(in float dt)
    {
        Position = controller.Position;

        change_stance_mechanic(in dt);
    }
    public void doMove(in float dt)
    {

        // Deacceleration and Friction
        if (isGrounded)
        {
            horSpeed = MathF.Sqrt(velocity.X * velocity.X + velocity.Z * velocity.Z);
            float control = horSpeed < controller.move_data.deaccel ? controller.move_data.deaccel : horSpeed;
            float drop = control * controller.move_data.friction * dt;
            float newspeed = horSpeed - drop;

            if (newspeed < 0f)
                newspeed = 0f;
            if (horSpeed > 0f)
                newspeed /= horSpeed;

            velocity.X *= newspeed;
            velocity.Z *= newspeed;
            ground_move(in dt);
        }
        else
        {
            velocity.Y -= controller.move_data.gravity * dt;
            air_move(in dt);
        }
        // Acceleration

        float currentspeed = velocity.X * move_direction.X + velocity.Z * move_direction.Z; // Basically Vector2 dot product
        float addspeed = target_speed - currentspeed;
        if (addspeed <= 0f)
            return;
        float accelspeed = target_accel * dt * target_speed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;


        velocity.X += accelspeed * move_direction.X;
        velocity.Z += accelspeed * move_direction.Z;



    }
    void ground_move(in float dt)
    {
        if (controller.PlayerInput.jump) // Jump Input check
        {
            switch (stance)
            {
                case StanceState.Standing:
                    onJump.Invoke();
                    break;

                case StanceState.Crouching:
                    ChangeStance(StanceState.Standing);
                    controller.PlayerInput.jump = false;
                    break;

                default:
                    ChangeStance(StanceState.Crouching);
                    controller.PlayerInput.jump = false;
                    break;
            }
        }

    }
    void air_move(in float dt)
    {
    }
    public bool isJumping = false;
    public Action onJump;

    public void onJumpMethod()
    {
        velocity.Y = controller.move_data.jump_speed;
        controller.PlayerInput.jump = false;
        isJumping = true;
    }
    public KinematicCollision3D[] collisions;
    public int ground_collision_index;

    [Export] public bool isGrounded = false;
    public Action<bool> OnGrounded;
    void collision_and_ground_check()
    {
        int col_count = controller.GetSlideCollisionCount();
        collisions = new KinematicCollision3D[col_count];
        int i;
        for (i = 0; i < col_count; i++)
        {
            collisions[i] = controller.GetSlideCollision(i);
        }
        bool really_is_grounded = false;
        for (i = 0; i < col_count; i++)
        {

            if (collisions[i].GetAngle() <= controller.FloorMaxAngle)
            {
                ground_collision_index = i;
                really_is_grounded = true;
                break;
            }

        }
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
            switch (controller.PlayerInput.sprintState)
            {
                case SprintState.None: // is already not sprinting, just setting the speed and accel values
                    switch (stance)
                    {
                        case StanceState.Standing:
                            CurrentSpeedMult = 1f;
                            CurrentAccelMult = 1f;
                            break;
                        case StanceState.Crouching:
                            CurrentSpeedMult = controller.move_data.move_speed_crouch_mult;
                            CurrentAccelMult = 1f;
                            break;
                        default: // Prone
                            CurrentSpeedMult = controller.move_data.move_speed_prone_mult;
                            CurrentAccelMult = 1f;
                            break;
                    }
                    target_speed = controller.move_data.move_speed_base * CurrentSpeedMult;
                    target_accel = controller.move_data.accel * CurrentAccelMult;
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
            sprintState = SprintState.None;
            controller.PlayerInput.sprintState = SprintState.None;
            CurrentAccelMult = controller.move_data.air_accel;
            target_accel = controller.move_data.accel * CurrentAccelMult;
            // basically stop_sprinting() except it doesn't modify the speed
        }
    }

    public PhysicsRayQueryParameters3D stance_check_ray_query = new();
    public void ChangeStance(StanceState new_stance)
    {
        if (new_stance == stance) return;
        CapsuleShape3D collider_shape = (CapsuleShape3D)world_collider.Shape;

        float new_height = collider_heights[(byte)new_stance];

        if ((byte)new_stance < 2 && (byte)stance > 0) // rising
        {
            Vector3 collider_height = new(0f, new_height + collider_shape.Margin, 0f);

            stance_check_ray_query.From = Position;
            stance_check_ray_query.To = Position + collider_height;
            Godot.Collections.Dictionary hit_results = GameManager.space_state.IntersectRay(stance_check_ray_query);

            if (hit_results.Count > 0)
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
                CurrentSpeedMult = (controller.move_data.move_speed_crouch_mult);
                break;
            default:
                CurrentSpeedMult = (controller.move_data.move_speed_prone_mult);
                break;
        }
        target_speed = controller.move_data.move_speed_base * CurrentSpeedMult;
    }
    public static readonly float[] stance_look_heights = new float[3] { 0f, -0.5f, -1f };
    public static readonly float[] collider_heights = new float[3] { 2f, 1f, 0.5f };

    [Export] public float current_stance_look_height = 0f;
    [Export] public float stance_change_speed = 3f;
    public StanceState previous_stance = StanceState.Standing;
    void change_stance_mechanic(in float delta)
    {
        if (stance == previous_stance) return;

        switch (stance)
        {
            case StanceState.Standing:
                if (current_stance_look_height < stance_look_heights[0])
                {
                    current_stance_look_height += delta * stance_change_speed;
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
                        current_stance_look_height -= delta * stance_change_speed;
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
                        current_stance_look_height += delta * stance_change_speed;
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
                    current_stance_look_height -= delta * stance_change_speed;
                }
                else
                {
                    current_stance_look_height = stance_look_heights[2];
                    previous_stance = StanceState.Prone;
                }
                break;
        }
    }
    public void start_sprinting()
    {
        if (stance > 0)
        {
            ChangeStance(StanceState.Standing);
        }
        sprintState = SprintState.Sprinting;
        controller.PlayerInput.sprintState = SprintState.Sprinting;
        CurrentSpeedMult = 0.5f * (controller.move_data.sprint_speed_mult);
        target_speed = controller.move_data.move_speed_base * CurrentSpeedMult;
        if (isGrounded)
        {
            CurrentAccelMult = 0.7f;
            target_accel = controller.move_data.accel * CurrentAccelMult;
        }
    }
    public void start_tac_sprinting()
    {

        if (stance > 0)
        {
            ChangeStance(StanceState.Standing);
        }
        sprintState = SprintState.TacSprinting;
        controller.PlayerInput.sprintState = SprintState.TacSprinting;
        CurrentSpeedMult = 0.5f * (controller.move_data.tac_sprint_speed_mult);
        target_speed = controller.move_data.move_speed_base * CurrentSpeedMult;

        if (isGrounded)
        {
            CurrentAccelMult = 0.5f;
            target_accel = controller.move_data.accel * CurrentAccelMult;
        }


    }
    public void stop_sprinting()
    {
        sprintState = SprintState.None;
        controller.PlayerInput.sprintState = SprintState.None;
        CurrentSpeedMult = 1f;
        target_speed = controller.move_data.move_speed_base * CurrentSpeedMult;
        if (isGrounded)
        {
            CurrentAccelMult = 1f;
            target_accel = controller.move_data.accel * CurrentAccelMult;
        }
    }
    Vector3 CalculateDiagonalMoveDirection(float horizontal, float vertical) // more efficient than using Godot's Basis infested RotateAxis mess
    {
        // Convert Euler angles to radians
        float y = controller.look.Rot.X * GameManager.DegToRad; // DegToRad

        // Calculate the diagonal move direction using the Euler angles
        float cosY = MathF.Cos(y);
        float sinY = MathF.Sin(y);

        return new Vector3(
            cosY * horizontal + sinY * vertical,
            0f,
            -sinY * horizontal + cosY * vertical
            ).Normalized();
    }
}