using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

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
    [FormerlySerializedAs("textMesh")] [SerializeField] private TextMeshPro scoreText;

    private int _id;
    private static int _currentId = 0;
    private EnemyAnimationManager _enemyAnimationManager;
    private EnemyState _enemyState;
    private int _score;
    private float _initialY;
    private float _timer;
    private Coroutine _dyingCoroutine;
    private GameController _gameController;
    private PlayerController _player;

    public AttackType willBeShotBy;
    
    public bool IsRight { get; set; }

    public int Id { get {return _id;} }

    public void Initialize(GameController gameController, PlayerController player, int score = 1)
    {
        _id = _currentId;
        _currentId++;
        
        _gameController = gameController;
        _player = player;

        _score = score;
        scoreText.text = score.ToString();
        
        _initialY = transform.position.y; // TODO: Do this better.
        
        _enemyAnimationManager = GetComponent<EnemyAnimationManager>();
        
        _dyingCoroutine = null;
        
        SetState(EnemyState.Walking);
        
        _timer = 0f;
    }

    private IEnumerator Die()
    {
        _gameController.ChangeScore(willBeShotBy, _score);
        
        _gameController.RemoveEnemyFromList(this);
        
        SetState(EnemyState.Dead);
        
        yield return new WaitForSeconds(1);
        
        Destroy(gameObject);
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Skill"))
        {
            return;
        }
        
        Destroy(other.gameObject);

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

    public void GoTowards(Vector3 position)
    {
        if (_enemyState != EnemyState.Walking)
        {
            return;
        }

        var deltaX = (position.x - transform.position.x);

        if (Mathf.Abs(deltaX) <= 0.55)
        {
            _timer = 0;
            SetState(EnemyState.Idle);
            return;
        }

        var direction = deltaX > 0 ? Vector3.right : Vector3.left;
        position.y = _initialY;

        transform.position += direction * speed * Time.deltaTime;
        transform.position = Mathf.Abs(transform.position.x - position.x) <= 0.55f ? 
            position - direction * 0.55f : transform.position;

    }

    private void Update()
    {
        DetermineStateByTimer();
    }
}
