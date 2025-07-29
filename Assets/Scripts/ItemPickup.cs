using UnityEngine;
using Unity.Netcode;

public class ItemPickup : NetworkBehaviour
{
    public ItemData itemData;

    void OnMouseOver()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (itemData == null)
            {
                Debug.LogError("❌ itemData 없음 (ItemPickup에 연결했는지 확인)");
                return;
            }

            Debug.Log($"아이템 줍기 요청: {itemData.itemName}");
            RequestPickupServerRpc(NetworkObject, itemData.itemName);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(NetworkObjectReference itemRef, string itemName, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(senderId, out var client))
        {
            Debug.LogError($"❌ SenderId {senderId} 클라이언트를 찾을 수 없음");
            return;
        }

        var player = client.PlayerObject;
        if (player == null)
        {
            Debug.LogError("❌ PlayerObject가 null");
            return;
        }

        var inv = player.GetComponent<InventoryManager>();
        if (inv == null)
        {
            Debug.LogError("❌ PlayerPrefab에 InventoryManager 없음");
            return;
        }

        // 서버가 직접 AddItemServerRpc 호출 → 이 안에서 Host 중복 방지 로직 처리됨
        inv.AddItemServerRpc(itemName, senderId);

        if (itemRef.TryGet(out NetworkObject netObj))
        {
            netObj.Despawn(true);
        }
    }
}
