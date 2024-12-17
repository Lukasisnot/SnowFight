using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour
{
    private Vector2 _moveInput;
    public NetworkVariable<Color> Skin = new NetworkVariable<Color>();
    [SerializeField] internal NetworkObject _snowballPrefab;
    [SerializeField] private LayerMask _interactableLayers;
    [SerializeField] private int _reachDistance;
    internal PlayerMovement _movement;
    internal PlayerInput _input;
    internal WorldManager _worldManager;
    internal Rigidbody2D _rb;
    internal Collider2D _box;
    internal SpriteRenderer _spriteRenderer;
    private int _snowballStartingRollingDir = 1;
    
    private NetworkObject _snowball;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Skin.Value = Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);
            _worldManager = FindFirstObjectByType<WorldManager>();
        }
        GetComponent<SpriteRenderer>().color = Skin.Value;
        
        if (IsOwner)
        {
            _movement = GetComponent<PlayerMovement>();
            _input = GetComponent<PlayerInput>();
            _rb = GetComponent<Rigidbody2D>();
            _box = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        _spriteRenderer.flipX = _movement.FacingDirection == -1;

        if (_snowball == null) return;
        var snowball = _snowball.GetComponent<Snowball>();
        var snowballRb = snowball.GetComponent<Rigidbody2D>();
        if (snowball.IsRolling && _movement.FacingDirection == _snowballStartingRollingDir)
        {
            _snowball.transform.position = (Vector2)transform.position + Vector2.right * _movement.FacingDirection * (_box.bounds.extents.x + snowball.Size / 2 + 0.1f) + Vector2.up * (snowball.Size / 2 - _box.bounds.extents.y);
        }
        else
        {
            StopRollingSnowball();
        }
    }

    public void OnInteract()
    {
        if (!IsOwner) return;
        
        Debug.Log("Requesting snowball");

        if (_snowball != null)
        {
            StopRollingSnowball();
            return;
        }

        var cast = Cast();
        _snowballStartingRollingDir = _movement.FacingDirection;
        
        if (cast.collider != null) _snowball = cast.collider.GetComponent<NetworkObject>();
        if (_snowball != null)
        {
            StartRollingSnowball();
        } 
        else if (_movement.IsGrounded)
        {
            // _snowball = _worldManager.SpawnObject(_snowballPrefab, transform.position);
            SpawnSnowballServerRPC(transform.position, OwnerClientId);
            StartRollingSnowball();
        }
    }

    RaycastHit2D Cast(Vector2? dir = null)
    {
        if (dir == null) dir = Vector2.right * _movement.FacingDirection;
        return Physics2D.BoxCast(_box.bounds.center, 
            _box.bounds.size,
            0f,
            (Vector3)dir,
            _reachDistance,
            _interactableLayers);
    }

    void StartRollingSnowball()
    {
        if (!_snowball.IsOwnedByServer)
        {
            Debug.LogWarning("Trying to start rolling snowball but not owned by server");
            return;
        }
        
        _snowball.RequestOwnership();
        if (_snowball.OwnerClientId == OwnerClientId)
        {
            _snowball.GetComponent<Snowball>().StartRolling();
            Debug.Log("Rolled snowball");
            return;
        }

        _snowball = null;
        Debug.LogWarning("Trying to start rolling snowball but not owned by client");
    }
    
    void StopRollingSnowball()
    {
        _snowball.GetComponent<Snowball>().StopRolling();
        RemoveObjOwnershipServerRPC(_snowball.NetworkObjectId);
        _snowball = null;
    }
    
    [Rpc(SendTo.Server)]
    public void RemoveObjOwnershipServerRPC(ulong id)
    {
        GetNetworkObject(id).RemoveOwnership();
    } 
    
    [Rpc(SendTo.Server)]
    public void SpawnSnowballServerRPC(Vector3 pos, ulong ownerClientId)
    {
        var snowball = _worldManager.SpawnObject(_snowballPrefab, pos);
        SetSnowballClientRPC(ownerClientId, snowball.NetworkObjectId);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    public void SetSnowballClientRPC(ulong ownerClientId, ulong snowballId)
    {
        if (ownerClientId == OwnerClientId)
            _snowball = GetNetworkObject(snowballId);
    }
}
