using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGunner : MonoBehaviour
{
    private Player _player;
    // 오른쪽 앞 대각선과 왼쪽 앞 대각선 벡터
    private Vector3 rightDiagonal;
    private Vector3 leftDiagonal;

    [SerializeField]
    private bool LeftSite;
    [SerializeField]
    private bool RightSite;

    void Awake() 
    {
        _player = this.GetComponent<Player>();
    }
    void Start()
    {
        // if (P_Info.TEAM == ETeam.Red){
        //     //transform.position = new Vector3(-70, 3, 0);
        //     transform.Rotate(P_Com.cameraObj.transform.right);

        //     rightDiagonal = new Vector3(1, 0, -1).normalized;
        //     leftDiagonal = new Vector3(1, 0, 1).normalized;
        // }
        // else if (P_Info.TEAM == ETeam.Blue){
        //     //transform.position = new Vector3(70, 3, 0);
        //     transform.Rotate(P_Com.cameraObj.transform.right * -1);

        //     rightDiagonal = new Vector3(-1, 0, 1).normalized;
        //     leftDiagonal = new Vector3(-1, 0, -1).normalized;
        // }
    }
    void Update()
    {   
        //Move();
        
        if (!_player.IsLocalPlayer) return;
        if (this.transform.position.x < -70 && (RightSite || LeftSite))
        {
            transform.position = new Vector3(-70, 3, 0);
            RightSite = false;
            LeftSite = false;
        }
    }

    public void Init(int uid, string id, ETeam team, Vector3 position, ERole role)
    {
        _player._playerInfos.UID = uid; 
        _player._playerInfos.ID = id; 
        _player._playerInfos.TEAM = team;
        _player._playerInfos.ROLE = role;
        _player._playerComponents.cameraObj = Camera.main;
        
        // base.Init(uid, id, team, position);
        
        // _destPosition = position;
        transform.position = position;
    }

    public void MoveInput(KeyCode keyCode)
    {
        if (keyCode == KeyCode.A)//Input.GetKey(KeyCode.A) && !RightSite)
        {
            if (!RightSite)
                LeftSite = true;
            if (LeftSite)
                _player._input.verticalMovement = 1;
            if (RightSite)
                _player._input.verticalMovement = -1;
            _player._currentState.isRunning = true;
        }
        else if (keyCode == KeyCode.D)//Input.GetKey(KeyCode.D) && !LeftSite)
        {
            if (!LeftSite)
                RightSite = true;
            if (LeftSite)
                _player._input.verticalMovement = -1;
            if (RightSite)
                _player._input.verticalMovement = 1;
            _player._currentState.isRunning = true;
        }
        else
        {
            _player._input
            .verticalMovement = 0;
            _player._currentState.isRunning = false;
        }

        if (keyCode == KeyCode.LeftShift)//Input.GetKey(KeyCode.LeftShift))
        {
            _player._currentState.isDashing = true;
        }

        _player._currentValue.moveAmount = Mathf.Clamp01(Mathf.Abs(_player._input
        .verticalMovement) + Mathf.Abs(_player._input
        .horizontalMovement));
    }
    public void Move(KeyCode keyCode)
    {
        MoveInput(keyCode);
        
        if (_player._currentState.isStop)
        {
            _player._playerComponents.rigidbody.velocity = Vector3.zero;
            transform.position = new Vector3(transform.position.x, 3, transform.position.z);
            return;
        }

        // leftDiagonal = new Vector3(1, 0, 1)
        // rightDiagonal = new Vector3(1, 0, -1)
        if (LeftSite)
        {
            if (_player._input
            .verticalMovement != 0)
            {
                // 오른쪽 대각선으로 이동
                _player._currentValue.moveDirection = leftDiagonal * _player._input
                .verticalMovement;
            }
            else
            {
                // 이동하지 않음
                _player._currentValue.moveDirection = Vector3.zero;
            }
        }
        else if (RightSite)
        {
            if (_player._input
            .verticalMovement != 0)
            {
                // 오른쪽 대각선으로 이동
                _player._currentValue.moveDirection = rightDiagonal * _player._input
                .verticalMovement;
            }
            else
            {
                // 이동하지 않음
                _player._currentValue.moveDirection = Vector3.zero;
            }
        }
        if (_player._currentValue.moveDirection != Vector3.zero)
        {
            if (_player._currentState.isDashing)
            {
                _player._currentValue.finalSpeed = _player._checkOption.dashingSpeed;
                _player._currentState._curState = EState.Dash;
            }
            else if (_player._currentState.isRunning)
            {
                _player._currentValue.finalSpeed = _player._checkOption.runningSpeed;
                _player._currentState._curState = EState.Run;
            }
            _player._currentValue.moveDirection = _player._currentValue.moveDirection * _player._currentValue.finalSpeed;

            _player._playerComponents.rigidbody.velocity = _player._currentValue.moveDirection;
        }
        else
        {
            _player._currentState._curState = EState.Idle;
            _player._currentState.isRunning = false;
            _player._currentState.isDashing = false;
            _player._currentState.isDodgeing = false;
        }
    }
}
