using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Player캐릭터 정의의 모든 것
public enum EState // 캐릭터의 상태
{
    Idle,
    Run,
    Dash,
    Fire,
}

[Serializable]
public class PlayerInfo
{
    public int UID;
    public string ID;
    public ETeam TEAM;
    public ERole ROLE;
    public bool _localPlayer;
}

[Serializable]
public class PlayerComponents
{
    [Header("Player")]
    public Animator animator;
    public Rigidbody rigidbody;
    public CapsuleCollider capsuleCollider;
    public Camera cameraObj;        //카메라.
}

[Serializable]
public class PlayerInput
{
    public float verticalMovement;   //상하
    public float horizontalMovement; //좌우
    public float mouseY;             //마우스 상하
    public float mouseX;             //마우스 좌우
}

[Serializable]
public class CheckOption
{
    [Tooltip("지면으로 체크할 레이어 설정")]
    public LayerMask groundLayerMask = -1;

    [Range(1f, 20f), Tooltip("회전속도")]
    public float rotSpeed = 20f;

    [Range(1f, 30f), Tooltip("달리는 속도")]
    public float runningSpeed = 15f;

    [Range(1f, 30f), Tooltip("대쉬 속도")]
    public float dashingSpeed = 25f;
}

[Serializable]
public class CurrentState
{
    public EState _curState;

    [Header("Player Moving")]
    public bool isNotMoving;
    public bool isRunning;  //뛰기
    public bool isDashing;  //대쉬
    public bool isDodgeing;  //닷지
    public bool doNotRotate;

    [Header("Timing Check")]
    public bool isStop; //대화창 활성화될때 움직임 비활성화여부
    [Space]
    public bool previousDodgeKeyPress;   //이전 프레임에서 대시 키 여부
    public bool currentDodgeKeyPress;    //현재 프레임에서 대시 키 여부
}

[Serializable]
public class CurrentValue
{
    public int point = 0;
    public float moveAmount;        // 움직임. (0 움직이지않음, 1 움직임)
    public Vector3 moveDirection;   //이동 방향
    public Vector3 playerVelocity;  //이동을 위한 플레이어 속도
    public float finalSpeed;
}