using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class EnemyAnimationManager : MonoBehaviour
{
    private Animator _animator;
    private static readonly int IsIdle = Animator.StringToHash("IsIdle");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                _animator.SetBool(IsWalking, false);
                _animator.SetBool(IsIdle, true);
                break;
            
            case EnemyState.Walking:
                _animator.SetBool(IsIdle, false);
                _animator.SetBool(IsWalking, true);
                break;
            
            case EnemyState.Dead:
                _animator.SetBool(IsIdle, false);
                _animator.SetBool(IsWalking, false);
                _animator.SetBool(IsDead, true);
                break;
        }
    }

    public float GetAnimationCurrentAnimationDuration()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).length;
    }
}