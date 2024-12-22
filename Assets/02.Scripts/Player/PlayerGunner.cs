using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGunner : MonoBehaviour
{
    private Player _player;
    private int originX;
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
        if (_player.Team == ETeam.Red)
        {
            leftDiagonal = new Vector3(1, 0, 1);
            rightDiagonal = new Vector3(1, 0, -1);
            originX = -70;
        }
        else if (_player.Team == ETeam.Blue)
        {
            leftDiagonal = new Vector3(-1, 0, -1);
            rightDiagonal = new Vector3(-1, 0, 1);
            originX = 70;
        }
    }
    void Update()
    {   
        //Move();
        
        if (!_player.IsLocalPlayer) return;
        if (Math.Abs(this.transform.position.x) > 70 && (RightSite || LeftSite))
        {
            transform.position = new Vector3(originX, transform.position.y, transform.position.z);
            RightSite = false;
            LeftSite = false;
        }
    }

    public void MoveInput(KeyCode keyCode)
    {
        if (keyCode == KeyCode.A)
        {
            if (!RightSite)
                LeftSite = true;
            if (LeftSite)
                _player._input.verticalMovement = 1;
            if (RightSite)
                _player._input.verticalMovement = -1;
            _player._currentState.isRunning = true;
        }
        else if (keyCode == KeyCode.D)
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
            _player._input.verticalMovement = 0;
            _player._currentState.isRunning = false;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _player._currentState.isDashing = true;
        }
        else
        {
            _player._currentState.isDashing = false;
        }

        _player._currentValue.moveAmount = Mathf.Clamp01(Mathf.Abs(_player._input.verticalMovement) + Mathf.Abs(_player._input.horizontalMovement));
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

        if (LeftSite)
        {
            if (_player._input.verticalMovement != 0)
            {
                // 오른쪽 대각선으로 이동
                _player._currentValue.moveDirection = leftDiagonal * _player._input.verticalMovement;
            }
            else
            {
                // 이동하지 않음
                _player._currentValue.moveDirection = Vector3.zero;
            }
        }
        else if (RightSite)
        {
            if (_player._input.verticalMovement != 0)
            {
                // 오른쪽 대각선으로 이동
                _player._currentValue.moveDirection = rightDiagonal * _player._input.verticalMovement;
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
                _player._currentValue.finalSpeed = GameManager.Instance.playerSheetData.GunnerDashSpeed;
                _player._currentState._curState = EState.Dash;
            }
            else if (_player._currentState.isRunning)
            {
                _player._currentValue.finalSpeed = GameManager.Instance.playerSheetData.GunnerMoveSpeed;
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
