using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SecondSceneUIController : MonoBehaviour
{
    public TMP_Text CountNum;
    private Coroutine count_Co = null;

    public float PlayTime = 180f;  // 게임 플레이 시간 (유저 기준)
    public float SettingPlayTime = 180f;   // 셋팅된 플레이 시간 (현재 3분)
    public Image GaugePlayTime; // 플레이 시간 소비 게이지

    public TMP_Text PlayerRed1ID;
    public TMP_Text PlayerRed1Point;

    public TMP_Text PlayerRed2ID;
    public TMP_Text PlayerRed2Point;

    public TMP_Text PlayerBlue1ID;
    public TMP_Text PlayerBlue1Point;

    public TMP_Text PlayerBlue2ID;
    public TMP_Text PlayerBlue2Point;

    void Start()
    {
        StartCoroutine(StartCount());
    }

    void Update()
    {
        
    }

    void FixedUpdate() 
    {
        //if (GameManager.Instance.IsGameEnd || !GameManager.Instance.IsGameStarted) return;
        //todo: "PlayTime" set scroll value
        GaugePlayTime.fillAmount = PlayTime / SettingPlayTime;
    }

    IEnumerator StartCount()
    {
        // Whan start game, counting 3,2,1,start!
        float time = 0;

        while (time < 4)
        {
            if (time < 1)   // 0~1
            {
                CountNum.text = "3";
            }
            else if (time < 2)   // 1~2
            {
                CountNum.text = "2";
            }
            else if (time < 3)   // 2~3
            {
                CountNum.text = "1";
            }
            else    //3~
            {
                CountNum.text = "게임 시작!!";
                if (count_Co == null)
                    count_Co = StartCoroutine(PlayTimeCalculate());
                GameManager.Instance.IsGamePlayOn = true;
            }
            time += Time.deltaTime;
            yield return null;
        }
        CountNum.gameObject.SetActive(false);
    }
    IEnumerator PlayTimeCalculate()
    {
        while (!GameManager.Instance.IsGameEnd)
        {
            if (PlayTime <= 0) 
            {
                CountNum.gameObject.SetActive(true);
                CountNum.text = "종료~~";
                GameManager.Instance.IsGameEnd = false;
            }
            PlayTime -= Time.deltaTime;
            yield return null;
        }
    }

    public void SetIDUI(int uid)
    {
        //Debug.Log($"SetReadyUI({uid})");
        Player player = GameManager.Instance.GetPlayer(uid);
        switch (uid+1)
        {
            case 1:
                {
                    PlayerRed1ID.text = "<" + player.ID + ">";
                }
                break;
            case 3:
                {
                    PlayerRed2ID.text = "<" + player.ID + ">";
                }
                break;
            case 2:
                {
                    PlayerBlue1ID.text = "<" + player.ID + ">";
                }
                break;
            case 4:
                {
                    PlayerBlue2ID.text = "<" + player.ID + ">";
                }
                break;
            default:
                break;
        }

    }

    public void SetPointUI(int uid, bool isReset = false, int point = 0)
    {
        //Debug.Log($"SetReadyUI({uid})");
        Player player = GameManager.Instance.GetPlayer(uid);
        player._currentValue.point = player._currentValue.point + point;
        if (isReset) player._currentValue.point = 0;
        switch (uid+1)
        {
            case 1:
                {
                    PlayerRed1Point.text = player._currentValue.point.ToString() + " Point";
                }
                break;
            case 3:
                {
                    PlayerRed2Point.text = player._currentValue.point.ToString() + " Point";
                }
                break;
            case 2:
                {
                    PlayerBlue1Point.text = player._currentValue.point.ToString() + " Point";
                }
                break;
            case 4:
                {
                    PlayerBlue2Point.text = player._currentValue.point.ToString() + " Point";
                }
                break;
            default:
                break;
        }

    }
}
