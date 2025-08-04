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
        if (IsOwner && slots != null && slots.Length > 0)
            UpdateSlotHighlight();
    }

    void Update()
    {
        if (!IsOwner || slots == null) return;

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

    public void UpdateSlotHighlight()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].SetSelected(i == selectedIndex);
    }

    public ItemData GetSelectedItem()
    {
        if (slots == null || slots.Length == 0) return null;
        if (selectedIndex >= 0 && selectedIndex < slots.Length)
            return slots[selectedIndex].GetCurrentItem();
        return null;
    }

    // ---------------- 로컬 전용 ----------------
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

    public bool RemoveItemLocal(ItemData item, int amount)
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

    // ---------------- 네트워크 반영 ----------------
    [ClientRpc]
    public void AddItemClientRpc(string itemName)
    {
        var item = GetItemByName(itemName);
        if (item != null)
            AddItemLocal(item);
    }

    [ClientRpc]
    public void RemoveItemClientRpc(string itemName, int amount)
    {
        var item = GetItemByName(itemName);
        if (item != null)
            RemoveItemLocal(item, amount);
    }

    // ---------------- 데이터 조회 ----------------
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

    public void InitializeSlots(InventorySlot[] newSlots)
    {
        slots = newSlots;
        selectedIndex = 0;
        UpdateSlotHighlight();
    }
}
