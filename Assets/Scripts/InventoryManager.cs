using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class InventoryManager : NetworkBehaviour
{
    public InventorySlot[] slots;
    public int selectedIndex = 0;
    public List<ItemData> allItems = new List<ItemData>();

    void Start()
    {
        if (IsOwner)
            UpdateSlotHighlight();
    }

    void Update()
    {
        if (!IsOwner) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            selectedIndex = (selectedIndex + 1) % slots.Length;
            UpdateSlotHighlight();
        }
        else if (scroll < 0f)
        {
            selectedIndex = (selectedIndex - 1 + slots.Length) % slots.Length;
            UpdateSlotHighlight();
        }
    }

    // 슬롯 하이라이트
    public void UpdateSlotHighlight()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].SetSelected(i == selectedIndex);
    }

    // 현재 선택된 아이템
    public ItemData GetSelectedItem()
    {
        if (selectedIndex >= 0 && selectedIndex < slots.Length)
            return slots[selectedIndex].GetCurrentItem();
        return null;
    }

    // 아이템 추가 (로컬)
    public bool AddItemLocal(ItemData newItem)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.Matches(newItem) && !slot.IsFull())
            {
                slot.AddCount();
                return true;
            }
        }
        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(newItem);
                return true;
            }
        }
        return false;
    }

    // 아이템 제거 (로컬)
    public bool RemoveItem(ItemData item, int amount)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.Matches(item))
            {
                slot.RemoveCount(amount);
                return true;
            }
        }
        return false;
    }

    // 이름으로 아이템 찾기
    public ItemData GetItemByName(string itemName)
    {
        foreach (var item in allItems)
        {
            if (item.itemName == itemName)
                return item;
        }
        Debug.LogError($"❌ {itemName} 아이템을 allItems에서 찾을 수 없음");
        return null;
    }
}
