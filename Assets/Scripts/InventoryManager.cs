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

    // 슬롯 하이라이트 갱신
    public void UpdateSlotHighlight()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].SetSelected(i == selectedIndex);
    }

    // 현재 선택된 아이템 반환
    public ItemData GetSelectedItem()
    {
        if (selectedIndex >= 0 && selectedIndex < slots.Length)
            return slots[selectedIndex].GetCurrentItem();
        return null;
    }

    // 로컬 인벤토리에 아이템 추가
    public void AddItemLocal(ItemData newItem)
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
        Debug.Log("⚠ 인벤토리 가득 참");
    }

    // 서버에서 아이템 추가 (Host 중복 방지)
    [ServerRpc(RequireOwnership = false)]
    public void AddItemServerRpc(string itemName, ulong senderId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(senderId, out var client))
        {
            Debug.LogError($"❌ {senderId}번 클라이언트 없음");
            return;
        }

        var player = client.PlayerObject;
        var inv = player.GetComponent<InventoryManager>();
        var item = inv.GetItemByName(itemName);

        if (item != null)
        {
            // 서버에서 먼저 추가 (Host 인벤토리 포함)
            inv.AddItemLocal(item);

            // Host라면 ClientRpc를 보내지 않음 (중복 방지)
            if (!(IsHost && client.ClientId == NetworkManager.Singleton.LocalClientId))
            {
                UpdateInventoryClientRpc(itemName, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { client.ClientId }
                    }
                });
            }
        }
    }

    [ClientRpc]
    public void UpdateInventoryClientRpc(string itemName, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return;

        var item = GetItemByName(itemName);
        if (item != null)
        {
            AddItemLocal(item);
            Debug.Log($"✅ {itemName} 인벤토리에 추가됨");
        }
    }

    [ClientRpc]
    public void RefreshInventoryClientRpc()
    {
        if (!IsOwner) return;
        UpdateSlotHighlight();
        Debug.Log("✅ 인벤토리 UI 갱신 완료");
    }

    // 아이템 제거
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
