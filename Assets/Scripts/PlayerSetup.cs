using Unity.Netcode;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    public Camera playerCamera;
    public AudioListener audioListener;

    public override void OnNetworkSpawn()
    {
        bool isMine = IsOwner;
        if (playerCamera != null) playerCamera.enabled = isMine;
        if (audioListener != null) audioListener.enabled = isMine;
    }
}