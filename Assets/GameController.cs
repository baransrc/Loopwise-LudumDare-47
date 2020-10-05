using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
    [SerializeField] private TextMeshProUGUI _conditionText;
    [SerializeField] private float _attackRadius;
    [SerializeField] private List<LevelData> _levelData;
    [SerializeField] private StripeManager _stripeManager;
    
    private int _levelIndex = 0;
    private List<EndCondition> _endConditions;
    
    public CameraUtility cameraUtility;

    private List<EnemyController> _leftEnemies;
    private List<EnemyController> _rightEnemies;
    private bool _canEnemiesMove = true;
    public bool mainMenu = true;
    
    private GameObject _leftmostFloor;
    private GameObject _rightmostFloor;
    private List<GameObject> _floors;
    private float _enemySpawnCounter;
    private bool _canGenerateEnemies = true;
    private bool _canCheckForEndConditions = true;
    public bool loadingNextLevel;

    private void Awake()
    {
        _leftEnemies = new List<EnemyController>();
        _rightEnemies = new List<EnemyController>();
        _floors = new List<GameObject>();
        _enemySpawnCounter = enemySpawnDuration;
        GenerateFloors();
        
        _stripeManager.PlayStripes();
        _endConditions = _levelData[_levelIndex].EndConditions;
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
        if (mainMenu)
        {
            _conditionText.text =
                "while(gameStarted == false)\n{\n  // This wizard is Loopwise!\n  // To break out of loops,\n  // He uses operators on monsters!" +
                "\n  // Press Enter to play the game!";
            return;
        }
        
        var newText = "while (";
        var comments =  "";
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

        comments += !_gameIsPaused.DoesEndConditionMet() ? "\n  // Game is paused." : "";
        _conditionText.text = newText + conditionals + ")\n{\n  // " + comments;
        _conditionText.text += "\n  // Selected Operator: " + player.GetAttackTypeString();
        
        CheckEndConditions();
    }

    private void CheckEndConditions()
    {
        if (!_canCheckForEndConditions)
        {
            return;
        }
        
        if (_endConditions.All(x => !x.DoesEndConditionMet()))
        {
            Debug.Log("Conditions are met");
            StartCoroutine(NextLevelCoroutine());
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

        if (!_canEnemiesMove)
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

        if (!_canGenerateEnemies)
        {
            return;
        }
        
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
        var randomizedValue = Mathf.Abs(_endConditions[_endConditions.FindIndex(x => x.specialEndConditionName == SpecialEndConditionName.PlayerScore)].GetValueToRandomize() / 4);
        var score = Random.Range(-randomizedValue, randomizedValue);

        if (score == 0)
        {
            score = 1;
        }
        
        enemy.Initialize(this, player,score);

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

    public IEnumerator NextLevelCoroutine()
    {
        loadingNextLevel = true;
        _canEnemiesMove = false;
        _canGenerateEnemies = false;
        _canCheckForEndConditions = false;
        
        yield return new WaitForSeconds(2f);

        foreach (var enemy in _leftEnemies)
        {
            Destroy(enemy.gameObject);
        }

        foreach (var enemy in _rightEnemies)
        {
            Destroy(enemy.gameObject);
        }
        
        _stripeManager.PlayStripes();
        AudioManager.Instance.PlaySound(Sounds.LevelUp);
        _leftEnemies.Clear();
        _rightEnemies.Clear();

        _levelIndex = (_levelIndex + 1) % _levelData.Count;

        _endConditions = _levelData[_levelIndex].EndConditions; 
        
        player.isDead = false;
        
        _canEnemiesMove = true;
        _canCheckForEndConditions = true;
        _canGenerateEnemies = true;
        
        UpdateConditionText();

        loadingNextLevel = false;
    }
    
    public IEnumerator LoseGameCoroutine()
    {
        _canEnemiesMove = false;
        _canGenerateEnemies = false;
        _canCheckForEndConditions = false;

        yield return new WaitForSeconds(2f);
        
        _stripeManager.PlayStripes();

        foreach (var enemy in _leftEnemies)
        {
            Destroy(enemy.gameObject);
        }

        foreach (var enemy in _rightEnemies)
        {
            Destroy(enemy.gameObject);
        }
        
        _leftEnemies.Clear();
        _rightEnemies.Clear();

        foreach (var endCondition in _endConditions)
        {
            endCondition.ResetCondition();
        }
        
        player.isDead = false;
        
        _canEnemiesMove = true;
        _canCheckForEndConditions = true;
        _canGenerateEnemies = true;
        
        UpdateConditionText();
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

    void CheckForGameStart()
    {
        if (!mainMenu)
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.Return))
        {
            return;
        }

        mainMenu = false;
        
        UpdateConditionText();
    }
    
    private void CheckForPause()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        _gameIsPaused.value = (_gameIsPaused.value + 1) % 2; 
        
        Time.timeScale = _gameIsPaused.value == 0 ? 1 : 0;
        
        UpdateConditionText();
    }

    public bool GameIsPaused()
    {
        return _gameIsPaused.value != 0;
    }

    private void Update()
    {
        CheckForGameStart();
        
        if (mainMenu)
        {
            return;
        }

        if (loadingNextLevel)
        {
            return;
        }
        
        CheckForPause();
        MoveFloors();
        GenerateEnemy();
        MoveEnemies();
        CheckEndConditions();
    }
}
