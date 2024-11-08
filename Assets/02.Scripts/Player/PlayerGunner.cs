using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGunner : Player
{
    // 오른쪽 앞 대각선과 왼쪽 앞 대각선 벡터
    private Vector3 rightDiagonal;
    private Vector3 leftDiagonal;

    [SerializeField]
    private bool LeftSite;
    [SerializeField]
    private bool RightSite;

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
            //transform.position = new Vector3(-70, 3, 0);
            transform.Rotate(P_Com.cameraObj.transform.right);

            rightDiagonal = new Vector3(1, 0, -1).normalized;
            leftDiagonal = new Vector3(1, 0, 1).normalized;
        }
        else if (P_Info.TEAM == ETeam.Blue){
            //transform.position = new Vector3(70, 3, 0);
            transform.Rotate(P_Com.cameraObj.transform.right * -1);

            rightDiagonal = new Vector3(-1, 0, 1).normalized;
            leftDiagonal = new Vector3(-1, 0, -1).normalized;
        }
    }
    void Update()
    {   
        Move();
        if (this.transform.position.x < -70 && (RightSite || LeftSite))
        {
            transform.position = new Vector3(-70, 3, 0);
            RightSite = false;
            LeftSite = false;
        }
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
        if (Input.GetKey(KeyCode.A))// && !RightSite)
        {
            if (!RightSite)
                LeftSite = true;
            if (LeftSite)
                P_Input.verticalMovement = 1;
            if (RightSite)
                P_Input.verticalMovement = -1;
            P_States.isRunning = true;
        }
        else if (Input.GetKey(KeyCode.D))// && !LeftSite)
        {
            if (!LeftSite)
                RightSite = true;
            if (LeftSite)
                P_Input.verticalMovement = -1;
            if (RightSite)
                P_Input.verticalMovement = 1;
            P_States.isRunning = true;
        }
        else
        {
            P_Input.verticalMovement = 0;
            P_States.isRunning = false;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            P_States.isDashing = true;
        }

        P_Value.moveAmount = Mathf.Clamp01(Mathf.Abs(P_Input.verticalMovement) + Mathf.Abs(P_Input.horizontalMovement));
    }
    public override void Move()
    {
        MoveInput();
        
        if (P_States.isStop)
        {
            P_Com.rigidbody.velocity = Vector3.zero;
            transform.position = new Vector3(transform.position.x, 3, transform.position.z);
            return;
        }

        // leftDiagonal = new Vector3(1, 0, 1)
        // rightDiagonal = new Vector3(1, 0, -1)
        if (LeftSite)
        {
            if (P_Input.verticalMovement != 0)
            {
                // 오른쪽 대각선으로 이동
                P_Value.moveDirection = leftDiagonal * P_Input.verticalMovement;
            }
            else
            {
                // 이동하지 않음
                P_Value.moveDirection = Vector3.zero;
            }
        }
        else if (RightSite)
        {
            if (P_Input.verticalMovement != 0)
            {
                // 오른쪽 대각선으로 이동
                P_Value.moveDirection = rightDiagonal * P_Input.verticalMovement;
            }
            else
            {
                // 이동하지 않음
                P_Value.moveDirection = Vector3.zero;
            }
        }
        if (P_Value.moveDirection != Vector3.zero)
        {
            if (P_States.isDashing)
            {
                P_Value.finalSpeed = P_COption.dashingSpeed;
                P_States._curState = EState.Dash;
            }
            else if (P_States.isRunning)
            {
                P_Value.finalSpeed = P_COption.runningSpeed;
                P_States._curState = EState.Run;
            }
            P_Value.moveDirection = P_Value.moveDirection * P_Value.finalSpeed;

            P_Com.rigidbody.velocity = P_Value.moveDirection;
        }
        else
        {
            P_States._curState = EState.Idle;
            P_States.isRunning = false;
            P_States.isDashing = false;
            P_States.isDodgeing = false;
        }
    }
}
