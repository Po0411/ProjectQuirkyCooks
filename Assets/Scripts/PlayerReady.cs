// Assets/Scripts/PlayerReady.cs
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerReady : NetworkBehaviour
{
    public static event System.Action OnAllPlayersReady;
    private static List<PlayerReady> players = new List<PlayerReady>();

    public NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);

    private void Awake()
    {
        players.Add(this);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // 초기 준비 상태 false
            isReady.Value = false;
        }
        // 값 변경 시 UI 업데이트 콜백 등록
        isReady.OnValueChanged += (oldVal, newVal) => { UIManager.Instance.UpdateReadyState(this); CheckAllReady(); };
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(bool ready)
    {
        isReady.Value = ready;
    }

    private static void CheckAllReady()
    {
        foreach (var p in players)
        {
            if (!p.isReady.Value) return;
        }
        OnAllPlayersReady?.Invoke();
    }
}
