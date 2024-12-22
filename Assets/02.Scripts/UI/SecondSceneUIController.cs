using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SecondSceneUIController : MonoBehaviour
{
    public TMP_Text CountNum;
    private Coroutine count_Co = null;

    public float PlayTime;   // 게임 플레이 시간 
    public Image GaugePlayTime; // 플레이 시간 소비 게이지

    public TMP_Text PlayerRed1ID;
    public TMP_Text PlayerRed1Point;
    public int[] points = new int[4];

    public TMP_Text PlayerRed2ID;
    public TMP_Text PlayerRed2Point;

    public TMP_Text PlayerBlue1ID;
    public TMP_Text PlayerBlue1Point;

    public TMP_Text PlayerBlue2ID;
    public TMP_Text PlayerBlue2Point;

    void Start()
    {
        StartCoroutine(StartCount());
        PlayTime = GameManager.Instance.PlayTime;
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        GaugePlayTime.fillAmount = PlayTime / GameManager.Instance.PlayTime;
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
                GameManager.Instance.IsGamePlayOn = true;
                if (count_Co == null)
                    count_Co = StartCoroutine(PlayTimeCalculate());
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
                if (GameManager.Instance.IsGameEnd)
                {
                    yield return null;
                }
                yield return new WaitForSeconds(3f);

                PacketGameEnd packet = new PacketGameEnd();
                packet.redTeamScores[0] = points[0];
                packet.redTeamScores[1] = points[2];
                packet.blueTeamScores[0] = points[1];
                packet.blueTeamScores[1] = points[3];

                if (points[0] + points[2] > points[1] + points[3]) // red > blue
                    packet.winTeam = ETeam.Red;
                else if (points[0] + points[2] < points[1] + points[3]) // red < blue
                    packet.winTeam = ETeam.Blue;
                else 
                    packet.winTeam = ETeam.None;

                GameManager.Instance.Client.Send(packet);

            }
            PlayTime -= Time.deltaTime;
            yield return null;
        }
    }

    public void SetIDUI(int uid)
    {
        Player player = GameManager.Instance.GetPlayer(uid);
        switch (uid + 1)
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

    public void SetPointUI(int uid, int point = 0)
    {
        switch (uid + 1)
        {
            case 1:
                {
                    PlayerRed1Point.text = point + " Point";
                }
                break;
            case 3:
                {
                    PlayerRed2Point.text = point + " Point";
                }
                break;
            case 2:
                {
                    PlayerBlue1Point.text = point + " Point";
                }
                break;
            case 4:
                {
                    PlayerBlue2Point.text = point + " Point";
                }
                break;
            default:
                break;
        }
        points[uid] = point;

    }
}
