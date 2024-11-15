using System.Collections.Concurrent;
using UnityEngine;

public class MainThread : MonoBehaviour
{
    private static MainThread _instance;
    public static MainThread Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            Init();
        }
        else
        {
            Destroy(this);
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