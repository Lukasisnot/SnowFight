using System;
using Unity.Netcode;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    private NetworkManager _NetworkManager;

    void Awake()
    {
        _NetworkManager = GetComponent<NetworkManager>();
    }

    void OnGUI()
    {
        if (_NetworkManager == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!_NetworkManager.IsClient && !_NetworkManager.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("Host")) _NetworkManager.StartHost();
        if (GUILayout.Button("Client")) _NetworkManager.StartClient();
        if (GUILayout.Button("Server")) _NetworkManager.StartServer();
    }

    void StatusLabels()
    {
        var mode = _NetworkManager.IsHost ? "Host" : _NetworkManager.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " + _NetworkManager.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    public NetworkObject SpawnObject(NetworkObject obj, Vector3 position = default(Vector3))
    {
        Debug.Log("Spawning object");
        var instance = Instantiate(obj, position, Quaternion.identity);
        instance.GetComponent<NetworkObject>().Spawn();
        return instance;
    }
    
}