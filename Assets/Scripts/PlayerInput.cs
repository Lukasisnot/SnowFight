using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : NetworkBehaviour
{
    [SerializeField] internal Player _master;
    private InputSystem_Actions _actions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _actions = new InputSystem_Actions();

            _actions.Enable();
            _actions.Player.Jump.started += (InputAction.CallbackContext ctx) => { _master._movement._shouldJump = true; _master._movement._shouldJumpCut = false; };
            _actions.Player.Jump.canceled += (InputAction.CallbackContext ctx) => { _master._movement._shouldJump = false; _master._movement._shouldJumpCut = true; };
            _actions.Player.Interact.started += (InputAction.CallbackContext ctx) => { _master.OnInteract(); Debug.Log("Interact started"); };
        }
    }

    void Update()
    {
        if (IsOwner)
            _master._movement._inputVector = _actions.Player.Move.ReadValue<Vector2>();
    }
}
