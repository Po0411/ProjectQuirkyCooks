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
            // 서버에 드랍 생성 요청
            SpawnDropServerRpc(item.itemName, dropPoint.position, dropPoint.forward);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnDropServerRpc(string itemName, Vector3 origin, Vector3 forward, ServerRpcParams rpcParams = default)
    {
        // 서버에서 아이템 데이터 찾기
        var invManager = FindObjectOfType<InventoryManager>();
        ItemData item = invManager.GetItemByName(itemName);

        if (item == null || item.worldPrefab == null)
        {
            Debug.LogError($"❌ {itemName} worldPrefab 없음");
            return;
        }

        // 드랍 위치 계산
        Vector3 dropPosition = origin +
                               forward * dropOffset.z +
                               Vector3.up * dropOffset.y +
                               Vector3.right * dropOffset.x;

        // 오브젝트 생성
        GameObject obj = Instantiate(item.worldPrefab, dropPosition, Quaternion.identity);

        var pickup = obj.GetComponent<ItemPickup>();
        if (pickup != null)
            pickup.itemData = item;

        // 물리 효과
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 throwDir = forward * forwardForce + Vector3.up * upwardForce;
            rb.AddForce(throwDir, ForceMode.Impulse);
        }

        // 네트워크 오브젝트 스폰
        var netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
            netObj.Spawn(true);

        // 일정 시간 후 자동 제거
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
