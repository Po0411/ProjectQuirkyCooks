using UnityEngine;
using Unity.Netcode;

public class ItemPickup : NetworkBehaviour
{
    public ItemData itemData;

    void OnMouseOver()
    {
        if (!IsOwner) return; // 자기 캐릭터일 때만
        if (Input.GetMouseButtonDown(0))
        {
            InventoryManager inv = FindObjectOfType<InventoryManager>();
            if (inv != null)
            {
                inv.AddItem(itemData);

                // 서버에 아이템 제거 요청
                RequestDestroyServerRpc(NetworkObject);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDestroyServerRpc(NetworkObjectReference itemRef)
    {
        if (itemRef.TryGet(out NetworkObject netObj))
        {
            netObj.Despawn(true); // 모든 클라이언트에서 제거
        }
    }
}
