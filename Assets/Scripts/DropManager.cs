using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class DropManager : NetworkBehaviour
{
    public Transform dropPoint;

    [Header("Drop Settings")]
    public Vector3 dropOffset = new Vector3(0, 1.2f, 0.6f);
    public float forwardForce = 2f;
    public float upwardForce = 3f;
    public float lifetime = 30f;

    void Update()
    {
        if (!IsOwner) return; // 자기 클라이언트만 입력 처리

        if (Input.GetKeyDown(KeyCode.Q))
        {
            var inv = GetComponent<InventoryManager>();
            if (inv == null) return;

            ItemData item = inv.GetSelectedItem();
            if (item == null)
            {
                Debug.Log("선택된 아이템 없음");
                return;
            }

            // 서버에 드랍 요청
            DropItemServerRpc(item.itemName);
        }
    }

    [ServerRpc]
    private void DropItemServerRpc(string itemName, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(senderId, out var client))
        {
            Debug.LogError($"❌ SenderId {senderId} 클라이언트 찾을 수 없음");
            return;
        }

        var player = client.PlayerObject;
        var inv = player.GetComponent<InventoryManager>();
        if (inv == null)
        {
            Debug.LogError("❌ PlayerPrefab에 InventoryManager 없음");
            return;
        }

        // 서버에서 아이템 제거
        var item = inv.GetItemByName(itemName);
        if (item == null)
        {
            Debug.LogError($"❌ {itemName} 아이템을 찾을 수 없음");
            return;
        }

        bool removed = inv.RemoveItem(item, 1);
        if (!removed)
        {
            Debug.Log("❌ 인벤토리에서 아이템 제거 실패");
            return;
        }

        // 드랍 위치
        Vector3 dropPosition = player.transform.position +
                               player.transform.forward * dropOffset.z +
                               Vector3.up * dropOffset.y +
                               player.transform.right * dropOffset.x;

        // 아이템 오브젝트 생성
        GameObject obj = Instantiate(item.worldPrefab, dropPosition, Quaternion.identity);

        var pickup = obj.GetComponent<ItemPickup>();
        if (pickup != null)
            pickup.itemData = item;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 throwDir = player.transform.forward * forwardForce + Vector3.up * upwardForce;
            rb.AddForce(throwDir, ForceMode.Impulse);
        }

        var netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
            netObj.Spawn(true);

        StartCoroutine(DestroyAfterLifetime(obj, lifetime));

        // 클라이언트 UI 갱신
        inv.RefreshInventoryClientRpc();
    }

    private IEnumerator DestroyAfterLifetime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);

        if (obj != null)
        {
            if (obj.TryGetComponent(out NetworkObject netObj) && netObj.IsSpawned)
                netObj.Despawn(true);
            else
                Destroy(obj);
        }
    }
}
