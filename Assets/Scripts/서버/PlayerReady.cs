using System;
using UnityEngine;
using Unity.Netcode;

public class PlayerReady : NetworkBehaviour
{
    public static event Action OnAllPlayersReady;

    public NetworkVariable<bool> isReady = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void OnEnable() { isReady.OnValueChanged += OnReadyChanged; }
    private void OnDisable() { isReady.OnValueChanged -= OnReadyChanged; }

    // UI에서 호출
    public void ToggleReadyFromUI()
    {
        if (!IsOwner) return;
        SetReadyServerRpc(!isReady.Value);
    }

    // 명시적 설정이 필요하면 사용
    public void RequestSetReady(bool ready)
    {
        if (!IsOwner) return;
        SetReadyServerRpc(ready);
    }

    [ServerRpc(RequireOwnership = true)]
    private void SetReadyServerRpc(bool ready)
    {
        isReady.Value = ready;

        // 서버에서 모두 준비됐는지 검사
        CheckAllReadyOnServer();
    }

    private void OnReadyChanged(bool prev, bool now)
    {
        // 필요 시 로컬 효과
    }

    private void CheckAllReadyOnServer()
    {
        if (!IsServer) return;

        var list = NetworkManager.Singleton.ConnectedClientsList;
        for (int i = 0; i < list.Count; i++)
        {
            var pr = list[i].PlayerObject.GetComponent<PlayerReady>();
            if (pr == null || !pr.isReady.Value)
                return;
        }

        OnAllPlayersReady?.Invoke();
    }
}
