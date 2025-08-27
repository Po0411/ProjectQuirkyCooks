// Assets/Scripts/InventoryManager.cs
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class InventoryManager : NetworkBehaviour
{
    public InventorySlot[] slots;
    public int selectedIndex = 0;

    [Tooltip("아이템 마스터 리스트(이름→데이터 조회용)")]
    public List<ItemData> allItems = new List<ItemData>();

    // ---- 권한 판별(싱글/네트워크 공통) ----
    bool HasLocalAuthority
    {
        get
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
                return true;             // 싱글(네트워크 미시작)
            if (!IsSpawned)
                return true;             // 씬 배치, 미스폰
            return IsOwner;              // 일반적인 오너 체크
        }
    }

    void Start()
    {
        if (slots != null && slots.Length > 0)
            UpdateSlotHighlight();
    }

    void Update()
    {
        if (!HasLocalAuthority || slots == null) return;

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

        for (int i = 0; i < slots.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedIndex = i;
                UpdateSlotHighlight();
            }
        }
    }

    // ---- 슬롯 UI ----
    public void UpdateSlotHighlight()
    {
        if (slots == null) return;
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

    public void InitializeSlots(InventorySlot[] newSlots)
    {
        slots = newSlots;
        selectedIndex = 0;
        UpdateSlotHighlight();
    }

    // ---- 로컬 처리 ----
    public bool AddItemLocal(ItemData newItem)
    {
        if (newItem == null || slots == null) return false;

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
        if (item == null || slots == null) return false;

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

    // ---- 공용 API(싱글/멀티 자동 분기) ----
    public void AddItem(string itemName)
    {
        if (!TryGetItem(itemName, out var item)) return;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && IsServer)
        {
            AddItemClientRpc(itemName);
        }
        else
        {
            AddItemLocal(item);
        }
    }

    public void RemoveItem(string itemName, int amount = 1)
    {
        if (!TryGetItem(itemName, out var item)) return;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && IsServer)
        {
            RemoveItemClientRpc(itemName, amount);
        }
        else
        {
            RemoveItemLocal(item, amount);
        }
    }

    // ---- ClientRpc(멀티 전용) ----
    [ClientRpc]
    void AddItemClientRpc(string itemName)
    {
        if (TryGetItem(itemName, out var item))
            AddItemLocal(item);
    }

    [ClientRpc]
    void RemoveItemClientRpc(string itemName, int amount)
    {
        if (TryGetItem(itemName, out var item))
            RemoveItemLocal(item, amount);
    }

    // ---- 데이터 조회 ----
    public bool TryGetItem(string itemName, out ItemData data)
    {
        data = null;
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning("❌ 빈 itemName 요청");
            return false;
        }

        foreach (var item in allItems)
        {
            if (item != null && item.itemName == itemName)
            {
                data = item;
                return true;
            }
        }

        Debug.LogError($"❌ '{itemName}' 아이템을 allItems에서 찾을 수 없음");
        return false;
    }

    // ===== 레거시 호환용 메서드(기존 코드 유지) =====
    // DropManager, CookingStation 등에서 여전히 호출하는 경우 대비
    public ItemData GetItemByName(string itemName)
    {
        TryGetItem(itemName, out var data);
        return data;
    }
}
