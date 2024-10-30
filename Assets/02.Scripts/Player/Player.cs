using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerComponents _playerComponents = new PlayerComponents();
    public PlayerInput _input = new PlayerInput();
    public CheckOption _checkOption = new CheckOption();
    public CurrentState _currentState = new CurrentState();
    public CurrentValue _currentValue = new CurrentValue();
    
    protected PlayerComponents P_Com => _playerComponents;
    protected PlayerInput P_Input => _input;
    protected CheckOption P_COption => _checkOption;
    protected CurrentState P_States => _currentState;
    protected CurrentValue P_Value => _currentValue;

    protected void Init(Animator animator, Rigidbody rigidbody, CapsuleCollider collider)
    {
       P_Com.animator = animator;
       P_Com.rigidbody = rigidbody;
       P_Com.capsuleCollider = collider;
       P_Com.cameraObj = Camera.main; 

       P_Value.point = 0;
    }

    public virtual void Move()
    {

    }

    public virtual void Rotate()
    {
        
    }
}
