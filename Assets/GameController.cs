﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Vector3 enemySpawnDistance;
    [SerializeField] private float enemyYPosition;
    [SerializeField] private float enemySpawnDuration;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private int initialLeft;
    [SerializeField] private int initialRight;
    [SerializeField] private float floorY;
    [SerializeField] private float maxFloorDistance;
    
    private GameObject _leftmostFloor;
    private GameObject _rightmostFloor;
    private List<GameObject> _floors;
    private float _enemySpawnCounter;

    private void Awake()
    {
        _floors = new List<GameObject>();
        _enemySpawnCounter = enemySpawnDuration;
        GenerateFloors();
    }

    private void GenerateEnemy()
    {
        _enemySpawnCounter += Time.deltaTime;
        
        if (_enemySpawnCounter < enemySpawnDuration)
        {
            return;
        }

        _enemySpawnCounter = 0f;
        
        var enemy = Instantiate(enemyPrefab).GetComponent<EnemyController>();
        enemy.Initialize(player);

        var playerPosition = player.transform.position;
        playerPosition.y = enemyYPosition;
        
        enemy.transform.position = player.FacingRight()
            ? playerPosition + enemySpawnDistance
            : playerPosition - enemySpawnDistance;
    }

    private void GenerateFloors()
    {
        var amount = initialRight - initialLeft;

        for (var i = 0; i <= amount; i++)
        {
            var floorX = i + initialLeft;
            var floor = Instantiate(floorPrefab);
            
            floor.transform.position = new Vector3(floorX, floorY, 0f);
            
            _floors.Add(floor);
        }

        _leftmostFloor = _floors[0];
        _rightmostFloor = _floors[_floors.Count - 1];
    }
    
    private void MoveFloors()
    {
        var playerPosition = player.transform.position;
        var playerDirection = player.GetHorizontalDirection();

        switch (playerDirection)
        {
            default:
            case 0:
                return;
            case -1:
                while (Vector3.Distance(_rightmostFloor.transform.position, playerPosition) >= maxFloorDistance)
                {
                    var rightmostFloorPosition = _rightmostFloor.transform.position;
                    _rightmostFloor.transform.position = _leftmostFloor.transform.position + Vector3.left;
                    _leftmostFloor = _floors.Find(floor 
                            => floor.transform.position == _rightmostFloor.transform.position);
                    _rightmostFloor = _floors.Find(floor =>
                        floor.transform.position == rightmostFloorPosition + Vector3.left);
                }
                break;
            case 1:
                while (Vector3.Distance(_leftmostFloor.transform.position, playerPosition) >= maxFloorDistance)
                {
                    var leftmostFloorPosition = _leftmostFloor.transform.position;
                    _leftmostFloor.transform.position = _rightmostFloor.transform.position + Vector3.right;
                    _rightmostFloor = _floors.Find(floor 
                        => floor.transform.position == _rightmostFloor.transform.position + Vector3.right);
                    _leftmostFloor = _floors.Find(floor =>
                        floor.transform.position == leftmostFloorPosition + Vector3.right);
                }
                break;
        }
    }

    private void Update()
    {
        MoveFloors();
        GenerateEnemy();
    }
}