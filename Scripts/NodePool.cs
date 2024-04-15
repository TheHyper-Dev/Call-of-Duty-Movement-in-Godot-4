using System;
using Godot;

public sealed class NodePool<T> : ObjectPool<T> where T : Node
{
    public Node Parent;
    public override void Add(out T Node)
    {
        base.Add(out Node);
        Parent.AddChild(Node);
    }
    public override void Remove(T thing)
    {
        base.Remove(thing);
        Parent.RemoveChild(thing);
    }
}