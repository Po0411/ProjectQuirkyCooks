using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropManager : MonoBehaviour
{
    public InventoryManager inventory;
    public Transform dropPoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 현재 선택된 슬롯의 아이템 가져오기
            ItemData item = inventory.GetSelectedItem();
            if (item == null)
            {
                Debug.Log("선택된 아이템이 없음");
                return;
            }

            // 인벤토리에서 1개 제거
            bool removed = inventory.RemoveItem(item, 1);
            if (!removed)
            {
                Debug.Log("인벤토리에서 아이템 제거 실패");
                return;
            }

            // 아이템에 연결된 프리팹으로 드랍
            if (item.worldPrefab == null)
            {
                Debug.LogError($"❌ {item.name} 의 worldPrefab 이 null 입니다. ItemData에 프리팹 연결하세요.");
                return;
            }

            GameObject obj = Instantiate(item.worldPrefab, dropPoint.position, Quaternion.identity);

            // 아이템 정보 연결
            ItemPickup pickup = obj.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.itemData = item;
            }

            // 물리 반동 효과
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(dropPoint.forward * 2f + Vector3.up * 1.5f, ForceMode.Impulse);
            }

            // 일정 시간 후 자동 제거 (선택 사항)
            Destroy(obj, 30f);
        }
    }
}