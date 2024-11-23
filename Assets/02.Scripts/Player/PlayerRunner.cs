using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunner : MonoBehaviour
{    
    private Player _player;
    float curVertVelocity;
    private float _dodgeCool = 0;

    void Awake() 
    {
        _player = this.GetComponent<Player>();
    }
    void Start()
    {
        _dodgeCool = 3;
    }
    void Update()
    {   
        if (!_player.IsLocalPlayer) return;
        //Move();
        Rotate();
        Dodge();
    }

    public void MoveInput(KeyCode keyCode)
    {
        if (keyCode == KeyCode.W)//Input.GetKey(KeyCode.W))
        {
            _player._input.verticalMovement = 1;
        }
        else if (keyCode == KeyCode.S)//Input.GetKey(KeyCode.S))
        {
            _player._input.verticalMovement = -1;
        }
        else
        {
            _player._input.verticalMovement = 0;
        }
        if (keyCode == KeyCode.A)//Input.GetKey(KeyCode.A))
        {
            _player._input.horizontalMovement = -1;
        }
        else if (keyCode == KeyCode.D)//Input.GetKey(KeyCode.D))
        {
            _player._input.horizontalMovement = 1;
        }
        else
        {
            _player._input.horizontalMovement = 0;
        }
        _player._currentValue.moveAmount = Mathf.Clamp01(Mathf.Abs(_player._input.verticalMovement) + Mathf.Abs(_player._input.horizontalMovement));
        if (_player._currentValue.moveAmount != 0) _player._currentState.isRunning = true;
    }
    
    public void Move(KeyCode keyCode)
    {
        MoveInput(keyCode);
        
        if (_player._currentState.isStop)
        {
            _player._playerComponents.rigidbody.velocity = Vector3.zero;
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            return;
        }
        Vector3 p_velocity;
        // 월드 좌표 기준으로 이동 방향 설정
        _player._currentValue.moveDirection = Vector3.forward * _player._input.verticalMovement;
        _player._currentValue.moveDirection += Vector3.right * _player._input.horizontalMovement;
        _player._currentValue.moveDirection.Normalize();

        if (_player._currentState.isDodgeing)
        {
            //_player._playerComponents.animator.Play("Dodge");

            _player._currentValue.moveDirection.y = 0f;

            _player._playerComponents.rigidbody.AddForce(_player._currentValue.moveDirection * _player._checkOption.dodgingForce, ForceMode.VelocityChange);

            Invoke("dodgeOut", 0.14f); 
        }
        else if (_player._currentState.isRunning)
        {
            _player._currentValue.finalSpeed = _player._checkOption.runningSpeed;
            _player._currentValue.moveDirection = _player._currentValue.moveDirection * _player._currentValue.finalSpeed;

            p_velocity = _player._currentValue.moveDirection;
            p_velocity = p_velocity + Vector3.up;
            _player._playerComponents.rigidbody.velocity = p_velocity;

            _player._currentState._curState = EState.Run;
        }
        else
        {
            _player._currentState._curState = EState.Idle;
            _player._currentState.isRunning = false;
            _player._currentState.isDashing = false;
            _player._currentState.isDodgeing = false;
        }
    }
    private void dodgeOut()
    {
        _player._currentState._curState = EState.Idle;
        _player._currentState.isDodgeing = false;
    }

    public void Rotate()
    {
        Vector3 targetDirect = Vector3.zero;

        targetDirect = Vector3.forward * _player._input.verticalMovement;
        targetDirect += Vector3.right * _player._input.horizontalMovement;
        targetDirect.Normalize(); //대각선 이동이 더 빨라지는 것을 방지하기 위해서
        targetDirect.y = 0;
        if (targetDirect == Vector3.zero)
        {
            //방향 전환이 없기에 캐릭터의 방향은 원래 방향.
            targetDirect = transform.forward;
        }
        Quaternion turnRot = Quaternion.LookRotation(targetDirect);
        Quaternion targetRot = Quaternion.Slerp(transform.rotation, turnRot, _player._checkOption.rotSpeed * Time.deltaTime);
        transform.rotation = targetRot;
    }

    private void Dodge()
    {
        _player._currentState.currentDodgeKeyPress = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        if (//_dodgeCool < 3f &&
             _player._currentState.previousDodgeKeyPress && _player._currentState.currentDodgeKeyPress)
        {
            return;
        }
        else if (//_dodgeCool >= 3f &&
            !_player._currentState.previousDodgeKeyPress && _player._currentState.currentDodgeKeyPress
            && _player._currentState._curState != EState.Dash)
        {
            //Debug.Log("Dodge");
            _player._currentState.isDodgeing = true;
            _dodgeCool = 0;
            _player._currentState._curState = EState.Dash;
            _player._playerComponents.animator.SetTrigger("isDodge");

            //! packet 보내기
            PacketDashStart packet = new PacketDashStart();
            packet.uid = _player.UID;
            GameManager.Instance.Client.Send(packet);

            curVertVelocity = _player._playerComponents.rigidbody.velocity.y;
        }

        _dodgeCool += Time.deltaTime;
        // 프레임마다 키 입력 저장
        _player._currentState.previousDodgeKeyPress = _player._currentState.currentDodgeKeyPress;
    }    
}
