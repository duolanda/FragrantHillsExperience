using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> actions = new Queue<Action>();

    private void Update()
    {
        while (actions.Count > 0)
        {
            actions.Dequeue().Invoke();
        }
    }

    public static void ExecuteOnMainThread(Action action)
    {
        if (action == null)
        {
            return;
        }

        lock (actions)
        {
            actions.Enqueue(action);
        }
    }
}