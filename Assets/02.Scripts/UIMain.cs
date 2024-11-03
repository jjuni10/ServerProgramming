using UnityEngine;
using UnityEngine.UI;

public class UIMain : MonoBehaviour
{
    // UI 관련 변수들
    public GameObject startUI;
    public GameObject lobbyUI;

    public InputField inputID;
    public InputField inputIP;
    public Button buttonHost;
    public Button buttonClient;

    public Text textRed;
    public Text textBlue;
    public Button buttonRed;
    public Button buttonBlue;
    public Button buttonGunner;
    public Button buttonRunner;
    public Button buttonReady;
    public Button buttonStart;
    //

    private Client _client;

    public enum EUIState
    {
        Start,
        Lobby,
        Game
    }

    // 게임 시작 시 UI 요소를 초기화
    private void Awake()
    {
        _client = FindObjectOfType<Client>();

        buttonHost.onClick.AddListener(() =>
        {
            string id = inputID.text;
            if (string.IsNullOrEmpty(id))
                return;

            GameManager.Instance.UserID = inputID.text;
            // Host 객체를 찾아서 시작한다.
            FindObjectOfType<Host>().StartHost();
        });

        buttonClient.onClick.AddListener(() =>
        {
            string id = inputID.text;
            if (string.IsNullOrEmpty(id))
                return;

            string ip = inputIP.text;
            if (string.IsNullOrEmpty(ip))
                return;

            GameManager.Instance.UserID = inputID.text;
            // Client 객체를 찾아서 시작한다.
            FindObjectOfType<Client>().StartClient(inputIP.text);
        });

        buttonRed.onClick.AddListener(() =>
        {
            SendTeam(ETeam.Red);
        });

        buttonBlue.onClick.AddListener(() =>
        {
            SendTeam(ETeam.Blue);
        });

        buttonGunner.onClick.AddListener(() =>
        {
            SendRole(ERole.Gunner);
        });

        buttonRunner.onClick.AddListener(() =>
        {
            SendRole(ERole.Runner);
        });

        buttonReady.onClick.AddListener(() =>
        {
            PacketGameReady packet = new PacketGameReady();
            _client.Send(packet);
        });

        buttonStart.onClick.AddListener(() =>
        {
            if (!GameManager.Instance.IsHost)
                return;

            if(GameManager.Instance.IsHost)
            {
                PacketGameReady hostredypacket = new PacketGameReady();
                _client.Send(hostredypacket);
            }
            PacketGameReadyOk packet = new PacketGameReadyOk();
            _client.Send(packet);
        });

        SetUIState(EUIState.Start);
    }

    // UI 상태를 변경한다 (시작 화면, 로비, 게임)
    public void SetUIState(EUIState state)
    {
        switch (state)
        {
            case EUIState.Start:
                startUI.SetActive(true);
                lobbyUI.SetActive(false);
                break;
            case EUIState.Lobby:
                startUI.SetActive(false);
                lobbyUI.SetActive(true);
                break;
            case EUIState.Game:
                startUI.SetActive(false);
                lobbyUI.SetActive(false);
                break;
        }
    }

    // 로비 화면에 팀별 유저 리스트를 표시한다
    public void SetLobbyText(string red, string blue)
    {
        textRed.text = red;
        textBlue.text = blue;
    }

    // 선택한 팀을 서버에 전송한다
    public void SendTeam(ETeam team)
    {
        PacketReqChangeTeam packet = new PacketReqChangeTeam();
        packet.team = team;

        _client.Send(packet);
    }

    // 선택한 역할을 서버에 전송한다
    public void SendRole(ERole role)
    {
        PacketReqChangeRole packet = new PacketReqChangeRole();
        packet.role = role;

        _client.Send(packet);
    }
}