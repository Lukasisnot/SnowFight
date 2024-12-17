using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class Snowball : NetworkBehaviour
{
    [SerializeField] private float _startSize = .5f;
    [SerializeField] private float _maxSize = 5f;
    [SerializeField] private float _startMass = 1f;
    [SerializeField] private float _maxMass = 5f;
    [SerializeField] private float _sizingSpeed = .1f;
    public float Size { get; private set; }
    public bool IsRolling { get; private set; }
    public bool IsGrabbed { get; private set; }
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private CircleCollider2D _col;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.Log("Snowball spawned");
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _col = GetComponent<CircleCollider2D>();
            IsRolling = false;
            IsGrabbed = false;
            Size = _startSize;
            UpdateSize();
        }
    }

    public void StartRolling()
    {
        Debug.Log("Snowball started rolling");
        IsRolling = true;
        // _rb.bodyType = RigidbodyType2D.Kinematic;
        // _rb.freezeRotation = true;
    }
    
    public void StopRolling()
    {
        Debug.Log("Snowball stopped rolling");
        IsRolling = false;
        // _rb.bodyType = RigidbodyType2D.Dynamic;
        // _rb.freezeRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (IsRolling)
        {
            Size = Mathf.Min(Size + Mathf.Abs(_rb.linearVelocityX) * Time.deltaTime * _sizingSpeed, _maxSize);
            UpdateSize();
        }
    }

    public void UpdateSize()
    {
        if (!IsRolling) return;

        _sr.size = new Vector2(Size, Size);
        _col.radius = Size / 2;
        _rb.mass = Mathf.Lerp(_startMass, _maxMass, Mathf.InverseLerp(_startSize, _maxSize, Size));
    }
}
