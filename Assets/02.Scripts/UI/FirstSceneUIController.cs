using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FirstSceneUIController : MonoBehaviour
{
    public Image Player1;
    public TMP_Text Player1Ready;

    public Image Player2;
    public TMP_Text Player2Ready;

    public Image Player3;
    public TMP_Text Player3Ready;
    
    public Image Player4;
    public TMP_Text Player4Ready;

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

        if (Input.GetKey(KeyCode.Space))
        {
            if (readyTime >= 1.5f){
                // 준비 완료
                if (client == null) client = GameManager.Instance.client;
                client.Send(new PacketGameReady{
                    IsReady = true
                });
                readyTime = 0;
            }
            readyTime += Time.deltaTime;
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
                    if (isReady) Player1Ready.text = "준비 완료";
                    else Player1Ready.text = "준비중..";
                    if (player == null) player = GameManager.Instance.GetPlayer(uid);
                    Player1Ready.text = "<" + player.ID.ToString() + "> " + Player1Ready.text;
                    //Debug.Log("SetReadyUI()11111111111");
                }
                break;
            case 2:
                {
                    if (isReady) Player2Ready.text = "준비 완료";
                    else Player2Ready.text = "준비중..";
                    if (player == null) player = GameManager.Instance.GetPlayer(uid);
                    Player2Ready.text = "<" + player.ID.ToString() + "> " + Player2Ready.text;
                    //Debug.Log("SetReadyUI()2222222222222");
                }
                break;
            case 3:
                {
                    if (isReady) Player3Ready.text = "준비 완료";
                    else Player3Ready.text = "준비중..";
                    if (player == null) player = GameManager.Instance.GetPlayer(uid);
                    Player3Ready.text = "<" + player.ID.ToString() + "> " + Player3Ready.text;
                    //Debug.Log("SetReadyUI()33333333333333");
                }
                break;
            case 4:
                {
                    if (isReady) Player4Ready.text = "준비 완료";
                    else Player4Ready.text = "준비중..";
                    if (player == null) player = GameManager.Instance.GetPlayer(uid);
                    Player4Ready.text = "<" + player.ID.ToString() + "> " + Player4Ready.text;
                    //Debug.Log("SetReadyUI()444444444444444");
                }
                break;
            default:
                break;
        }

    }
}
