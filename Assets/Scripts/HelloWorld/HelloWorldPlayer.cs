using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public NetworkVariable<Color> Skin = new NetworkVariable<Color>();

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                Move();
            }

            if (IsServer)
            {
                Skin.Value = Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);
            }
            
            GetComponent<SpriteRenderer>().color = Skin.Value;
        }

        public void Move()
        {
            SubmitPositionRequestRpc();
        }

        [Rpc(SendTo.Server)]
        void SubmitPositionRequestRpc(RpcParams rpcParams = default)
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }
        
        // [Rpc(SendTo.Server)]
        // void SubmitColorRequestRpc(RpcParams rpcParams = default)
        // {
        //      Skin.Value = Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);
        //      GetComponent<SpriteRenderer>().color = Skin.Value;
        // }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0f);
        }

        void Update()
        {
            transform.position = Position.Value;
        }
    }
}