using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerFlip))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PlayerAnimationManager))]
public class PlayerController : MonoBehaviour
{
    private BoxCollider2D _collider2D;
    private Rigidbody2D _rigidbody2D;
    private float _timeElapsedSinceLastJump;
    private float _timeElapsedSinceLastGrounded;
    private PlayerAnimationManager _playerAnimationManager;
    private PlayerFlip _playerFlip;
    
    [SerializeField] private float groundedHeight;
    [SerializeField] private LayerMask _jumpEnabledGrounds;
    [SerializeField] private float _groundColliderHeight = 1f;
    [SerializeField] private float _jumpVelocity = 10f; 
    [SerializeField] private float _allowedTimeBetweenJumps = 0.15f;
    [SerializeField] private float _horizontalSpeed = 3f;
    [Range(0,1)] [SerializeField] private float _horizontalDampingNormal = 0.22f;
    [Range(0,1)] [SerializeField] private float _horizontalDampingWhenStopping = 0.22f;
    [Range(0,1)] [SerializeField] private float _horizontalDampingWhenTurning = 0.22f;
    [Range(0,1)] [SerializeField] private float _coyoteTime = 0.2f;
    [Range(0,1)] [SerializeField] private float _cutJumpHeight = 0.3f;

    private void Awake()
    {
        _playerFlip = GetComponent<PlayerFlip>();
        _collider2D = GetComponent<BoxCollider2D>(); 
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _playerAnimationManager = GetComponent<PlayerAnimationManager>();
        _timeElapsedSinceLastJump = 0f;
    }

    public bool FacingRight()
    {
        return _playerFlip.FacingRight;
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        // if (!other.gameObject.CompareTag($"Enemy"))
        // {
        //     return;
        // }

        // _playerAnimationManager.SetState(PlayerAnimationState.Dead); // TODO: Uncomment this for death condition.
    }

    private bool IsGrounded(bool drawCollider = false)
    {
        // var grounded = Physics2D.OverlapBox(_collider2D.bounds.center - new Vector3(0f, _collider2D.bounds.extents.y + (_groundColliderHeight * 0.5f)), new Vector3(_collider2D.bounds.extents.x, _groundColliderHeight * 0.5f), 0, _jumpEnabledGrounds);
        //
        // if (drawCollider)
        // {
        //     var rayColor = !grounded ? Color.red : Color.green;
        //
        //     Debug.DrawLine(_collider2D.bounds.center - new Vector3(_collider2D.bounds.extents.x, _collider2D.bounds.extents.y + _groundColliderHeight), _collider2D.bounds.center - new Vector3(-_collider2D.bounds.extents.x, _collider2D.bounds.extents.y + _groundColliderHeight), rayColor);
        //     Debug.DrawLine(_collider2D.bounds.center - new Vector3(_collider2D.bounds.extents.x, _collider2D.bounds.extents.y), _collider2D.bounds.center - new Vector3(_collider2D.bounds.extents.x, _collider2D.bounds.extents.y + _groundColliderHeight) , rayColor);
        //     Debug.DrawLine(_collider2D.bounds.center - new Vector3(-_collider2D.bounds.extents.x, _collider2D.bounds.extents.y), _collider2D.bounds.center - new Vector3(-_collider2D.bounds.extents.x, _collider2D.bounds.extents.y + _groundColliderHeight), rayColor);
        // }
        //
        // return grounded;

        return transform.position.y <= groundedHeight;

    }

    private void Jump()
    {
        _timeElapsedSinceLastJump -= Time.deltaTime;
        _timeElapsedSinceLastGrounded -= Time.deltaTime;

        if (IsGrounded(true))
        {
            _timeElapsedSinceLastGrounded = _coyoteTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            _timeElapsedSinceLastJump = _allowedTimeBetweenJumps;
        }

        if (Input.GetButtonUp("Jump") && _rigidbody2D.velocity.y > 0f)
        {
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _rigidbody2D.velocity.y * _cutJumpHeight);
        }

        if ((_timeElapsedSinceLastJump > 0f) && (_timeElapsedSinceLastGrounded > 0f))
        {
            _timeElapsedSinceLastJump = 0f;
            _timeElapsedSinceLastGrounded = 0f;
            
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpVelocity);
        }   
    }

    private void Move(bool autoMove = false)
    {
        var horizontalVelocity = _rigidbody2D.velocity.x;
        horizontalVelocity += _horizontalSpeed * Input.GetAxisRaw("Horizontal");
        
        var horizontalDamping = _horizontalDampingNormal;

        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.01f)
        {
            horizontalDamping = _horizontalDampingWhenStopping;
            _playerAnimationManager.SetState(PlayerAnimationState.Idle);
        }

        else if (Mathf.Sign(Input.GetAxisRaw("Horizontal")) != Mathf.Sign(horizontalVelocity))  
        {
            horizontalDamping = _horizontalDampingWhenTurning;
        }

        horizontalVelocity *= Mathf.Pow(1f - horizontalDamping, 10f);
    
        if (horizontalVelocity != 0f)
        {
            _playerAnimationManager.SetState(PlayerAnimationState.Walking);
        }
        
        _rigidbody2D.velocity = new Vector2(horizontalVelocity, _rigidbody2D.velocity.y);
    }

    public int GetHorizontalDirection()
    {
        if (_rigidbody2D.velocity.x > 0)
        {
            return 1;
        }
        
        else if (_rigidbody2D.velocity.x < 0)
        {
            return -1;
        }

        return 0;
    }
    

    private void Update() 
    {
        Jump(); // TODO: Decide on Including Jump.
        Move();
    }  
}