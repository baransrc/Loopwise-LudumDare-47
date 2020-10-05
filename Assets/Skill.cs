using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Skill : MonoBehaviour
{
    [SerializeField] private Vector3 offsetVector;
    [SerializeField] private float shotDuration;
    private EnemyController _enemyController;
    private AttackType _type;
    private SpriteRenderer _spriteRenderer;
    private PlayerController _player;
    
    public void Initialize(PlayerController player)
    {
        _player = player;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
    }

    public void ShootTo(EnemyController enemyController, AttackType type)
    {
        transform.position = _player.transform.position + offsetVector;
        _spriteRenderer.enabled = true;
        _enemyController = enemyController;
        _enemyController.willBeShotBy = type;
        StartCoroutine(GoTowardsEnemy());
    }

    private IEnumerator GoTowardsEnemy()
    {
        var step = 0f;
        var initialPosition = transform.position;

        while (step < 1f)
        {
            step += Time.deltaTime / shotDuration;
            step = (step > 1f) ? 1f : step;

            transform.position = Vector3.Lerp(initialPosition, _enemyController.transform.position, step);

            yield return null;
        }

        //_spriteRenderer.enabled = false;
    }

}
