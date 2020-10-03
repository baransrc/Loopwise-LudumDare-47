using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerAnimationState
{
    Idle,
    Walking,
    Dead,
}

[RequireComponent(typeof(Animator))]
public class PlayerAnimationManager : MonoBehaviour
{
    private Animator _animator;
    private static readonly int IsIdle = Animator.StringToHash("IsIdle");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetState(PlayerAnimationState state)
    {
        switch (state)
        {
            case PlayerAnimationState.Idle:
                _animator.SetBool(IsWalking, false);
                _animator.SetBool(IsIdle, true);
                break;
            
            case PlayerAnimationState.Walking:
                _animator.SetBool(IsIdle, false);
                _animator.SetBool(IsWalking, true);
                break;
            
            case PlayerAnimationState.Dead:
                _animator.SetBool(IsIdle, false);
                _animator.SetBool(IsWalking, false);
                _animator.SetBool(IsDead, true);
                break;
        }
    }
}
