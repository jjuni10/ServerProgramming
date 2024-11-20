using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public GameObject[] prefabs;
    public Transform[] roots;
    List<GameObject>[] pools;

    void Start()
    {
        pools = new List<GameObject>[prefabs.Length];

        for (int index = 0; index < pools.Length; index++)
        {
            pools[index] = new List<GameObject>();
        }
    }

    public GameObject Get(int index, Vector3 position)
    {
        GameObject select = null;

        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                select = item;
                break;
            }
        }
        if (!select)
        {
            select = Instantiate(prefabs[index], roots[index]);
            select.transform.position = position;
            pools[index].Add(select);
        }
        return select;
    }
}
