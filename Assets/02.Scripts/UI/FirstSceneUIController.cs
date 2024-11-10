using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FirstSceneUIController : MonoBehaviour
{
    public Image PlayerRed1;
    public TMP_Text PlayerRed1Ready;

    public Image PlayerRed2;
    public TMP_Text PlayerRed2Ready;

    public Image PlayerBlue1;
    public TMP_Text PlayerBlue1Ready;
    
    public Image PlayerBlue2;
    public TMP_Text PlayerBlue2Ready;

    private List<bool> readyPlayers;

    [SerializeField]
    private Client client;

    void Start()
    {
        client = GameManager.Instance.client;
        readyPlayers.Clear();
    }

    void Update()
    {
        
    }

    void FixedUpdate() 
    {
    }

    public void StartOnClick()
    {
        //todo: 모두 준비완료일 때 씬 전환(GamePlay)
        if (!GameManager.Instance.IsHost) return;
        if (readyPlayers.Count >= 4)    //! 4 = userList
        {
            GameManager.Instance.GameSceneNext();
        }
    }

    public void SetReadyUI(int uid, bool isReady)
    {
        //Debug.Log($"SetReadyUI({uid})");
        Player player = GameManager.Instance.GetPlayer(uid);
        switch (uid+1)
        {
            case 1:
                {
                    if (isReady) PlayerRed1Ready.text = "준비 완료";
                    else PlayerRed1Ready.text = "준비중..";
                    PlayerRed1Ready.text = "<" + player.ID.ToString() + "> " + PlayerRed1Ready.text;
                    //Debug.Log("SetReadyUI()11111111111");
                }
                break;
            case 3:
                {
                    if (isReady) PlayerRed2Ready.text = "준비 완료";
                    else PlayerRed2Ready.text = "준비중..";
                    PlayerRed2Ready.text = "<" + player.ID.ToString() + "> " + PlayerRed2Ready.text;
                    //Debug.Log("SetReadyUI()2222222222222");
                }
                break;
            case 2:
                {
                    if (isReady) PlayerBlue1Ready.text = "준비 완료";
                    else PlayerBlue1Ready.text = "준비중..";
                    PlayerBlue1Ready.text = "<" + player.ID.ToString() + "> " + PlayerBlue1Ready.text;
                    //Debug.Log("SetReadyUI()33333333333333");
                }
                break;
            case 4:
                {
                    if (isReady) PlayerBlue2Ready.text = "준비 완료";
                    else PlayerBlue2Ready.text = "준비중..";
                    PlayerBlue2Ready.text = "<" + player.ID.ToString() + "> " + PlayerBlue2Ready.text;
                    //Debug.Log("SetReadyUI()444444444444444");
                }
                break;
            default:
                break;
        }
        if (isReady)
        {
            readyPlayers.Add(true);
        }
        else
        {
            if (readyPlayers.Count > 0)
                readyPlayers.RemoveAt(readyPlayers.Count-1);
        }
    }
}
