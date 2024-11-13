using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerReadyUIGroup
    {
        public Image ProgressImage;
        public TMP_Text StateText;
    }

    public PlayerReadyUIGroup[] ReadyGroups;
    public Transform LobbySpawnPoints;

    private List<bool> readyPlayers = new List<bool>();
    private Host host;

    [SerializeField]
    private Client client;

    void Start()
    {
        client = GameManager.Instance.client;
        host = FindObjectOfType<Host>();
        readyPlayers.Clear();
    }

    public void SetReadyState(int uid, bool isReady)
    {
        //Debug.Log($"SetReadyUI({uid})");
        Player player = GameManager.Instance.GetPlayer(uid);

        PlayerReadyUIGroup uiGroup = ReadyGroups[uid];
        uiGroup.StateText.text = "<" + player.ID.ToString() + "> ";
        if (isReady)
        {
            uiGroup.StateText.text += "준비 완료";
        }
        else
        {
            uiGroup.StateText.text += "준비중..";
        }

        if (isReady)
        {
            readyPlayers.Add(true);
            uiGroup.ProgressImage.fillAmount = 1;
        }
        else
        {
            if (readyPlayers.Count > 0)
                readyPlayers.RemoveAt(readyPlayers.Count - 1);
            uiGroup.ProgressImage.fillAmount = 0;
        }
    }

    public void SetReadyProgress(int uid, float progress)
    {
        ReadyGroups[uid].ProgressImage.fillAmount = Mathf.Clamp01(progress);
    }

    public Vector3 GetSpawnPoint(int uid)
    {
        return LobbySpawnPoints.GetChild(uid).position;
    }
}