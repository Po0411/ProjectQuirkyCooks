using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class DropManager : NetworkBehaviour
{
    public InventoryManager inventory;
    public Transform dropPoint;

    [Header("Drop Settings")]
    public Vector3 dropOffset = new Vector3(0, 0.5f, 1f); // 앞쪽으로
    public float forwardForce = 6f;   // 앞으로 더 강하게
    public float upwardForce = 4f;    // 위쪽 힘
    public float lifetime = 30f;

    void Start()
    {
        // dropPoint가 설정 안 되어 있으면 카메라 앞쪽 기준으로 자동 설정
        if (dropPoint == null && Camera.main != null)
        {
            GameObject dp = new GameObject("DropPoint");
            dp.transform.SetParent(Camera.main.transform);
            dp.transform.localPosition = new Vector3(0, -0.2f, 0.8f); // 카메라 앞쪽
            dropPoint = dp.transform;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (inventory == null)
            {
                Debug.LogError("❌ InventoryManager 없음");
                return;
            }

            ItemData item = inventory.GetSelectedItem();
            if (item == null)
            {
                Debug.Log("❌ 선택된 아이템이 없음");
                return;
            }

            if (!inventory.RemoveItem(item, 1))
            {
                Debug.Log("❌ 인벤토리에서 아이템 제거 실패");
                return;
            }

            DropItemServerRpc(item.itemName);
        }
    }

    [ServerRpc]
    private void DropItemServerRpc(string itemName, ServerRpcParams rpcParams = default)
    {
        ItemData item = inventory.GetItemByName(itemName);
        if (item == null || item.worldPrefab == null)
        {
            Debug.LogError($"❌ {itemName} worldPrefab이 null 입니다.");
            return;
        }

        Vector3 dropPosition = dropPoint.position + dropPoint.forward * dropOffset.z;

        GameObject obj = Instantiate(item.worldPrefab, dropPosition, Quaternion.identity);

        ItemPickup pickup = obj.GetComponent<ItemPickup>();
        if (pickup != null)
            pickup.itemData = item;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero; // 초기 속도 초기화

            // 던지는 방향
            Vector3 throwDir = dropPoint.forward * forwardForce + Vector3.up * upwardForce;
            rb.AddForce(throwDir, ForceMode.Impulse);
        }

        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null)
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
