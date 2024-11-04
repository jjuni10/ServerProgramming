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
        //! 서버에서 시간 체크하고 아이템 뿌려야 할 거 같아서 리턴값으로 위치값 넣음
        if (Input.GetKeyUp(KeyCode.Z))
        {
            SpreadCoin();
        }
        if (Input.GetKeyUp(KeyCode.X))
        {
            SpreadBomb();
        }
    }

    public Vector3 SpreadCoin()
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
        return _randomCoinPos;
    }

    public Vector3 SpreadBomb()
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
        return _randomBombPos;
    }
}
