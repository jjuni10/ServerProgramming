using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThirdSceneUIController : MonoBehaviour
{
    public GameObject RedWinText;
    public GameObject BlueWinText;
    public GameObject NoneWinText;

    private Player[] player = new Player[4];
    public GameObject[] platform = new GameObject[4];

    private ETeam winTeam;

    public float HeightOffset = 20;
    private float rateValue;
    private float pointHeight;

    void Start()
    {
        winTeam = GameManager.Instance.WinTeam;
        SetPointHeight();
        SetWinTeamUI();
    }

    public void SetPointHeight()
    {
        rateValue = (GetPoint(true) - GetPoint(false)) / HeightOffset;

        for (int i = 0; i < 4; i++)
        {
            player[i] = GameManager.Instance.GetPlayer(i);

            pointHeight = player[i]._currentValue.point * rateValue;

            platform[i].transform.localScale += new Vector3(0, pointHeight, 0);
            platform[i].transform.position += new Vector3(0, pointHeight/2, 0);
            player[i].transform.position += new Vector3(0, pointHeight, 0);
        }
    }

    public void SetWinTeamUI()
    {
        if (winTeam == ETeam.Red)
        {
            RedWinText.SetActive(true);
        }
        else if (winTeam == ETeam.Blue)
        {
            BlueWinText.SetActive(true);
        }
        else
        {
            NoneWinText.SetActive(true);
        }
    }

    public float GetPoint(bool isBig)
    {
        int result;
        if (isBig)
        {
            result = 0;
            foreach (var item in GameManager.Instance.points)
            {
                if (result > item) result = item;
            }
        }
        else
        {
            result = 100;
            foreach (var item in GameManager.Instance.points)
            {
                if (result < item) result = item;
            }
        }
        return result;
    }
}
