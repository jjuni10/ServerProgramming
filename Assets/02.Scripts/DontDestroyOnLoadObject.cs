using System.Collections;
using UnityEngine;

public class DontDestroyOnLoadObject : MonoBehaviour
{
    private static DontDestroyOnLoadObject instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}