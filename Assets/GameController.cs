using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Vector3 enemySpawnDistance;
    [SerializeField] private float enemyYPosition;
    [SerializeField] private float enemySpawnDuration;
    [SerializeField] private float maxEnemyCount;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private int initialLeft;
    [SerializeField] private int initialRight;
    [SerializeField] private float floorY;
    [SerializeField] private float maxFloorDistance;
    [SerializeField] private EndCondition _gameIsPaused;
    [SerializeField] private List<EndCondition> _endConditions;
    [SerializeField] private TextMeshProUGUI _conditionText;


    private List<EnemyController> _leftEnemies;
    private List<EnemyController> _rightEnemies;
    
    private GameObject _leftmostFloor;
    private GameObject _rightmostFloor;
    private List<GameObject> _floors;
    private float _enemySpawnCounter;

    private void Awake()
    {
        _leftEnemies = new List<EnemyController>();
        _rightEnemies = new List<EnemyController>();
        _floors = new List<GameObject>();
        _enemySpawnCounter = enemySpawnDuration;
        GenerateFloors();
        UpdateConditionText();
    }

    public void ChangeScore(AttackType attackType, int enemyScore)
    {
        var scoreCondition = _endConditions.Find(x =>
            x.specialEndConditionName == SpecialEndConditionName.PlayerScore);

        if (scoreCondition == null)
        {
            Debug.Log("Score Condition is Null.");
            return;
        }

        switch (attackType)
        {
            default:
                scoreCondition.value += enemyScore;
                break;
            case AttackType.Multiply:
                scoreCondition.value *= enemyScore;
                break;
            case AttackType.Sum:
                scoreCondition.value += enemyScore;
                break;
            case AttackType.Substract:
                scoreCondition.value -= enemyScore;
                break;
            case AttackType.Divide:
                scoreCondition.value /= enemyScore;
                break;
            case AttackType.CommentOut:
                break;
        }
        
        UpdateConditionText();
    }

    public void UpdateConditionText()
    {
        var newText = "while (";
        var comments = "" /*+ _gameIsPaused.GetCommentText() + " "*/;
        var conditionals = "" + _gameIsPaused.GetConditionText() + " && ";
        
        if (_endConditions.Count > 0)
        {
            for (int i = 0; i < _endConditions.Count; i++)
            {
                var padding =  i == _endConditions.Count - 1 ? "" : " && ";
                var paddingComment =  i == _endConditions.Count - 1 ? "" : " ";
                comments += _endConditions[i].GetCommentText() + paddingComment;
                conditionals += _endConditions[i].GetConditionText() + padding;
            }
        }
        
        _conditionText.text = newText + conditionals + ")\n{\n  // " + comments;
        _conditionText.text += "\n  // Selected Operator: " + player.GetAttackTypeString();
        
        CheckEndConditions();
    }

    private void CheckEndConditions()
    {
        if (_endConditions.All(x => x.DoesEndConditionMet() == false))
        {
            Debug.Log("Game Has Ended.");
        }
    }
    
    public void RemoveEnemyFromList(EnemyController enemyController)
    {
        var enemyId = enemyController.Id;
        var isRight = enemyController.IsRight;

        if (isRight)
        {
            _rightEnemies.RemoveAt(_rightEnemies.FindIndex(x => x.Id == enemyId));
        }
        else
        {
            _leftEnemies.RemoveAt(_leftEnemies.FindIndex(x => x.Id == enemyId));
        }
    }

    private void MoveEnemies()
    {
        if (player.castingSkill)
        {
            return;
        }
        
        for (int i = 0; i < _rightEnemies.Count; i++)
        {
            var current = _rightEnemies[i];
            
            if (i == 0)
            {
                current.GoTowards(player.transform.position);
                continue;
            }
            
            current.GoTowards(_rightEnemies[i-1].transform.position);
        }
        
        for (int i = 0; i < _leftEnemies.Count; i++)
        {
            var current = _leftEnemies[i];
            
            if (i == 0)
            {
                current.GoTowards(player.transform.position);
                continue;
            }
            
            current.GoTowards(_leftEnemies[i-1].transform.position);
        }
    }

    private void GenerateEnemy()
    {
        _enemySpawnCounter += Time.deltaTime;

        if (_leftEnemies.Count + _rightEnemies.Count >= maxEnemyCount)
        {
            return;
        }
        
        if (_enemySpawnCounter < enemySpawnDuration)
        {
            return;
        }

        _enemySpawnCounter = 0f;
        
        var enemy = Instantiate(enemyPrefab).GetComponent<EnemyController>();
        enemy.Initialize(this, player);

        var playerPosition = player.transform.position;
        playerPosition.y = enemyYPosition;

        if (player.FacingRight())
        {
            enemy.transform.position = playerPosition + enemySpawnDistance;
            enemy.IsRight = true;
            _rightEnemies.Add(enemy);
        }
        
        else
        {
            enemy.transform.position = playerPosition - enemySpawnDistance;
            enemy.IsRight = false;
            _leftEnemies.Add(enemy);
        }
    }

    public EnemyController GetEnemyInFront(bool isRight)
    {
        if (isRight)
        {
            if (_rightEnemies.Count <= 0)
            {
                return null;
            }
            return _rightEnemies[0];
        }
        else
        {
            if (_leftEnemies.Count <= 0)
            {
                return null;
            }

            return _leftEnemies[0];
        }
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

    private void CheckForPause()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            
            return;
        }

        _gameIsPaused.value = (_gameIsPaused.value + 1) % 2; 
        
        Time.timeScale = _gameIsPaused.value == 0 ? 1 : 0;
    }

    public bool GameIsPaused()
    {
        return _gameIsPaused.value != 0;
    }

    private void Update()
    {
        CheckForPause();
        MoveFloors();
        GenerateEnemy();
        MoveEnemies();
        CheckEndConditions();
    }
}
