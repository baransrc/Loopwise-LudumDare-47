using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Walking,
    Dead
}

[RequireComponent(typeof(EnemyAnimationManager))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float walkingTime;
    [SerializeField] private float idleTime;

    private EnemyAnimationManager _enemyAnimationManager;
    private EnemyState _enemyState;
    private float _score;
    private float _timer;
    private Coroutine _dyingCoroutine;
    [SerializeField] private PlayerController _player;

    public void Initialize(PlayerController player)
    {
        _player = player;
        _enemyAnimationManager = GetComponent<EnemyAnimationManager>();
        _dyingCoroutine = null;
        SetState(EnemyState.Walking);
        _timer = 0f;
    }

    private IEnumerator Die()
    {
        SetState(EnemyState.Dead);
        
        yield return new WaitForSeconds(1);
        
        Destroy(gameObject);
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_dyingCoroutine != null)
        {
            return;
        }
        
        _dyingCoroutine = StartCoroutine(Die());
    }

    private float TakeDamage()
    {
        return _score;
    }

    private void DetermineStateByTimer()
    {
        if (_enemyState == EnemyState.Dead)
        {
            _timer = 0f;
            return;
        }

        if (_enemyState == EnemyState.Idle)
        {
            if (_timer >= idleTime)
            {
                _timer = 0f;
                SetState(EnemyState.Walking);
            }
        }
        
        else if (_enemyState == EnemyState.Walking)
        {
            if (_timer >= walkingTime)
            {
                _timer = 0f;
                SetState(EnemyState.Idle);
            }
        }
        
        _timer += Time.deltaTime;
    }

    private void SetState(EnemyState state)
    {
        _enemyState = state;
        _enemyAnimationManager.SetState(_enemyState);
    }

    private void GoTowardsPlayer()
    {
        if (_enemyState != EnemyState.Walking)
        {
            return;
        }

        var deltaX = (_player.transform.position.x - transform.position.x);

        if (Mathf.Abs(deltaX) <= 1)
        {
            _timer = 0;
            SetState(EnemyState.Idle);
            return;
        }

        var direction = deltaX > 0 ? Vector3.right : Vector3.left;

        transform.position += direction * speed * Time.deltaTime;
        transform.position = Mathf.Abs(transform.position.x - _player.transform.position.x) <= 1f ? 
            _player.transform.position - direction * 1f : transform.position;

    }

    private void Update()
    {
        DetermineStateByTimer();
        GoTowardsPlayer();
    }
}
