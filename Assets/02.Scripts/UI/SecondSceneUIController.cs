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

    public TMP_Text Player1ID;
    public TMP_Text Player1Point;

    public TMP_Text Player2ID;
    public TMP_Text Player2Point;

    public TMP_Text Player3ID;
    public TMP_Text Player3Point;
    
    public TMP_Text Player4ID;
    public TMP_Text Player4Point;

    void Start()
    {
        //todo: get each Player's ID, And set ID

        StartCoroutine(StartCount());
    }

    void Update()
    {
        
    }

    void FixedUpdate() 
    {
        if (GameManager.Instance.IsGameEnd || !GameManager.Instance.IsGameStarted) return;
        //todo: "PlayTime" set scroll value
        GaugePlayTime.fillAmount = PlayTime / SettingPlayTime;

        //todo: get each Player's Point And set Point

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
                //! 게임 진짜 시작 패킷 전송? 시작한거 알려야할듯
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
                CountNum.text = "끝~~";
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
                    Player1ID.text = "<" + player.ID + ">";
                }
                break;
            case 2:
                {
                    Player2ID.text = "<" + player.ID + ">";
                }
                break;
            case 3:
                {
                    Player3ID.text = "<" + player.ID + ">";
                }
                break;
            case 4:
                {
                    Player4ID.text = "<" + player.ID + ">";
                }
                break;
            default:
                break;
        }

    }

    public void SetPointUI(int uid)
    {
        //Debug.Log($"SetReadyUI({uid})");
        Player player = GameManager.Instance.GetPlayer(uid);
        switch (uid+1)
        {
            case 1:
                {
                    Player1Point.text = player._currentValue.point.ToString() + " Point";
                }
                break;
            case 2:
                {
                    Player2Point.text = player._currentValue.point.ToString() + " Point";
                }
                break;
            case 3:
                {
                    Player3Point.text = player._currentValue.point.ToString() + " Point";
                }
                break;
            case 4:
                {
                    Player4Point.text = player._currentValue.point.ToString() + " Point";
                }
                break;
            default:
                break;
        }

    }
}
