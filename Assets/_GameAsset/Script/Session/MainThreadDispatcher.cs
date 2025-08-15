using System;
using System.Collections.Concurrent;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> _actions = new();

    public static void Enqueue(Action action)
    {
        _actions.Enqueue(action);
    }

    void Update()
    {
        while (_actions.TryDequeue(out var action))
        {
            action.Invoke(); // Gọi trên main thread
        }
    }
}
