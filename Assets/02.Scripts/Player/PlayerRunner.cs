using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunner : Player
{    
    float curVertVelocity;
    private float _dodgeCool = 0;


    void Awake() 
    {        
        P_Com.rigidbody = GetComponent<Rigidbody>();
        P_Com.animator = GetComponent<Animator>();
        P_Com.capsuleCollider = GetComponent<CapsuleCollider>();
        P_Com.cameraObj = Camera.main;
    }
    void Start()
    {
        if (P_Info.TEAM == ETeam.Red){
            //transform.position = new Vector3(-10, 3, 0);
            transform.Rotate(P_Com.cameraObj.transform.right);
        }
        else if (P_Info.TEAM == ETeam.Blue){
            //transform.position = new Vector3(10, 3, 0);
            transform.Rotate(P_Com.cameraObj.transform.right * -1);
        }
        _dodgeCool = 0;
    }
    void Update()
    {   
        Move();
        Rotate();
    }
    void FixedUpdate() 
    {
        Dodge();
    }

    public void Init(int uid, string id, ETeam team, Vector3 position, ERole role)
    {
        P_Info.UID = uid; 
        P_Info.ID = id; 
        P_Info.TEAM = team;
        P_Info.ROLE = role;
        P_Com.cameraObj = Camera.main;

        base.Init(uid, id, team, position, role);
        
        _destPosition = position;
        transform.position = position;
    }

    public void MoveInput()
    {
        //move
        if (Input.GetKey(KeyCode.W))
        {
            P_Input.verticalMovement = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            P_Input.verticalMovement = -1;
        }
        else
        {
            P_Input.verticalMovement = 0;
        }
        if (Input.GetKey(KeyCode.A))
        {
            P_Input.horizontalMovement = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            P_Input.horizontalMovement = 1;
        }
        else
        {
            P_Input.horizontalMovement = 0;
        }
        P_Value.moveAmount = Mathf.Clamp01(Mathf.Abs(P_Input.verticalMovement) + Mathf.Abs(P_Input.horizontalMovement));
        if (P_Value.moveAmount != 0) P_States.isRunning = true;
    }
    
    public override void Move()
    {
        MoveInput();
        
        if (P_States.isStop)
        {
            P_Com.rigidbody.velocity = Vector3.zero;
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            return;
        }
        Vector3 p_velocity;
        // 월드 좌표 기준으로 이동 방향 설정
        P_Value.moveDirection = Vector3.forward * P_Input.verticalMovement;
        P_Value.moveDirection += Vector3.right * P_Input.horizontalMovement;
        P_Value.moveDirection.Normalize();

        if (P_States.isDodgeing)
        {
            P_Com.animator.Play("Dodge", 0);

            P_Value.moveDirection.y = 0f;

            P_Com.rigidbody.AddForce(P_Value.moveDirection * P_COption.dodgingForce, ForceMode.VelocityChange);

            // 기존 수직 속도를 유지하도록 수직 속도 다시 설정
            //P_Com.rigidbody.velocity = new Vector3(P_Com.rigidbody.velocity.x, curVertVelocity, P_Com.rigidbody.velocity.z);

            Invoke("dodgeOut", 0.14f);    //닷지 유지 시간 = 0.14초
        }
        else if (P_States.isRunning)
        {
            P_Value.finalSpeed = P_COption.runningSpeed;
            P_Value.moveDirection = P_Value.moveDirection * P_Value.finalSpeed;

            p_velocity = P_Value.moveDirection;
            p_velocity = p_velocity + Vector3.up;
            P_Com.rigidbody.velocity = p_velocity;

            P_States._curState = EState.Run;
        }
        else
        {
            P_States._curState = EState.Idle;
            P_States.isRunning = false;
            P_States.isDashing = false;
            P_States.isDodgeing = false;
        }
    }
    private void dodgeOut()
    {
        P_States._curState = EState.Idle;
        P_States.isDodgeing = false;
    }

    public override void Rotate()
    {
        Vector3 targetDirect = Vector3.zero;

        targetDirect = Vector3.forward * P_Input.verticalMovement;
        targetDirect += Vector3.right * P_Input.horizontalMovement;
        targetDirect.Normalize(); //대각선 이동이 더 빨라지는 것을 방지하기 위해서
        targetDirect.y = 0;
        if (targetDirect == Vector3.zero)
        {
            //vector3.zero는 0,0,0 이다.
            //방향 전환이 없기에 캐릭터의 방향은 고냥 원래 방향.
            targetDirect = transform.forward;
        }
        Quaternion turnRot = Quaternion.LookRotation(targetDirect);
        Quaternion targetRot = Quaternion.Slerp(transform.rotation, turnRot, P_COption.rotSpeed * Time.deltaTime);
        transform.rotation = targetRot;
    }

    private void Dodge()
    {
        P_States.currentDodgeKeyPress = Input.GetKey(KeyCode.LeftShift);

        if (_dodgeCool < 3f && ReturnDodgeAnim()
            && P_States.previousDodgeKeyPress && P_States.currentDodgeKeyPress)
        {
            return;
        }
        else if (_dodgeCool >= 3f && !ReturnDodgeAnim()
            &&!P_States.previousDodgeKeyPress && P_States.currentDodgeKeyPress
            && P_States._curState != EState.Dash)
        {
            Debug.Log("Dodge");
            P_States.isDodgeing = true;
            _dodgeCool = 0;
            P_States._curState = EState.Dash;
            curVertVelocity = P_Com.rigidbody.velocity.y;
        }

        _dodgeCool += Time.deltaTime;
        // 프레임마다 키 입력 저장
        P_States.previousDodgeKeyPress = P_States.currentDodgeKeyPress;
    }    
    private bool ReturnDodgeAnim()
    {
        if (P_Com.animator.GetCurrentAnimatorStateInfo(0).IsName("Dodge"))
        {
            return true;
        }
        else return false;
    }
}
