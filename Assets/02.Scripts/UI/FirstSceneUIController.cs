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

    private float readyTime;
    [SerializeField]
    private Client client;

    void Start()
    {
        client = GameManager.Instance.client;
    }

    void Update()
    {
        
    }

    void FixedUpdate() 
    {
        // if (Input.GetKey(KeyCode.Space))
        // {
        //     if (readyTime >= 1.5f){
        //         // 준비 완료
        //         if (client == null) client = GameManager.Instance.client;
        //         client.Send(new PacketGameReady{
        //             uid = GameManager.Instance.UserUID,
        //             IsReady = true
        //         });
        //         readyTime = 0;
        //     }
        //     readyTime += Time.deltaTime;
        // }
        // if (Input.GetKey(KeyCode.LeftShift))
        // {
        //     if (readyTime >= 1.5f){
        //         // 준비 완료
        //         if (client == null) client = GameManager.Instance.client;
        //         client.Send(new PacketGameReady{
        //             uid = GameManager.Instance.UserUID,
        //             IsReady = false
        //         });
        //         readyTime = 0;
        //     }
        //     readyTime += Time.deltaTime;
        // }
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
                    if (player == null) player = GameManager.Instance.GetPlayer(uid);
                    PlayerRed1Ready.text = "<" + player.ID.ToString() + "> " + PlayerRed1Ready.text;
                    //Debug.Log("SetReadyUI()11111111111");
                }
                break;
            case 3:
                {
                    if (isReady) PlayerRed2Ready.text = "준비 완료";
                    else PlayerRed2Ready.text = "준비중..";
                    if (player == null) player = GameManager.Instance.GetPlayer(uid);
                    PlayerRed2Ready.text = "<" + player.ID.ToString() + "> " + PlayerRed2Ready.text;
                    //Debug.Log("SetReadyUI()2222222222222");
                }
                break;
            case 2:
                {
                    if (isReady) PlayerBlue1Ready.text = "준비 완료";
                    else PlayerBlue1Ready.text = "준비중..";
                    if (player == null) player = GameManager.Instance.GetPlayer(uid);
                    PlayerBlue1Ready.text = "<" + player.ID.ToString() + "> " + PlayerBlue1Ready.text;
                    //Debug.Log("SetReadyUI()33333333333333");
                }
                break;
            case 4:
                {
                    if (isReady) PlayerBlue2Ready.text = "준비 완료";
                    else PlayerBlue2Ready.text = "준비중..";
                    if (player == null) player = GameManager.Instance.GetPlayer(uid);
                    PlayerBlue2Ready.text = "<" + player.ID.ToString() + "> " + PlayerBlue2Ready.text;
                    //Debug.Log("SetReadyUI()444444444444444");
                }
                break;
            default:
                break;
        }

    }
}
