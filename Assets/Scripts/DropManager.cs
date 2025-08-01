using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class DropManager : NetworkBehaviour
{
    public Transform dropPoint;

    [Header("Drop Settings")]
    public Vector3 dropOffset = new Vector3(0, 1.2f, 0.6f);
    public float throwForce = 8f;
    public float upwardForce = 5f;
    public float lifetime = 30f;

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            var inv = GetComponent<InventoryManager>();
            if (inv == null) return;

            ItemData item = inv.GetSelectedItem();
            if (item == null)
            {
                Debug.Log("⚠ 선택된 아이템 없음");
                return;
            }

            bool removed = inv.RemoveItem(item, 1);
            if (!removed)
            {
                Debug.Log("⚠ 아이템 제거 실패");
                return;
            }

            Debug.Log($"✅ {item.itemName} 드랍 요청");
            SpawnDropServerRpc(item.itemName, dropPoint.position, dropPoint.forward);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnDropServerRpc(string itemName, Vector3 origin, Vector3 forward, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(senderId, out var client))
        {
            Debug.LogError($"❌ {senderId} 클라이언트를 찾을 수 없음");
            return;
        }

        var player = client.PlayerObject;
        if (player == null)
        {
            Debug.LogError("❌ PlayerObject 없음");
            return;
        }

        var inv = player.GetComponent<InventoryManager>();
        ItemData item = inv.GetItemByName(itemName);

        if (item == null || item.worldPrefab == null)
        {
            Debug.LogError($"❌ {itemName} worldPrefab 없음");
            return;
        }

        Vector3 dropPosition = origin +
                               forward * dropOffset.z +
                               Vector3.up * dropOffset.y;

        GameObject obj = Instantiate(item.worldPrefab, dropPosition, Quaternion.identity);

        var pickup = obj.GetComponent<ItemPickup>();
        if (pickup != null)
            pickup.itemData = item;

        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;

            Vector3 throwDir = (forward * throwForce) + (Vector3.up * upwardForce);
            rb.AddForce(throwDir, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }

        var netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
            netObj.Spawn(true);

        StartCoroutine(DestroyAfterLifetime(obj, lifetime));
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
