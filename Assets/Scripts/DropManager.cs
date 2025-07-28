using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropManager : MonoBehaviour
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

            if (item.worldPrefab == null)
            {
                Debug.LogError($"❌ {item.name} 의 worldPrefab 이 null 입니다. ItemData에 프리팹 연결하세요.");
                return;
            }

            // 드랍 위치 계산
            Vector3 dropPosition = dropPoint.position +
                                   dropPoint.forward * dropOffset.z +
                                   dropPoint.up * dropOffset.y +
                                   dropPoint.right * dropOffset.x;

            GameObject obj = Instantiate(item.worldPrefab, dropPosition, Quaternion.identity);

            ItemPickup pickup = obj.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.itemData = item;
            }

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 throwDirection = dropPoint.forward * forwardForce + Vector3.up * upwardForce;
                rb.AddForce(throwDirection, ForceMode.Impulse);
            }

            Destroy(obj, lifetime);
        }
    }
}