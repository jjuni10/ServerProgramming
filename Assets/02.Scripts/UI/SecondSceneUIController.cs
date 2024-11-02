using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SecondSceneUIController : MonoBehaviour
{
    private bool isStartGame = false;
    private bool isEndGame = false;

    public TMP_Text CountNum;
    private Coroutine count_Co = null;

    public float PlayTime = 180f;  // 게임 플레이 시간 (유저 기준)
    public float SettingPlayTime = 180f;   // 셋팅된 플레이 시간 (현재 3분)
    public Image GaugePlayTime; // 플레이 시간 소비 게이지

    void Start()
    {
        StartCoroutine(StartCount());
    }

    void Update()
    {
        
    }

    void FixedUpdate() 
    {
        if (isEndGame || !isStartGame) return;
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
                //! 게임 시작 패킷 전송? 시작한거 알려야할듯
            }
            time += Time.deltaTime;
            yield return null;
        }
        CountNum.gameObject.SetActive(false);
    }
    IEnumerator PlayTimeCalculate()
    {
        isStartGame = true;
        while (!isEndGame)
        {
            if (PlayTime <= 0) 
            {
                isEndGame = true;
                CountNum.gameObject.SetActive(true);
                CountNum.text = "끝~~";
            }
            PlayTime -= Time.deltaTime;
            yield return null;
        }
    }
}
