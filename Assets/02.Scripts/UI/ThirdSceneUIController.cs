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
        //SetPointHeight();
        SetWinTeamUI();
    }

    public void SetPointHeight()
    {
        float maxPoint = GetPoint(true);
        rateValue = 20 / maxPoint;

        for (int i = 0; i < GameManager.Instance.PlayerCount; i++)
        {
            player[i] = GameManager.Instance.GetPlayer(i);

            pointHeight = player[i]._currentValue.point * rateValue;
            //Debug.Log($"[PointTest] pointHeight: {pointHeight}");

            platform[i].transform.localScale += new Vector3(0, pointHeight, 0);
            platform[i].transform.position += new Vector3(0, pointHeight/2, 0);
            player[i].transform.position += new Vector3(0, pointHeight, 0);
            
            //Debug.Log($"[PointTest] GameManager.Instance.points[i]: {GameManager.Instance.points[i]}");
            player[i]._currentValue.point = GameManager.Instance.points[i];
            player[i].SetPlayerPoint();
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
        int result_Max = GameManager.Instance.points[0];
        int result_Min = GameManager.Instance.points[0];
        // foreach (var item in GameManager.Instance.points)
        // {
        //     if (result_Max < item) result_Max = item;
        //     else if (item < result_Min) result_Min = item;
        // }
        for (int i = 0; i < GameManager.Instance.PlayerCount; i++)
        {
            if (result_Max < GameManager.Instance.points[i]) result_Max = GameManager.Instance.points[i];
            else if (GameManager.Instance.points[i] < result_Min) result_Min = GameManager.Instance.points[i];
        }
        if (isBig)
        {
            return result_Max;
        }
        else
        {
            return result_Min;
        }
        
    }
    public float GetSumPoint()
    {
        float sum = 0;
        foreach (var item in GameManager.Instance.points)
        {
            sum += item;
        }
        return sum;
    }
}
