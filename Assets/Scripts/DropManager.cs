using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DropManager : NetworkBehaviour
{
    public InventoryManager inventory;
    public Transform dropPoint;

    [Header("Drop Settings")]
    public Vector3 dropOffset = new Vector3(0, 1.2f, 0.6f); // 중앙 앞쪽
    public float forwardForce = 2f;
    public float upwardForce = 3f;
    public float lifetime = 30f;

    void Update()
    {
        if (!IsOwner) return; // 자기 클라이언트만 입력 처리

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ItemData item = inventory.GetSelectedItem();
            if (item == null)
            {
                Debug.Log("선택된 아이템이 없음");
                return;
            }

            bool removed = inventory.RemoveItem(item, 1);
            if (!removed)
            {
                Debug.Log("인벤토리에서 아이템 제거 실패");
                return;
            }

            // 서버에 드랍 요청
            DropItemServerRpc(item.itemName);
        }
    }

    [ServerRpc]
    private void DropItemServerRpc(string itemName, ServerRpcParams rpcParams = default)
    {
        ItemData item = inventory.GetItemByName(itemName);
        if (item == null || item.worldPrefab == null)
        {
            Debug.LogError($"❌ {itemName} worldPrefab이 null 입니다. ItemData를 확인하세요.");
            return;
        }

        // 드랍 위치 계산
        Vector3 dropPosition = dropPoint.position +
                               dropPoint.forward * dropOffset.z +
                               dropPoint.up * dropOffset.y +
                               dropPoint.right * dropOffset.x;

        GameObject obj = Instantiate(item.worldPrefab, dropPosition, Quaternion.identity);

        // 아이템 데이터 연결
        ItemPickup pickup = obj.GetComponent<ItemPickup>();
        if (pickup != null)
            pickup.itemData = item;

        // 물리 반동 적용
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 throwDirection = dropPoint.forward * forwardForce + Vector3.up * upwardForce;
            rb.AddForce(throwDirection, ForceMode.Impulse);
        }

        // 네트워크 오브젝트 스폰
        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            if (!netObj.IsSpawned)
            {
                netObj.Spawn(true);
            }
            else
            {
                Debug.LogWarning("⚠ 이미 스폰된 NetworkObject 입니다.");
            }
        }
        else
        {
            Debug.LogError("❌ worldPrefab에 NetworkObject가 없습니다!");
        }

        // 일정 시간 후 자동 삭제
        StartCoroutine(DestroyAfterLifetime(obj, lifetime));
    }

    private IEnumerator DestroyAfterLifetime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);

        if (obj != null)
        {
            if (obj.TryGetComponent(out NetworkObject netObj) && netObj.IsSpawned)
            {
                netObj.Despawn(true);
            }
            else
            {
                Destroy(obj);
            }
        }
    }
}
