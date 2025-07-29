using UnityEngine;
using Unity.Netcode;

public class ItemPickup : NetworkBehaviour
{
    public ItemData itemData;

    void OnMouseOver()
    {
        if (!IsOwner) return; // 내 클라만 입력 받음

        if (Input.GetMouseButtonDown(0))
        {
            if (itemData == null)
            {
                Debug.LogError("❌ itemData 없음 (프리팹에 연결했는지 확인)");
                return;
            }

            // 내 Player 오브젝트 찾기
            var myPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (myPlayer == null)
            {
                Debug.LogError("❌ 내 PlayerObject 없음");
                return;
            }

            var inv = myPlayer.GetComponent<InventoryManager>();
            if (inv == null)
            {
                Debug.LogError("❌ 내 Player에 InventoryManager 없음");
                return;
            }

            // 인벤토리에 추가
            bool added = inv.AddItemLocal(itemData);
            if (added)
            {
                Debug.Log($"✅ {itemData.itemName} 먹음");
                // 서버에 Despawn 요청
                RequestDespawnServerRpc(NetworkObject);
            }
            else
            {
                Debug.Log("⚠ 인벤토리 가득 참");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDespawnServerRpc(NetworkObjectReference itemRef)
    {
        if (itemRef.TryGet(out NetworkObject netObj))
        {
            netObj.Despawn(true); // 서버에서 제거 → 모든 클라 동기화
        }
    }
}
