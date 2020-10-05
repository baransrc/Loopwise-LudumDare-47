using UnityEngine;

public class CameraUtility : MonoBehaviour
{
    [SerializeField] private Transform _target = null;
    [SerializeField] private float _smoothingAmount = 1f;
    [SerializeField] private bool _followX = false;
    [SerializeField] private bool _followY = false;
    [SerializeField] private bool _followZ = false;
    
    [SerializeField] private float _defaultShakeMagnitude = 0.2f;
    [SerializeField] private float _defaultDampingSpeed = 0.9f;
    private float _shakeMagnitude = 0.2f;
    private float _dampingSpeed = 0.5f;
    private float _shakeDuration = 0f;
    
    private Vector3 _offset;
    private Vector3 _initialPosition;

    private void Awake()
    {
        _initialPosition = transform.position;
        _offset = _initialPosition - _target.position;
    }
    
    public void TriggerShake(float shakeDuration = 0.2f, float dampingSpeedMultiplier = 1f, float shakeMagnitudeMultiplier = 1f) 
    {
        if (dampingSpeedMultiplier <= 0f) return;
        if (_shakeDuration > shakeDuration) return;
        
        _shakeDuration = shakeDuration;
        _shakeMagnitude = _defaultShakeMagnitude * shakeMagnitudeMultiplier; 
        _dampingSpeed = _defaultDampingSpeed * dampingSpeedMultiplier;
    }

    private void Shake()
    {
        if (_shakeDuration > 0)
        {
            transform.localPosition = _target.position + _offset + new Vector3(Random.Range(-1f, 1f),Random.Range(-1f, 1f), 0f) * _shakeMagnitude;
   
            _shakeDuration -= Time.deltaTime * _dampingSpeed;
        }
        else
        {
            _shakeDuration = 0f;
            _dampingSpeed = _defaultDampingSpeed;
            _shakeMagnitude = _defaultShakeMagnitude;
        }
    }

    private Vector3 GetSmoothedPosition()
    {
        var currentPos = transform.position;
        var targetX = (_followX ? _target.position.x + _offset.x : currentPos.x);
        var targetY = (_followY ? _target.position.y + _offset.y : currentPos.y);
        var targetZ = (_followZ ? _target.position.z + _offset.z : currentPos.z);
        
        return Vector3.Lerp(currentPos, new Vector3(targetX, targetY, targetZ), _smoothingAmount * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        Shake();
        
        if(_shakeDuration <= 0f)
        {
            transform.position = GetSmoothedPosition();
            _initialPosition = transform.position;
        }
    }
}
