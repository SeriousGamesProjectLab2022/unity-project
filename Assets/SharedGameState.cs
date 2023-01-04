using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SharedGameState : NetworkBehaviour
{
    public NetworkVariable<Vector2> spaceshipPosition = new NetworkVariable<Vector2>(new Vector2(0, 0));
    public NetworkVariable<float> spaceshipRotation = new NetworkVariable<float>(0f);

    private bool IsPilot {
        get { return IsHost; }
    }
    private bool IsInstructor {
        get { return !IsHost; }
    }

    public delegate void InstructorReceivedGameState();
    public static event InstructorReceivedGameState OnInstructorReceivedGameState = delegate { };

    public override void OnNetworkSpawn()
    {
        if (IsInstructor)
        {
            DontDestroyOnLoad(this);
            OnInstructorReceivedGameState();
            InstructorReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void InstructorReadyServerRpc()
    {
    }
}
