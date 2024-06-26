using System;
using System.Text;
using Godot;
public static class Utilities
{
    public static void SetNode<T>(this Node parent, out T node, NodePath nodepath) where T : Node => node = (T)parent.GetNode(nodepath);
    public static Vector2 LissajousCurve(in float Time) => new(MathF.Sin(Time), MathF.Sin(2f * Time + MathF.PI));
    public const float pos_rot_balancer_ratio = 0.0125f; // or divide it by 80f, same thing
    public const float DegToRad = 0.01745329251994329576923690768489f;
    public const float RadToDeg = 57.295779513082320876798154814105f;
    public static float AngleDifference(in float from, in float to)
    {
        float num = (to - from) % 360f;
        return 2f * num % 360f - num;
    }
    public static float LerpAngleDegrees(in float from, in float to, in float weight)
    {
        return from + AngleDifference(from, to) * weight;
    }
    // More efficient than using Godot's -GlobalTransform.Basis.Z
    public static Vector3 DegreesToDirection(in float horizontal_degree, in float vertical_degree)
    {
        // Convert angles to radians
        float angleX = horizontal_degree * DegToRad;
        float angleY = vertical_degree * DegToRad;
        float cos_ver = MathF.Cos(angleY);

        return new Vector3(
        -MathF.Sin(angleX) * cos_ver,
        MathF.Sin(angleY),
        -MathF.Cos(angleX) * cos_ver
        ).Normalized();

    }
    public static void Normalize(ref Vector3 vector3)
    {

        float length = vector3.X * vector3.X + vector3.Y * vector3.Y + vector3.Z * vector3.Z;
        if (length > 0f)
        {
            length = MathF.Sqrt(length);
            vector3.X /= length;
            vector3.Y /= length;
            vector3.Z /= length;
        }
        else
        {
            vector3.X = 0f;
            vector3.Y = 0f;
            vector3.Z = 0f;
            return;
        }

    }

}
public sealed class RenderTimer
{
    public float time_left;
    public bool is_playing;
    public Action on_finish;
    public static RenderTimer Create() => new() { is_playing = false };
    public void RenderTick()
    {
        if (time_left > 0f)
        {
            time_left -= Game.Delta;
        }
        else
        {
            time_left = 0f;
            is_playing = false;
            on_finish?.Invoke();
        }
    }
}

public sealed class PhysicsTimer
{
    public float time_left;
    public bool is_playing;
    public Action on_finish;
    public static PhysicsTimer Create() => new() { is_playing = false };
    public void PhysicsTick()
    {
        if (time_left > 0f)
        {
            time_left -= Game.PhysicsDelta;
        }
        else
        {
            time_left = 0f;
            is_playing = false;
            on_finish?.Invoke();
        }
    }
}

public static class RayCaster
{
    public static RayCast3D instance = new() { Enabled = false, ExcludeParent = false };
    public static bool CollideWithAreas { get => instance.CollideWithAreas; set => instance.CollideWithAreas = value; }
    public static bool CollideWithBodies { get => instance.CollideWithBodies; set => instance.CollideWithBodies = value; }
    public static uint CollisionMask { get => instance.CollisionMask; set => instance.CollisionMask = value; }
    public static Vector3 TargetPosition { get => instance.TargetPosition; set => instance.TargetPosition = value; }
    public static Vector3 Origin { get => instance.GlobalPosition; set => instance.GlobalPosition = value; }
    public static bool HitBackFaces { get => instance.HitBackFaces; set => instance.HitBackFaces = value; }
    public static bool HitFromInside { get => instance.HitFromInside; set => instance.HitFromInside = value; }
    public static bool IsColliding { get => instance.IsColliding(); }
    public static Vector3 CollisionNormal => instance.GetCollisionNormal();
    public static Vector3 CollisionPoint => instance.GetCollisionPoint();
    public static GodotObject GetCollider() => instance.GetCollider();
    public static T GetCollider<T>() where T : GodotObject => (T)instance.GetCollider();
    public static void ForceRaycastUpdate() => instance.ForceRaycastUpdate();
    public static bool Cast()
    {
        instance.ForceRaycastUpdate();
        return instance.IsColliding();
    }
    public static bool Cast(in bool hit_body_or_area)
    {
        CollideWithBodies = hit_body_or_area;
        CollideWithAreas = !hit_body_or_area;
        instance.ForceRaycastUpdate();
        return instance.IsColliding();
    }
    public static bool Cast(in uint collision_mask, in bool hit_body_or_area)
    {
        CollisionMask = collision_mask;
        CollideWithBodies = hit_body_or_area;
        CollideWithAreas = !hit_body_or_area;
        instance.ForceRaycastUpdate();
        return instance.IsColliding();
    }
    public static void AddExceptionRid(in Rid rid) => instance.AddExceptionRid(rid);
    public static void RemoveExceptionRid(in Rid rid) => instance.RemoveExceptionRid(rid);
    public static void RemoveExceptionRids(in Span<Rid> rids)
    {
        int amount = rids.Length;
        for (int i = 0; i < amount; ++i)
        {
            instance.RemoveExceptionRid(rids[i]);
        }
    }
    public static void AddExceptionRids(in Span<Rid> rids)
    {
        int amount = rids.Length;
        for (int i = 0; i < amount; ++i)
        {
            instance.AddExceptionRid(rids[i]);
        }
    }

    public static void ClearExceptions() => instance.ClearExceptions();
    public static void SetCollisionMaskValue(in int layer_number, in bool enable) => instance.SetCollisionMaskValue(layer_number, enable);
    public static bool GetCollisionMaskValue(in int layer_number) => instance.GetCollisionMaskValue(layer_number);
}

public static class ShapeCaster
{
    public static ShapeCast3D instance = new() { Enabled = false, ExcludeParent = false, TargetPosition = Vector3.Zero };
    public static Shape3D Shape { get => instance.Shape; set => instance.Shape = value; }
    public static bool CollideWithAreas { get => instance.CollideWithAreas; set => instance.CollideWithAreas = value; }
    public static bool CollideWithBodies { get => instance.CollideWithBodies; set => instance.CollideWithBodies = value; }
    public static uint CollisionMask { get => instance.CollisionMask; set => instance.CollisionMask = value; }
    public static int MaxResults { get => instance.MaxResults; set => instance.MaxResults = value; }
    public static Vector3 TargetPosition { get => instance.TargetPosition; set => instance.TargetPosition = value; }
    public static Vector3 Origin { get => instance.GlobalPosition; set => instance.GlobalPosition = value; }
    public static bool IsColliding { get => instance.IsColliding(); }
    public static int CollisionCount { get => instance.GetCollisionCount(); }
    public static Vector3 CollisionNormal(in int index) => instance.GetCollisionNormal(index);
    public static Vector3 CollisionPoint(in int index) => instance.GetCollisionPoint(index);
    public static GodotObject GetCollider(in int index) => instance.GetCollider(index);
    public static T GetCollider<T>(in int index) where T : GodotObject => (T)instance.GetCollider(index);
    public static void ForceShapecastUpdate() => instance.ForceShapecastUpdate();
    public static void AddExceptionRid(in Rid rid) => instance.AddExceptionRid(rid);
    public static void RemoveExceptionRid(in Rid rid) => instance.RemoveExceptionRid(rid);
    public static void RemoveExceptionRids(in Span<Rid> rids)
    {
        int amount = rids.Length;
        for (int i = 0; i < amount; ++i)
        {
            instance.RemoveExceptionRid(rids[i]);
        }
    }
    public static void AddExceptionRids(in Span<Rid> rids)
    {
        int amount = rids.Length;
        for (int i = 0; i < amount; ++i)
        {
            instance.AddExceptionRid(rids[i]);
        }
    }
    public static void ClearExceptions() => instance.ClearExceptions();
    public static void SetCollisionMaskValue(in int layer_number, in bool enable) => instance.SetCollisionMaskValue(layer_number, enable);
    public static bool GetCollisionMaskValue(in int layer_number) => instance.GetCollisionMaskValue(layer_number);
}

public static class Constants
{
    public static class PlayerUI
    {
        public static string ClimbHintText;

        public static void Init()
        {
            StringBuilder keys = new(200);

            int length = Cmd.keyboard_inputs[Cmd.Mappings.use].Length;

            for (int i = 0; i < length; i++)
            {
                keys.Append("[color=#e6991d]");
                keys.Append(OS.GetKeycodeString(Cmd.keyboard_inputs[Cmd.Mappings.use][i]));
                if (length - 1 > i)
                    keys.Append(" [color=#ffffff]or [color=#e6991d]");
                else
                    keys.Append(" [color=#ffffff]");
            }
            ClimbHintText = $"Press {keys} to climb";
            keys.Clear();
            GD.Print("keys capacity = " + keys.Capacity);
        }
    }

    public static void Init()
    {
        PlayerUI.Init();
    }
}