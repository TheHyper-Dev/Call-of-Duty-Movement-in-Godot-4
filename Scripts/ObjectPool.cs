using System;
using System.Collections.Generic;
using Godot;

public class ObjectPool<T> where T : class
{
    public HashSet<T> active_pool;
    public Queue<T> inactive_pool;
    public Func<T> instantiate_method;

    public void Initialize(Func<T> instantiate_method, in int warm_capacity = 30)
    {
        active_pool = new(warm_capacity);
        inactive_pool = new(warm_capacity);
        this.instantiate_method = instantiate_method;
        int i = 0;
        do
        {
            inactive_pool.Enqueue(instantiate_method.Invoke());
            ++i;
        } while (i < warm_capacity);
    }

    public virtual void Add(out T thing)
    {
        if (inactive_pool.Count > 0)
        {
            GD.Print("Dequeing from the inactive queue");
            thing = inactive_pool.Dequeue();
        }
        else
        {
            GD.Print("inactive queue is empty, creating a new one");
            thing = instantiate_method.Invoke();
        }
        active_pool.Add(thing);
    }

    public virtual void Remove(T thing)
    {
        inactive_pool.Enqueue(thing);
        active_pool.Remove(thing);
    }
}