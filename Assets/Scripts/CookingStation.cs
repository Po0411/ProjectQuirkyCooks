using UnityEngine;
using Unity.Netcode;

public enum CookingType { Boil, Fry, Chop, Blend }

public class CookingStation : NetworkBehaviour, IInteractable
{
    [Header("조리 도구 설정")]
    public CookingType type;
    public string requiredItemName;
    public string resultItemName;

    public string GetInteractText()
    {
        switch (type)
        {
            case CookingType.Boil: return "● 끓이기";
            case CookingType.Fry: return "● 튀기기";
            case CookingType.Chop: return "● 썰기";
            case CookingType.Blend: return "● 갈기기";
        }
        return "사용하기";
    }

    // InventoryManager를 매개변수로 받는 Interact
    public void Interact(InventoryManager inventory)
    {
        if (inventory == null) return;

        ItemData input = inventory.GetItemByName(requiredItemName);
        if (input == null)
        {
            Debug.Log($"❌ {requiredItemName} 없음");
            return;
        }

        if (!inventory.RemoveItemLocal(input, 1))
        {
            Debug.Log($"❌ {requiredItemName} 제거 실패");
            return;
        }

        ItemData output = inventory.GetItemByName(resultItemName);
        if (output != null)
        {
            inventory.AddItemLocal(output);
            Debug.Log($"✅ {requiredItemName} → {resultItemName} 변환 완료");
        }
        else
        {
            Debug.LogError($"❌ {resultItemName} 아이템 데이터를 찾을 수 없음");
        }
    }

    // 매개변수 없는 Interact (인터페이스 요구사항 충족)
    public void Interact()
    {
        InventoryManager inv = FindObjectOfType<InventoryManager>();
        if (inv != null)
            Interact(inv);
        else
            Debug.LogError("❌ InventoryManager를 찾을 수 없음");
    }
}
