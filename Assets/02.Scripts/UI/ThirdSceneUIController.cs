using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdSceneUIController : MonoBehaviour
{
    private ETeam winTeam;
    void Start()
    {
        winTeam = GameManager.Instance.WinTeam;
    }

    void Update()
    {
        
    }
}
