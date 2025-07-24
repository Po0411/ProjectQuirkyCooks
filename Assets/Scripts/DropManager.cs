using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropManager : MonoBehaviour
{
    public GameObject dropPrefab; // 드랍될 아이템 프리팹 (ItemPickup이 붙어 있어야 함)
    public InventoryManager inventory;
    public Transform dropPoint; // 드랍될 위치 (카메라 앞 or 플레이어 발밑)

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ItemData item = inventory.GetSelectedItem();
            if (item == null) return;

            // 인벤토리에서 제거
            bool removed = inventory.RemoveItem(item, 1);
            if (!removed) return;

            // 드랍 오브젝트 생성
            GameObject obj = Instantiate(dropPrefab, dropPoint.position, Quaternion.identity);
            ItemPickup pickup = obj.GetComponent<ItemPickup>();
            pickup.itemData = item;
        }
    }
}