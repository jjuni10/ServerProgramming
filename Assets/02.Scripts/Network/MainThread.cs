using System.Collections.Concurrent;
using UnityEngine;

public class MainThread : MonoBehaviour
{
    private static MainThread _instance;

    public static MainThread Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject container = new GameObject("MainThread");
                _instance = container.AddComponent<MainThread>();
            }

            return _instance;
        }
    }

    private ConcurrentQueue<System.Action> _executionQueue;

    public void Init()
    {
        _executionQueue = new ConcurrentQueue<System.Action>();
    }

    private void Update()
    {
        while (_executionQueue.TryDequeue(out System.Action execution))
        {
            execution?.Invoke();
        }
    }

    public void Add(System.Action execution)
    {
        _executionQueue.Enqueue(execution);
    }
}