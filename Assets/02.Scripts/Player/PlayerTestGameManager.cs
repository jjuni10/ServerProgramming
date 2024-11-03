using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTestGameManager : MonoBehaviour
{
    public GameObject Gunner;
    public GameObject Runner;

    public GameObject Coin;
    public GameObject Bomb;

    private Vector3 _randomCoinPos;
    private Vector3 _randomBombPos;
    private int _ranPosX;
    private int _ranPosZ;

    List<GameObject> Coins = new List<GameObject>();
    List<GameObject> Bombs = new List<GameObject>();

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Z))
        {
            SpreadCoin();
        }
        if (Input.GetKeyUp(KeyCode.X))
        {
            SpreadBomb();
        }
    }

    public void SpreadCoin()
    {
        while (true)
        {
            _ranPosX = UnityEngine.Random.Range(-60, 60);
            _ranPosZ = UnityEngine.Random.Range(-60, 60);
            if (Math.Abs(_ranPosX) + Math.Abs(_ranPosZ) <= 60) break;
        }

        _randomCoinPos = new Vector3(_ranPosX, 3, _ranPosZ);

        var Coin = Resources.Load("Coin");
        Coins.Add(Instantiate(Coin, _randomCoinPos, Quaternion.identity) as GameObject);
    }
    
    public void SpreadBomb()
    {
        while (true)
        {
            _ranPosX = UnityEngine.Random.Range(-60, 60);
            _ranPosZ = UnityEngine.Random.Range(-60, 60);
            if (Math.Abs(_ranPosX) + Math.Abs(_ranPosZ) <= 60) break;
        }

        _randomBombPos = new Vector3(_ranPosX, 3, _ranPosZ);

        var Bomb = Resources.Load("Bomb");
        Bombs.Add(Instantiate(Bomb, _randomBombPos, Quaternion.identity) as GameObject);
    }
}
