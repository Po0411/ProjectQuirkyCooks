using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Slots")]
    public InventorySlot[] slots;
    public int selectedIndex = 0;

    [Header("아이템 데이터베이스")]
    public List<ItemData> allItems; // ✅ Inspector에서 모든 ItemData 등록

    void Start()
    {
        UpdateSlotHighlight();
    }

    void Update()
    {
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

    void UpdateSlotHighlight()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].SetSelected(i == selectedIndex);
    }

    public void AddItem(ItemData newItem)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.Matches(newItem) && !slot.IsFull())
            {
                slot.AddCount();
                return;
            }
        }
        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(newItem);
                return;
            }
        }

        Debug.Log("인벤토리 가득 참");
    }

    public bool HasItem(ItemData item, int amount)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.Matches(item) && slot.GetCount() >= amount)
                return true;
        }
        return false;
    }

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

    public ItemData GetSelectedItem()
    {
        if (selectedIndex >= 0 && selectedIndex < slots.Length)
            return slots[selectedIndex].GetCurrentItem();
        return null;
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        selectedIndex = index;
        UpdateSlotHighlight();
    }

    public ItemData GetItemByName(string itemName)
    {
        foreach (var item in allItems) // ✅ Inspector에 등록된 ItemData 중 검색
        {
            if (item != null && item.itemName == itemName)
                return item;
        }
        Debug.LogWarning($"❌ {itemName} ItemData를 allItems에서 찾지 못했습니다.");
        return null;
    }
}
