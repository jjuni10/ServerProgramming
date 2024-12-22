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
        public TMP_Text RoleText;
        public TMP_Text StateText;
    }

    [Header("Ready")]
    public PlayerReadyUIGroup[] ReadyGroups;

    [Header("Start")]
    public Button StartButton;

    [Header("Spawn Points")]
    public Transform LobbySpawnPoints;

    [Header("Role Button Highlight")]
    public Color RoleButtonHighlightColor;
    public Image GunnerButtonImage;
    public Image RunnerButtonImage;

    void Start()
    {
        StartButton.onClick.AddListener(() => GameManager.Instance.Host.ReadyCheckGameStart());
    }

    public void OnLocalPlayerJoin()
    {
        if (GameManager.Instance.IsHost)
        {
            StartButton.gameObject.SetActive(true);
            StartButton.interactable = false;
        }
        else
        {
            StartButton.gameObject.SetActive(false);
        }
    }

    public void SetRole(int uid, ERole role)
    {
        PlayerReadyUIGroup uiGroup = ReadyGroups[uid];
        Image roleButtonImage = null;

        if (role == ERole.Runner)
        {
            uiGroup.RoleText.text = "R";
        }
        else
        {
            uiGroup.RoleText.text = "G";
        }

        if (uid == GameManager.Instance.UserUID)
        {
            if (role == ERole.Runner)
            {
                roleButtonImage = RunnerButtonImage;
            }
            else
            {
                roleButtonImage = GunnerButtonImage;
            }
        }

        if (roleButtonImage != null)
        {
            GunnerButtonImage.color = RunnerButtonImage.color = Color.white;
            roleButtonImage.color = RoleButtonHighlightColor;
        }
    }

    public void SetReadyState(int uid, bool isReady)
    {
        Player player = GameManager.Instance.GetPlayer(uid);

        PlayerReadyUIGroup uiGroup = ReadyGroups[uid];
        uiGroup.StateText.text = "<" + player.ID.ToString() + "> ";
        if (isReady)
        {
            uiGroup.StateText.text += "준비 완료";
            uiGroup.ProgressImage.fillAmount = 1;
        }
        else
        {
            uiGroup.StateText.text += "준비중..";
            uiGroup.ProgressImage.fillAmount = 0;
        }

        bool canStart = GameManager.Instance.PlayerCount >= 2 
            && GameManager.Instance.IsAllPlayerReady();

        StartButton.interactable = canStart;
    }

    public void SetReadyProgress(int uid, float progress)
    {
        ReadyGroups[uid].ProgressImage.fillAmount = Mathf.Clamp01(progress);
    }

    public Vector3 GetSpawnPoint(int uid)
    {
        if (GameManager.Instance.sheetData == null) GameManager.Instance.sheetData.Init();
        switch (uid + 1)
        {
            case 1:
            {
                return GameManager.Instance.sheetData.Red1BasicStartPos;
            }
            case 3:
            {
                return GameManager.Instance.sheetData.Red2BasicStartPos;
            }
            case 2:
            {
                return GameManager.Instance.sheetData.Blue1BasicStartPos;
            }
            case 4:
            {
                return GameManager.Instance.sheetData.Blue2BasicStartPos;
            }
            default: break;
        }
        return LobbySpawnPoints.GetChild(uid).position;
    }
}
