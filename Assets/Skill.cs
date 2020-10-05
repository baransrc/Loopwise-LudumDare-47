using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using DefaultNamespace;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Skill : MonoBehaviour
{
    
    [SerializeField] private Vector3 offsetVector;
    [SerializeField] private float shotDuration;
    private EnemyController _enemyController;
    private AttackType _type;
    private SpriteRenderer _spriteRenderer;
    private PlayerController _player;
    private Animator _animator;
    
    public void Initialize(PlayerController player)
    {
        _player = player;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
    }

    private IEnumerator StartCastingAnim(AttackType attackType)
    {
        var animName = "Magic - Multiplication";
        
        switch (attackType)
        {
            case AttackType.Multiply:
                animName = "Magic - Multiplication";
                break;
            case AttackType.Divide:
                animName = "Magic - Division";
                break;
            case AttackType.Substract:
                animName = "Magic - Substraction";
                break;
            case AttackType.Sum:
                animName = "Magic - Sum";
                break;
            case  AttackType.CommentOut:
                animName = "Magic - Comment";
                break;   
        }
        
        _animator.Play(animName);    
            
        yield return new WaitForSeconds(0.95f);
    }

    public void ShootTo(EnemyController enemyController, AttackType type)
    {
        var position = _player.transform.position;    
        
        if (!_player.FacingRight())
        {    
            offsetVector.x *= -1;
        }

        transform.position = position + offsetVector;
                
        _spriteRenderer.enabled = true;
        _enemyController = enemyController;
        _enemyController.willBeShotBy = type;
        StartCoroutine(GoTowardsEnemy(type));
    }

    private IEnumerator GoTowardsEnemy(AttackType type)
    {
        yield return StartCastingAnim(type);
        
        var step = 0f;
        var initialPosition = transform.position;

        while (step < 1f)
        {
            step += Time.deltaTime / shotDuration;
            step = (step > 1f) ? 1f : step;

            if (_enemyController == null)
            {
                Destroy(gameObject);
                yield break;
            }

            transform.position = Vector3.Lerp(initialPosition, _enemyController.transform.position, step);

            yield return null;
        }

        //_spriteRenderer.enabled = false;
    }

}
