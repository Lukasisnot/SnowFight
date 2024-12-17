using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

enum MovementType
{
    None,
    Run,
    Jump,
    Fall,
}

public class PlayerMovement : NetworkBehaviour
{
    public bool IsGrounded;
    public int FacingDirection;
    
    [SerializeField] internal Player _master;
    internal Vector2 _inputVector = Vector2.zero;
    internal bool _shouldJump = false;
    internal bool _shouldJumpCut = false;

    [SerializeField] private float _movementSpeed = 10f;
    [SerializeField] private float _runPower = 1.5f;
    [SerializeField] private float _acceleration = 7f;
    [SerializeField] private float _deceleration = 7f;
    [SerializeField] private float _friction = .25f;
    [SerializeField] private float _airControll = 1f;

    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _jumpCut = .5f;
    [SerializeField] private float _coyoteTime = .5f;
    [SerializeField] private float _jumpInterval = .2f;

    [SerializeField] private float _airTime = 0;
    [SerializeField] private float _groundTime = 0;
    [SerializeField] private MovementType _movementType = MovementType.None;
    [SerializeField] private MovementType _lastMovementType = MovementType.None;

    private float _defGravity = 1f;
    [SerializeField] private float _fallGravity = 1.5f;
    [SerializeField] private float _groundCheckDist = .25f;
    [SerializeField] private LayerMask _groundingLayers;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _defGravity = _master._rb.gravityScale;
        }
    }

    void FixedUpdate()
    {
        if (IsOwner)
        {
            GroundCheck();
            Jump();
            JumpCut();
            Gravity();
            Run();
            EvalFacingDir();
        }
    }

    private void EvalFacingDir()
    {
        if (_inputVector.x < 0) FacingDirection = -1;
        else if (_inputVector.x > 0) FacingDirection = 1;
    }

    private void ChangeMovementType(MovementType type)
    {
        if (_movementType == type) return;
        _lastMovementType = _movementType;
        _movementType = type;
    }

    private void GroundCheck()
    {
        Vector2 rayOrigin = (Vector2)_master._box.bounds.center + Vector2.down * _master._box.bounds.extents.y;
        RaycastHit2D hit = Physics2D.BoxCast(rayOrigin,
                                            new(_master._box.bounds.size.x, 0.01f),
                                            0f,
                                            Vector2.down,
                                            _groundCheckDist,
                                            _groundingLayers);

        Debug.DrawLine(rayOrigin, rayOrigin + Vector2.down * _groundCheckDist, Color.red);

        if (hit.collider == null)
        {
            IsGrounded = false;
            // _master._box.excludeLayers = _spearLayer;
        }
        // else if (hit.collider.gameObject.layer != _spearLayerInt)
        // {
        //     IsGrounded = true;
        //     // _master._box.excludeLayers = _spearLayer;
        // }
        else if ((int)(hit.point.y * 100) <= (int)(rayOrigin.y * 100))
        {
            _master._box.excludeLayers = 0;
            IsGrounded = true;
        }

        if (IsGrounded)
        {
            ChangeMovementType(Mathf.Abs(_master._rb.linearVelocity.x) > 0.075f ? MovementType.Run : MovementType.None);
            _airTime = 0f;
            _groundTime += Time.deltaTime;
        }
        else
        {
            _groundTime = 0f;
            _airTime += Time.deltaTime;
        }
    }

    private void Gravity()
    {
        if (_master._rb.linearVelocity.y < -0.01f && !IsGrounded)
        {
            ChangeMovementType(MovementType.Fall);
            _master._rb.gravityScale = _defGravity * _fallGravity;
        }
        else _master._rb.gravityScale = _defGravity;
    }

    private void Jump()
    {
        if (!_shouldJump) return;
        if (_movementType == MovementType.Jump) return;
        if (!IsGrounded && _airTime > _coyoteTime) return;
        if(IsGrounded && _groundTime < _jumpInterval) return;

        ChangeMovementType(MovementType.Jump);
        _master._rb.gravityScale = _defGravity;
        _master._rb.AddForce(Vector2.up * (_jumpForce + Mathf.Abs(_master._rb.linearVelocity.y)), ForceMode2D.Impulse);
    }

    private void JumpCut()
    {
        if (!_shouldJumpCut) return;
        if (IsGrounded || _master._rb.linearVelocity.y < 0.01f) return;

        _shouldJumpCut = false;
        _master._rb.linearVelocity = new Vector2(_master._rb.linearVelocity.x, _master._rb.linearVelocity.y * _jumpCut);
    }

    private void Run()
    {
        #region Run
        float targetSpeed = _inputVector.x * _movementSpeed;
        float speedDif = targetSpeed - _master._rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _acceleration : _deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, _runPower) * Mathf.Sign(speedDif);

        if (!IsGrounded) movement *= _airControll;

        _master._rb.AddForce(movement * Vector2.right);

        #endregion

        #region Friction

        if (IsGrounded && Mathf.Abs(_inputVector.x) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(_master._rb.linearVelocity.x), _friction);
            amount *= -Mathf.Sign(_master._rb.linearVelocity.x);

            _master._rb.AddForce(amount * Vector2.right);
        }

        #endregion
    }
}
