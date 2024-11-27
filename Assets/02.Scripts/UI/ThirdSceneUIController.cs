using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThirdSceneUIController : MonoBehaviour
{
    public GameObject RedWinText;
    public GameObject BlueWinText;

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
        else
        {
            BlueWinText.SetActive(true);
        }
    }
}
