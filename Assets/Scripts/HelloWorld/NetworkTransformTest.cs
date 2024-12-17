using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetworkTransformTest : NetworkBehaviour
{
    private Vector3 _basePosition;
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
            _basePosition = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0f);
    }

    void Update()
    {
        if (IsServer)
        {
            float theta = Time.frameCount / 10.0f;
            transform.position = new Vector3((float) Math.Cos(theta) + _basePosition.x, (float) Math.Sin(theta) + _basePosition.y, 0.0f);
        }
    }
}