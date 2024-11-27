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

    private ETeam winTeam;
    void Start()
    {
        winTeam = GameManager.Instance.WinTeam;
        SetWinTeamUI();
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
}
